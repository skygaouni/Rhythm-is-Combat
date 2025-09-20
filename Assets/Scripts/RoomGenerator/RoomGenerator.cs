using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.AI;
using Unity.AI.Navigation;
using System.Threading;
using RVO;

public class RoomGenerator : MonoBehaviour
{
    public enum Direction { up, down, left, right };
    public Direction direction;

    [Header("玩家資訊")]
    public Transform player; // 拖你的主角進來

    [Header("房間信息")]
    public List<GameObject> roomPrefabs = new List<GameObject>();
    public int roomNumber;
    public int[] roomDoor;
    public Color startColor, endColor;

    [Header("位置控制")]
    public Transform generatorPoint;
    public float xOffset;
    public float yOffset;
    public float zOffset;
    public LayerMask roomLayer;
    public Vector3 lastRoomPos; // 記住上一次的房間位置

    public List<GameObject> rooms = new List<GameObject>();

    [Header("敵人管理")]
    public List<GameObject> enemyPrefab; // 支援多個敵人種類
    public List<GameObject> spawnedEnemies = new List<GameObject>();
    private int SpawnNumInRoom = 3;

    [Header("導航烘焙")]
    public NavMeshSurface navMeshSurface;

    private bool reset = false;

    // Start is called before the first frame update
    void Start()
    {
        Simulator.Instance.Clear();
        RVOAgentWithNav.initialized = true;

        rooms.Add(Instantiate(roomPrefabs[0], lastRoomPos, Quaternion.identity));
        roomDoor = new int[roomNumber];
        for (int i = 0; i < roomNumber - 1; i++)
        {
            ChangePointPos();

            GameObject newRoom = Instantiate(roomPrefabs[0], lastRoomPos, Quaternion.identity);
            rooms.Add(newRoom);
            // Debug.Log(lastRoomPos);
        }

        // regenerate corresponding room
        for(int i = 0; i < roomNumber; i++)
        {
            Vector3 oldPosition = rooms[i].transform.position;
            Destroy(rooms[i]);
            GameObject newRoom = Instantiate(roomPrefabs[roomDoor[i]], oldPosition, Quaternion.identity);
            rooms[i] = newRoom;
        }


        // 生成完後，統一給每個房間編號
        for (int i = 0; i < rooms.Count; i++)
        {
            var numberTransform = rooms[i].transform.Find("RoomNumber");
            if (numberTransform != null)
            {
                var textMesh = numberTransform.GetComponent<TextMeshPro>();
                if (textMesh != null)
                {
                    textMesh.text = i.ToString();
                }
                else
                {
                    Debug.LogWarning("RoomNumber物件上找不到TextMeshPro元件！");
                }
            }
            else
            {
                Debug.LogWarning("找不到RoomNumber子物件！");
            }
        }

        if (navMeshSurface != null)
        {
            Debug.Log("延遲 0.5 秒後烘焙 NavMesh（使用 Invoke）");
            // StartCoroutine(DoSomethingAfterDelay());
            // Thread.Sleep(500);
            // Invoke(nameof(BakeNavMeshNow), 0f); // 延遲 0.5 秒執行
            // BakeNavMeshNow();
            StartCoroutine(BakeMeshAndResetPosition());
        }
        else
        {
            Debug.LogWarning("NavMeshSurface 尚未連結，無法自動烘焙！");
        }

        foreach (var room in rooms)
        {
            var spawnParent = room.transform.Find("EnemySpawns");
            if (spawnParent != null)
            {
                List<Transform> spawnPoints = new List<Transform>();
                foreach (Transform child in spawnParent)
                {
                    spawnPoints.Add(child);
                }

                for (int i = 0; i < spawnPoints.Count; i++)
                {
                    int randIndex = UnityEngine.Random.Range(i, spawnPoints.Count);
                    (spawnPoints[i], spawnPoints[randIndex]) = (spawnPoints[randIndex], spawnPoints[i]);
                }

                for (int i = 0; i < Mathf.Min(SpawnNumInRoom, spawnPoints.Count); i++)
                {
                    int randomEnemyIndex = UnityEngine.Random.Range(0, enemyPrefab.Count);
                    GameObject enemyToSpawn = enemyPrefab[randomEnemyIndex];

                    GameObject enemyInstance = Instantiate(enemyToSpawn, spawnPoints[i].position, Quaternion.identity, room.transform);
                    spawnedEnemies.Add(enemyInstance); // ✅ 存進 List
                }
            }
            else
            {
                Debug.LogWarning($"房間 {room.name} 沒有找到 EnemySpawns 物件！");
            }
        }
        
    }


    // Update is called once per frame
    float enemyCheckInterval = 0.5f;
    float enemyCheckTimer = 0f;

    float roomCheckInterval = 1f;
    float roomCheckTimer = 0f;

    float enemyActiveRange = 52f;
    float roomActiveRange = 104f;

    void Update()
    {
        if(!reset)
        {
            reset = true;
            for (int i = 0; i < spawnedEnemies.Count; i++)
            {
                spawnedEnemies[i].transform.position = spawnedEnemies[i].GetComponent<FSM>().parameter.spawnPosition;
            }
        }

        // ✅ 檢查敵人距離（每 0.5 秒一次）
        enemyCheckTimer += Time.deltaTime;
        if (enemyCheckTimer >= enemyCheckInterval)
        {
            enemyCheckTimer = 0;

            foreach (GameObject enemy in spawnedEnemies)
            {
                if (enemy == null) continue;

                float distance = Vector3.Distance(player.position, enemy.transform.position);
                bool shouldBeActive = distance <= enemyActiveRange;

                if (enemy.activeSelf != shouldBeActive)
                {
                    enemy.SetActive(shouldBeActive);
                }
            }
        }

        // ✅ 檢查房間距離（每 1 秒一次）
        roomCheckTimer += Time.deltaTime;
        if (roomCheckTimer >= roomCheckInterval)
        {
            roomCheckTimer = 0f;

            foreach (GameObject room in rooms)
            {
                if (room == null) continue;

                float distance = Vector3.Distance(player.position, room.transform.position);
                bool shouldBeActive = distance <= roomActiveRange;

                if (room.activeSelf != shouldBeActive)
                {
                    room.SetActive(shouldBeActive);
                }
            }
        }
    }



    public void ChangePointPos()
    {
        Vector3 newPos = Vector3.zero;
        bool positionFound = false;
        int attempts = 0;
        int maxAttempts = 100; // 每個房間嘗試100次
        int backStep = 1; // 一開始是回到倒數第1間

        while (!positionFound)
        {
            // 先確保index不會超過範圍
            int index = Mathf.Max(rooms.Count - backStep, 0);
            Vector3 basePos = rooms[index].transform.position;

            direction = (Direction)UnityEngine.Random.Range(0, 4);
            switch (direction)
            {
                case Direction.up:
                    newPos = basePos + new Vector3(0, 0, zOffset);
                    break;
                case Direction.down:
                    newPos = basePos + new Vector3(0, 0, -zOffset);
                    break;
                case Direction.left:
                    newPos = basePos + new Vector3(-xOffset, 0, 0);
                    break;
                case Direction.right:
                    newPos = basePos + new Vector3(xOffset, 0, 0);
                    break;
            }

            // 檢查這個新位置是否可以生成
            if (Physics.OverlapSphere(newPos, 0.2f, roomLayer).Length == 0)
            {
                positionFound = true;
                lastRoomPos = newPos;
                switch (direction)
                {
                    case Direction.up:
                        roomDoor[index] = roomDoor[index] | 8;
                        roomDoor[rooms.Count] = roomDoor[rooms.Count] | 2;
                        break;
                    case Direction.down:
                        roomDoor[index] = roomDoor[index] | 2;
                        roomDoor[rooms.Count] = roomDoor[rooms.Count] | 8;
                        break;
                    case Direction.left:
                        roomDoor[index] = roomDoor[index] | 1;
                        roomDoor[rooms.Count] = roomDoor[rooms.Count] | 4;
                        break;
                    case Direction.right:
                        roomDoor[index] = roomDoor[index] | 4;
                        roomDoor[rooms.Count] = roomDoor[rooms.Count] | 1;
                        break;
                }
            }
            else
            {
                attempts++;

                if (attempts >= maxAttempts)
                {
                    backStep++; // 試太多次了，回溯到更前一間房間
                    attempts = 0; // 重置次數

                    if (backStep > rooms.Count)
                    {
                        Debug.LogWarning("已經回溯到最早的房間了，找不到空位！");
                        break; // 全部回溯完都沒空格，直接跳出避免卡死
                    }
                }
            }
        }
    }
    private void BakeNavMeshNow()
    {
        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh 已烘焙完成！");
    }
    IEnumerator DoSomethingAfterDelay()
    {

        yield return new WaitForSeconds(10.0f);
        // 這裡可以放你真正要延遲後做的事情
        // 例如：BakeNavMeshNow();
    }
    IEnumerator BakeMeshAndResetPosition()
    {

        yield return new WaitForSeconds(0f);
        Debug.Log("BuildNavMesh");
        navMeshSurface.BuildNavMesh();

        yield return new WaitForSeconds(0.1f);

        Debug.Log("ResetPosition");
        for (int i = 0; i < spawnedEnemies.Count; i++)
        {

            spawnedEnemies[i].transform.position = spawnedEnemies[i].GetComponent<FSM>().parameter.spawnPosition;
        }
    }
}

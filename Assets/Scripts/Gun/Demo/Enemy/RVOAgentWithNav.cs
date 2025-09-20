using UnityEngine;
using UnityEngine.AI;
using RVO;
using System.Collections.Generic;
using DG.Tweening;

public class RVOAgentWithNav : MonoBehaviour
{
    public bool isActive = false; // 控制是否啟動

    private Vector3 target; // 目標
    //public Transform player;
    public float repathRate = 1f; // 每幾秒重新計算 NavMesh 路徑
    public float waypointThreshold = 0.3f;
    public float rvoSpeed = 0.03f;

    private int id;
    private List<Vector3> pathPoints = new();
    private int currentPathIndex = 0;
    private float repathTimer = 0f;
    public static bool initialized;

    public LayerMask whatIsGround;
    //private RaycastHit groundHit;
    public GameObject head;
    public GameObject tail;

    private Vector3 oldPos;

    void Start()
    {
        rvoSpeed = Random.Range(0.3f, 0.3f);

        //target = GameObject.FindGameObjectWithTag("Player");
        if (!initialized || Simulator.Instance.getNumAgents() == 0)
        {
            Simulator.Instance.Clear(); // 清除上一次模擬資料

            Simulator.Instance.setTimeStep(0.25f);
            Simulator.Instance.setAgentDefaults(15f, 10, 10f, 10f, 0.5f, rvoSpeed, new RVO.Vector2(0, 0));
            initialized = true;
        }

        id = Simulator.Instance.addAgent(new RVO.Vector2(transform.position.x, transform.position.z));
        Simulator.Instance.setAgentRadius(id, 2f);
        Simulator.Instance.setAgentMaxSpeed(id, rvoSpeed);

        //RecalculatePath();
    }

    void Update()
    {
        //Physics.Raycast(transform.position, Vector3.down, out groundHit ,0.2f, whatIsGround);

        if (!isActive)
        {
            // 停止 agent 的模擬移動
            Simulator.Instance.setAgentPrefVelocity(id, new RVO.Vector2(0, 0));

            // 確保模擬器位置不會亂飄
            RVO.Vector2 syncPos = new RVO.Vector2(transform.position.x, transform.position.z);
            Simulator.Instance.setAgentPosition(id, syncPos);
            return;
        }

        //SetTarget(player.position);

        repathTimer += Time.deltaTime;
        if (repathTimer >= repathRate)
        {
            RecalculatePath();
            repathTimer = 0f;
        }

        if (pathPoints.Count == 0) return;

        Vector3 pos = transform.position;
        Vector3 goal = pathPoints[currentPathIndex];
        Vector3 dir3D = (goal - pos);
        dir3D.y = 0;

        if (dir3D.magnitude < waypointThreshold && currentPathIndex < pathPoints.Count - 1)
            currentPathIndex++;

        RVO.Vector2 dir = new (dir3D.x, dir3D.z);
        if (RVOMath.absSq(dir) > rvoSpeed * rvoSpeed)
        {
            dir = RVOMath.normalize(dir) * rvoSpeed;
        }

        Simulator.Instance.setAgentPrefVelocity(id, dir);

        
    }

    void LateUpdate()
    {
        if (!isActive) return;

        Vector3 nowPos = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);

        RVO.Vector2 newPos = Simulator.Instance.getAgentPosition(id);
        Vector3 flatPos = new Vector3(newPos.x(), transform.position.y + 1f, newPos.y());

        Vector3 headOffset = head.transform.position - transform.position;
        Vector3 tailOffset = tail.transform.position - transform.position;
        Vector3 newHeadPos = flatPos + headOffset;
        Vector3 newTailPos = flatPos + tailOffset;

        Vector3 newHedPosProjectOnPlane = new Vector3(newHeadPos.x, transform.position.y, newHeadPos.z);

        bool hasHeadHit = Physics.Raycast(newHeadPos, Vector3.down, out RaycastHit headHit, 10f, whatIsGround);
        bool hasTailHit = Physics.Raycast(newTailPos, Vector3.down, out RaycastHit tailHit, 10f, whatIsGround);
        bool frontHit = Physics.Raycast(newHedPosProjectOnPlane, transform.forward, 0.3f, whatIsGround);

        // 前面沒有東西且站在地面上
        if (!frontHit && hasHeadHit && hasTailHit && Mathf.Abs(headHit.point.y - tailHit.point.y) < 0.01f) 
        {
            transform.position = new Vector3(newPos.x(), headHit.point.y , newPos.y());
        }
        else
        {
            //保持 Y 不變
            float maxHeight;
            // maxHeight = (headHit.point.y + tailHit.point.y) / 2; 
            maxHeight = headHit.point.y > tailHit.point.y ? headHit.point.y : tailHit.point.y;
            transform.position = new Vector3(newPos.x(), maxHeight, newPos.y());

        }

        
    }

    public void Activate()
    {
        isActive = true;
        RecalculatePath();
    }

    public void Deactivate()
    {
        isActive = false;
        RecalculatePath();
    }

    public void SetTarget(Vector3 newTarget)
    {
        target = newTarget;
        //RecalculatePath(); // 設定完後自動重新計算路徑（選用）
    }


    void RecalculatePath()
    {
        NavMeshPath navPath = new();

        if (NavMesh.CalculatePath(transform.position, target, NavMesh.AllAreas, navPath))
        {
            pathPoints = new List<Vector3>(navPath.corners);
            currentPathIndex = 0;

            // ✅ 顯示 NavMesh 路徑
            for (int i = 0; i < navPath.corners.Length - 1; i++)
            {
                Debug.DrawLine(navPath.corners[i], navPath.corners[i + 1], Color.yellow, 1f); // cyan 線表示 NavMesh path
            }

            // ✅ 顯示路徑終點（用一條向上的線或一個十字）
            if (navPath.corners.Length > 0)
            {
                Vector3 endPoint = navPath.corners[navPath.corners.Length - 1];
                Debug.DrawRay(endPoint + Vector3.up * 0.1f, Vector3.up * 0.5f, Color.red, 1f); // 向上的紅線
            }
        }
    }

    void OnApplicationQuit()
    {
        Simulator.Instance.Clear();
        initialized = false;
    }

}

using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class Spawner : MonoBehaviour
{
    string filePath;

    public AudioSource myMusic; // 音樂 source

    private List<float> beatTimes = new List<float>(); // 用於存放時間戳記
    private int beatIndex = 0;

    public float noteTravelTime = 1.0f; // 音符從出生位置到打擊位置所需秒數

    public GameObject spritePrefab;
    public Vector3 spawnPosition;

    public GameObject spritePrefab2;
    public Vector3 spawnPosition2;

    private bool isSpawning = false;
    private double musicStartDspTime;

    private void Start()
    {
        filePath = Path.Combine(Application.dataPath, "Logs/BeatMap.txt");
        File.WriteAllText(filePath, "");
    }

    void Update()
    {
        if (!isSpawning || beatIndex >= beatTimes.Count) return;

        double currentTime = AudioSettings.dspTime - musicStartDspTime;

        while (beatIndex < beatTimes.Count && currentTime >= beatTimes[beatIndex] - noteTravelTime)
        {
            Instantiate(spritePrefab, spawnPosition, spritePrefab.transform.rotation);
            Instantiate(spritePrefab2, spawnPosition2, spritePrefab2.transform.rotation);

            string log = $"{(Time.time + 1.0f):F3}";
            File.AppendAllText(filePath, log + "\n");

            beatIndex++;
        }
    }

    public void LoadBeatData(string path)
    {
        beatTimes.Clear();
        string[] lines = System.IO.File.ReadAllLines(path);
        foreach (string line in lines)
        {
            if (float.TryParse(line, out float beatTime))
            {
                beatTimes.Add(beatTime);
            }
        }
    }

    public void StartSpawning(double dspStartTime)
    {
        Debug.Log("Start Playing!");
        musicStartDspTime = dspStartTime;
        beatIndex = 0;
        isSpawning = true;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}

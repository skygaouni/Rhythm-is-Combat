using System;
using System.IO;
using UnityEngine;

public class BulletDiagram : MonoBehaviour
{
    string filePath;

    public Animator animator;

    public AudioSource audioSource;


    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "Scripts/Gun/click_log.txt");
        File.WriteAllText(filePath, "");
    }

    public void recordPressShootKey(string whatHit)
    {
        if (audioSource.isPlaying)
        {
            Debug.Log("record");
            string log = $"{whatHit}, clicked at {Time.time:F3} seconds";
            File.AppendAllText(filePath, log + "\n");
            //Debug.Log(log);
        }

    }
}

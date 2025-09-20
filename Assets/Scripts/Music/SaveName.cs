using System.IO;
using UnityEngine;

public class SaveName : MonoBehaviour
{
    public AudioSource audioSource;

    void OnApplicationQuit()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            string clipName = audioSource.clip.name;
            string savePath = Path.Combine(Application.dataPath, "Clip.txt");
            File.WriteAllText(savePath, clipName);
            Debug.Log("已將 clip name 寫入：" + clipName);
        }
    }
}

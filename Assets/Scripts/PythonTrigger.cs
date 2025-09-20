using UnityEngine;
using System.Diagnostics;
using System.IO;

public class PythonTrigger : MonoBehaviour
{
    // 相對路徑：Assets/Scripts/BulletClick.py
    public string relativePath = @"Scripts/BulletClick.py";

    public AudioSource audioSource;

    public void RunPython()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            UnityEngine.Debug.LogWarning("⚠️ AudioSource 或 AudioClip 未設定");
            return;
        }

        string clipName = audioSource.clip.name;
        string fullScriptPath = Path.Combine(Application.dataPath, relativePath);

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "python";
        psi.Arguments = $"\"{fullScriptPath}\" {clipName}"; // ✅ 把 clip name 傳給 Python
        psi.WorkingDirectory = Application.dataPath;         // ✅ 確保 Python 執行位置正確
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        try
        {
            Process.Start(psi);
            UnityEngine.Debug.Log($"✅ 執行 Python 腳本：{fullScriptPath}，傳入參數 clipName：{clipName}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("❌ Python 啟動失敗：" + e.Message);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            RunPython();
        }
    }
}
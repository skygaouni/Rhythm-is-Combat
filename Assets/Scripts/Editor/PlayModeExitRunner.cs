#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.IO;

[InitializeOnLoad]
public class PlayModeExitRunner
{
    static PlayModeExitRunner()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            RunPythonScript();
        }
    }

    static void RunPythonScript()
    {
        string relativePyPath = @"Scripts/BulletClick.py";
        string fullScriptPath = Path.Combine(Application.dataPath, relativePyPath).Replace("\\", "/");
        UnityEngine.Debug.Log(fullScriptPath);

        // 嘗試讀取 clip 名稱
        string clipNamePath = Path.Combine(Application.dataPath, "Clip.txt");
        string clipName = "unknown_clip";
        if (File.Exists(clipNamePath))
        {
            clipName = File.ReadAllText(clipNamePath).Trim();
            UnityEngine.Debug.Log("✅ Clip 名稱已讀取：" + clipName);
        }
        else
        {
            UnityEngine.Debug.LogWarning("⚠️ 找不到 ClipName.txt，將使用預設值");
        }

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "python";
        psi.Arguments = $"\"{fullScriptPath}\" {clipName}";
        psi.WorkingDirectory = Path.GetDirectoryName(fullScriptPath);
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;

        try
        {
            Process p = Process.Start(psi);
            string output = p.StandardOutput.ReadToEnd();
            string error = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (!string.IsNullOrWhiteSpace(output))
                UnityEngine.Debug.Log("✅ Python 輸出：\n" + output);

            if (!string.IsNullOrWhiteSpace(error))
                UnityEngine.Debug.LogError("❌ Python 錯誤：\n" + error);
            else
                UnityEngine.Debug.Log("✅ Python 腳本執行成功：" + clipName);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("❌ 無法啟動 Python：" + e.Message);
        }
    }
}
#endif

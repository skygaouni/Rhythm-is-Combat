using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MusicBrowser : MonoBehaviour
{
    private string musicFolderPath = Application.dataPath + "/Resources/Audio"; // 改成你要讀的資料夾
    public TMP_Dropdown dropdown;  // 指向 Unity Inspector 中的 Dropdown
    public AudioSource audioSource;

    private List<string> musicFiles = new List<string>();

    void Start()
    {
        LoadMusicFiles();
        PopulateDropdown();
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
    }

    void LoadMusicFiles()
    {
        string[] extensions = { "*.mp3", "*.wav", "*.flac" };
        musicFiles.Clear();

        foreach (string ext in extensions)
        {
            musicFiles.AddRange(Directory.GetFiles(musicFolderPath, ext));
        }
    }

    void PopulateDropdown()
    {
        dropdown.ClearOptions();

        List<string> options = new List<string>();
        foreach (string filePath in musicFiles)
        {
            options.Add(Path.GetFileName(filePath));
        }

        dropdown.AddOptions(options);
        LoadAudioFromFile(musicFiles[0]);
    }

    void OnDropdownValueChanged(int index)
    {
        Player.instance.Pause();
        string selectedPath = musicFiles[index];
        LoadAudioFromFile(selectedPath);
        // StartCoroutine(PlayAudioFromFile(selectedPath));
    }

    IEnumerator PlayAudioFromFile(string path)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                // audioSource.Play();
            }
        }
    }
    void LoadAudioFromFile(string path)
    {
        string nameOnly = Path.GetFileNameWithoutExtension(path);
        audioSource.clip = Resources.Load<AudioClip>("Audio/" + nameOnly);
    }


    AudioType GetAudioType(string path)
    {
        string ext = Path.GetExtension(path).ToLower();
        switch (ext)
        {
            case ".mp3": return AudioType.MPEG;
            case ".wav": return AudioType.WAV;
            case ".flac": return AudioType.OGGVORBIS; // FLAC 載入支援可能需特別 plugin
            default: return AudioType.UNKNOWN;
        }
    }
}

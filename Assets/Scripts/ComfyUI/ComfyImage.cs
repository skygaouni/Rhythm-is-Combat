using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Collections;
using UnityEditor;
using System.IO;
using UnityEngine.Events;
using TMPro;

//[System.Serializable]
/*public class ImageData
{
    public string filename; //檔案名稱
    public string subfolder; //路徑
    public string type; //類型
}*/

/*[System.Serializable]
public class OutputData
{
    public ImageData[] images; // 一個請求可以有多張圖片
}*/

/*[System.Serializable]
public class PromptData
{
    public OutputData outputs;
}*/
public class ComfyImage : MonoBehaviour
{
    
    private string FilePath = "D:\\Tools\\ComfyUI\\ComfyUI_windows_portable\\ComfyUI\\output\\audio\\";
   // private string FilePath = "D:\\comfyui\\ComfyUI_windows_portable_backup\\ComfyUI_windows_portable\\ComfyUI\\output\\audio\\"; // 本地檔案路徑
    [SerializeField] public AudioSource audioSource;
    [SerializeField] public AudioClip audioClip;
    [SerializeField] public UnityEvent StartBeatDetector;
    public TMP_InputField newFileNameInput;
    public GameObject backbutton;
    public GameObject progress;

    void Start()
    {
        // audioSource = gameObject.GetComponent<AudioSource>();
    }
    public void RequestFileName(string id)
    {
        //協程是一種特殊的函數，允許程式在執行過程中暫停，然後在下一幀或條件滿足時繼續執行。
        StartCoroutine(RequestFileNameRoutine(id));
    }
    IEnumerator RequestFileNameRoutine(string promptID)
    {
        string url = "http://127.0.0.1:8188/history/" + promptID;

        //發送 GET 請求到 http://127.0.0.1:8188/history/<promptID>
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // yield return 讓 Unity 等待伺服器回應，不會阻塞主執行緒
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    Debug.Log(":\nReceived: " + webRequest.downloadHandler.text);
                    string filename = ExtractFilename(webRequest.downloadHandler.text);
                    string extension = Path.GetExtension(filename).ToLower(); // 確保副檔名是小寫
                    string imageURL = "http://127.0.0.1:8188/view?filename=" + filename;
                    // string savepath = Application.dataPath;
                    // Debug.Log("savepath: " + savepath);
                    if (extension == ".flac")
                    {
                        string destinationFolder = Path.Combine(Application.dataPath, "Resources/Audio");
                        string destinationPath = Path.Combine(destinationFolder, filename);
                        // string destinationPath = Path.Combine(Application.dataPath, "Resources/Audio", filename);
                        Debug.Log("savepath: " + destinationPath);

                        if (File.Exists(FilePath + filename))
                        {
                            File.Copy(FilePath + filename, destinationPath, overwrite: true);
                            Debug.Log($"文件已复制到: {destinationPath}");

                            string userInputNewName = newFileNameInput.text.Trim(); // 順便去除空白

                            if (!string.IsNullOrEmpty(userInputNewName))
                            {
                                // 如果有輸入新檔名，就改名字（自動加上 .flac 副檔名）
                                string newFileName = userInputNewName + ".flac";
                                string newDestinationPath = Path.Combine(destinationFolder, newFileName);

                                // 如果新檔案已經存在，先刪除
                                if (File.Exists(newDestinationPath))
                                {
                                    File.Delete(newDestinationPath);
                                }

                                File.Move(destinationPath, newDestinationPath);

                                Debug.Log($"文件已重新命名為: {newDestinationPath}");

#if UNITY_EDITOR
                                AssetDatabase.Refresh();
#endif
                                backbutton.SetActive(true);
                                progress.SetActive(false);
                                LoadAudioFromFile(newDestinationPath);
                            }
                            else
                            {
                                // 如果沒輸入，就直接用原本的檔案
#if UNITY_EDITOR
                                AssetDatabase.Refresh();
#endif
                                backbutton.SetActive(true);
                                progress.SetActive(false);
                                LoadAudioFromFile(destinationPath);
                            }
                        }
                        else
                        {
                            Debug.LogError("源文件不存在：" + FilePath + filename);
                        }

                    }
                    else
                    {
                        Debug.LogWarning("不支援的檔案類型: " + extension);
                    }
                    break;
            }
        }
    }
    string ExtractFilename(string jsonString)
    {
        // Step 1: Identify the part of the string that contains the "filename" key
        string keyToLookFor = "\"filename\":";
        int startIndex = jsonString.IndexOf(keyToLookFor);

        if (startIndex == -1)
        {
            return "filename key not found";
        }

        // Adjusting startIndex to get the position right after the keyToLookFor
        startIndex += keyToLookFor.Length;

        // Step 2: Extract the substring starting from the "filename" key
        string fromFileName = jsonString.Substring(startIndex);
        Debug.Log("fromFileName:" + fromFileName);

        // Assuming that filename value is followed by a comma (,)
        int endIndex = fromFileName.IndexOf(',');

        // Extracting the filename value (assuming it's wrapped in quotes)
        string filenameWithQuotes = fromFileName.Substring(0, endIndex).Trim();
        Debug.Log("filenameWithQuotes:" + filenameWithQuotes);

        // Removing leading and trailing quotes from the extracted value
        string filename = filenameWithQuotes.Trim('"');
        Debug.Log(filename);
        return filename;
    }
    public Image outputImage;

    /*IEnumerator DownloadImage(string imageUrl)
    {
        yield return new WaitForSeconds(0.5f);
        using (UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // Get the downloaded texture
                Texture2D texture = ((DownloadHandlerTexture)webRequest.downloadHandler).texture;

                outputImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);

            }
            else
            {
                Debug.LogError("Image download failed: " + webRequest.error);
            }
        }
    }*/

    /*IEnumerator DownloadObj(string objUrl, string savePath)
    {
        yield return new WaitForSeconds(0.5f);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(objUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                // 取得下載的 `.obj` 檔案資料
                byte[] objData = webRequest.downloadHandler.data;

                // 將下載的檔案儲存到指定路徑
                System.IO.File.WriteAllBytes(savePath, objData);
                Debug.Log("OBJ file downloaded and saved to: " + savePath);

                #if UNITY_EDITOR
                                AssetDatabase.Refresh();
                #endif

                // 如果需要立即載入 .obj 模型，可調用相關載入函式
                //StartCoroutine(LoadObjModel(savePath));
            }
            else
            {
                Debug.LogError("OBJ file download failed: " + webRequest.error);
            }
        }
    }
    */
    /*private IEnumerator DownloadFlac(string url, string savePath)
    {
        Debug.Log("Downloading FLAC: " + url);
        UnityWebRequest webRequest = UnityWebRequest.Get(url);
        webRequest.downloadHandler = new DownloadHandlerFile(savePath);

        yield return webRequest.SendWebRequest();

        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("FLAC Download Error: " + webRequest.error);
        }
        else
        {
            Debug.Log("FLAC Downloaded Successfully: " + savePath);
        }
    }*/
    
    IEnumerator LoadAudio(string path)
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
                //audioSource.loop = true;
                //audioSource.Play();
                StartBeatDetector.Invoke();
            }
        }
    }

    void LoadAudioFromFile(string path)
    {
        string nameOnly = Path.GetFileNameWithoutExtension(path);
        audioSource.clip = Resources.Load<AudioClip>("Audio/" + nameOnly);
        audioSource.Play();
        audioSource.loop = true;
    }
    /*void LoadLocalObj(string filePath)
    {
        if (System.IO.File.Exists(filePath))
        {
            // 使用 Runtime OBJ Importer 載入本地模型
            GameObject loadedObject = new OBJLoader().Load(filePath);

            if (loadedObject != null)
            {
                // 設定模型的位置和旋轉
                loadedObject.transform.position = Vector3.zero;
                loadedObject.transform.rotation = Quaternion.identity;

                Debug.Log("OBJ model loaded successfully.");
            }
            else
            {
                Debug.LogError("Failed to load OBJ model.");
            }
        }
        else
        {
            Debug.LogError("OBJ file not found at path: " + filePath);
        }
    }*/
    IEnumerator LoadAndInstantiateAssetCoroutine(string path)
    {
        #if UNITY_EDITOR
                yield return new WaitForSeconds(2f); // 延遲指定時間

                // 從資產資料夾中加載物件
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (asset != null)
                {
                    // 將資產實例化到場景中
                    GameObject instance = Instantiate(asset) as GameObject;
                    instance.transform.position = Vector3.zero; // 設置模型在場景中的位置
                    Debug.Log("Asset instantiated successfully.");
                }
                else
                {
                    Debug.LogError("Asset not found at path: " + path);
                }
        #else
                Debug.LogError("AssetDatabase is only available in the Unity Editor.");
                yield break;
        #endif
    }
    /*IEnumerator DelayedAction(float delayTime)
    {
        Debug.Log($"Waiting for {delayTime} seconds...");
        yield return new WaitForSeconds(delayTime);
        Debug.Log("Action executed!");
    }*/
    /*public void PlayAudioFile()
    {
        audioSource.clip = audioClip;
        audioSource.Play();
        audioSource.loop = true;

    }*/

}

using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // public static UIManager Instance;

    public GameObject startUI;
    public GameObject selectMusicUI;
    public GameObject generateMusicUI;
    public GameObject setDifficultyUI;

    public AudioSource audioSource;

    public TMP_Dropdown dropdown;  // 連到 UI 的 Dropdown
    private string filePath;

    public GameObject beatAnalyzer;
    public GameObject ComfyUIController;

    /*private void Awake()
    {
        // Singleton 初始化（確保只有一個）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }*/

    void Start()
    {
        ShowStartUI();
        // 設定檔案儲存位置：Assets/Log/musicname.txt
        filePath = Path.Combine(Application.dataPath, "Logs/musicname.txt");

        // 如果資料夾不存在就建立
        string folderPath = Path.GetDirectoryName(filePath);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }

    public void ShowStartUI()
    {
        audioSource.Stop();
        startUI.SetActive(true);
        selectMusicUI.SetActive(false);
        generateMusicUI.SetActive(false);
        setDifficultyUI.SetActive(false);
    }

    public void ShowSelectMusicUI()
    {
        startUI.SetActive(false);
        selectMusicUI.SetActive(true);
        generateMusicUI.SetActive(false);
        setDifficultyUI.SetActive(false);
    }

    public void ShowGenerateMusicUI()
    {
        ComfyUIController.SetActive(true);
        startUI.SetActive(false);
        selectMusicUI.SetActive(false);
        generateMusicUI.SetActive(true);
        setDifficultyUI.SetActive(false);
    }
    public void ShowsetDifficultyUI()
    {
        SaveSelectedText();
        audioSource.Pause();
        startUI.SetActive(false);
        selectMusicUI.SetActive(false);
        generateMusicUI.SetActive(false);
        setDifficultyUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    public void LoadGameScene()
    {
        beatAnalyzer.SetActive(true);
        // 載入新的場景，並卸載目前場景
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        // LoadSceneMode.Single 會自動卸載當前場景
    }

    private void SaveSelectedText()
    {
        string selectedText = dropdown.options[dropdown.value].text;
        File.WriteAllText(filePath, selectedText);
        Debug.Log("Saved: " + selectedText);
    }
}

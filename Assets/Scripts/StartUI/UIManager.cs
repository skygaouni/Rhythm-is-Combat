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

    public TMP_Dropdown dropdown;  // �s�� UI �� Dropdown
    private string filePath;

    public GameObject beatAnalyzer;
    public GameObject ComfyUIController;

    /*private void Awake()
    {
        // Singleton ��l�ơ]�T�O�u���@�ӡ^
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
        // �]�w�ɮ��x�s��m�GAssets/Log/musicname.txt
        filePath = Path.Combine(Application.dataPath, "Logs/musicname.txt");

        // �p�G��Ƨ����s�b�N�إ�
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
        // ���J�s�������A�è����ثe����
        SceneManager.LoadScene("SampleScene", LoadSceneMode.Single);
        // LoadSceneMode.Single �|�۰ʨ�����e����
    }

    private void SaveSelectedText()
    {
        string selectedText = dropdown.options[dropdown.value].text;
        File.WriteAllText(filePath, selectedText);
        Debug.Log("Saved: " + selectedText);
    }
}

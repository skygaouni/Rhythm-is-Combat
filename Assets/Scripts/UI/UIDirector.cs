using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIDirector: MonoBehaviour
{
    public GameObject pauseUI; // Drag your PauseUI here in the Inspector
    public Slider volumeSlider;
    public TextMeshProUGUI volumeText;

    public Transform player;  // 拖入你的玩家物件
    public PlayerAttributes playerAttributes;

    void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = savedVolume;
        AudioListener.volume = savedVolume;
        UpdateVolumeText(savedVolume);

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && playerAttributes.isDied == false)
        {
            TogglePauseUI();
        }
    }

    public void TogglePauseUI()
    {
        if (pauseUI != null)
        {
            bool isActive = pauseUI.activeSelf;
            pauseUI.SetActive(!isActive);

            Time.timeScale = isActive ? 1f : 0f;

            AudioListener.pause = !isActive;

            Cursor.visible = !isActive;
            Cursor.lockState = isActive ? CursorLockMode.Locked : CursorLockMode.None;
        }
        else
        {
            Debug.LogWarning("PauseUI reference is missing!");
        }

    }

    public void QuitGame()
    {
        Debug.Log("Loading StartScene...");
        Time.timeScale = 1f; // 確保時間恢復正常
        AudioListener.pause = false; // 恢復音效
        SceneManager.LoadScene("StartScene");
    }
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
        UpdateVolumeText(volume);
    }

    void UpdateVolumeText(float volume)
    {
        // 顯示百分比（如 75%）
        int percent = Mathf.RoundToInt(volume * 100);
        volumeText.text = percent + "%";
    }

    public void ResetPlayerPosition()
    {
        if (player != null)
        {
            player.position = new Vector3(0f, 5f, 0f); // 傳送到 (0, 5, 0)
            player.rotation = Quaternion.identity;     // 可選，轉向重設
            Debug.Log("玩家已重置位置至 (0, 5, 0)");
        }
        else
        {
            Debug.LogWarning("Player 未指定！");
        }
    }
}

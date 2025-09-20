using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraSensitivitySettings : MonoBehaviour
{
    public Slider sensXSlider;
    public Slider sensYSlider;
    public TextMeshProUGUI sensXText;
    public TextMeshProUGUI sensYText;

    public PlayerCam playerCam;

    void Start()
    {
        // 預設值或從 PlayerPrefs 讀取
        float x = PlayerPrefs.GetFloat("SensX", 100f);
        float y = PlayerPrefs.GetFloat("SensY", 100f);

        sensXSlider.value = x;
        sensYSlider.value = y;

        playerCam.sensX = x;
        playerCam.sensY = y;

        UpdateText(sensXText, x);
        UpdateText(sensYText, y);

        sensXSlider.onValueChanged.AddListener(UpdateSensX);
        sensYSlider.onValueChanged.AddListener(UpdateSensY);
    }

    void UpdateSensX(float value)
    {
        playerCam.sensX = value;
        PlayerPrefs.SetFloat("SensX", value);
        UpdateText(sensXText, value);
    }

    void UpdateSensY(float value)
    {
        playerCam.sensY = value;
        PlayerPrefs.SetFloat("SensY", value);
        UpdateText(sensYText, value);
    }

    void UpdateText(TextMeshProUGUI textField, float value)
    {
        textField.text = Mathf.RoundToInt(value).ToString();
    }
}

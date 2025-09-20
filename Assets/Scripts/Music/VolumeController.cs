using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public AudioSource musicSource;   // 拖入你的音樂 AudioSource
    public Slider volumeSlider;       // 拖入 UI Slider
    public TextMeshProUGUI volumeLabel;
    // Start is called before the first frame update
    void Start()
    {
        // 初始化 Slider 值（從現有音量）
        volumeSlider.value = musicSource.volume;

        // 加事件監聽：Slider 每次變動都呼叫 OnVolumeChange
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);
    }
    public void OnVolumeChange(float value)
    {
        musicSource.volume = value;
        volumeLabel.text = Mathf.RoundToInt(value * 100) + "%";
    }

}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    public AudioSource musicSource;   // ��J�A������ AudioSource
    public Slider volumeSlider;       // ��J UI Slider
    public TextMeshProUGUI volumeLabel;
    // Start is called before the first frame update
    void Start()
    {
        // ��l�� Slider �ȡ]�q�{�����q�^
        volumeSlider.value = musicSource.volume;

        // �[�ƥ��ť�GSlider �C���ܰʳ��I�s OnVolumeChange
        volumeSlider.onValueChanged.AddListener(OnVolumeChange);
    }
    public void OnVolumeChange(float value)
    {
        musicSource.volume = value;
        volumeLabel.text = Mathf.RoundToInt(value * 100) + "%";
    }

}

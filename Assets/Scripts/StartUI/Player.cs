using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    public Transform buttons; // ����Ҧ����s�����X��

    public GameObject playBtn, pauseBtn, muteBtn, unmuteBtn;
    // �����B�Ȱ��B�R���B�����R�������s

    // public AudioClip[] audioClips;      // �s��q�����}�C
    public AudioSource audioSource;     // ���ּ��񾹲ե�

    public TextMeshProUGUI volText;                // ���q�ƭ����
    public Slider volSlider;            // ���q�վ�Ʊ�

    public TextMeshProUGUI musicName;              // ��ܺq���W�٩κq��
    public TextMeshProUGUI nowTime;                // ��ܥثe����ɶ�
    public TextMeshProUGUI allTime;                // ��ܾ㭺�q���`����
    public Slider progressSlider;       // ��ܼ���i�ת��Ʊ�

    public int index;                   // ��e�q�������ޡ]�ĴX���^

    public TextMeshProUGUI musicname;

    private int currentHour, currentMinute, currentSecond;
    // �ثe���񪺮ɶ��]�ɡB���B��^

    private int clipHour, clipMinute, clipSecond;
    // �`�ɶ��]�ɡB���B��^

    public static Player instance;      // �R�A�� Player ��ҡ]�i�ΨӦb��L�����s���^

    void Start()
    {
        playBtn.SetActive(true);
        pauseBtn.SetActive(false);
        muteBtn.SetActive(true);
        unmuteBtn.SetActive(false);

        instance = this;

        audioSource.Stop(); // �}�l�B��ɤ�����A�ݼ�����~�}�l����

        foreach (Transform go in buttons)  // �M���Ҧ����ާ@���s
        {
            go.GetComponent<Button>().onClick.AddListener(delegate  // �ھګ��s�W�ٵ����s�K�[�ƥ��ť
            {
                switch (go.name)
                {
                    case "PlayBtn":
                        Play();
                        break;
                    case "PauseBtn":
                        Pause();
                        break;
                    case "BackBtn":
                        Backward10Seconds();
                        break;
                    case "ForwardBtn":
                        Forward10Seconds();
                        break;
                    case "MuteBtn":
                        Mute();
                        break;
                    case "UnmuteBtn":
                        Unmute();
                        break;
                }
            });
        }
    }
    void Update()
    {
        if (audioSource.clip != null)
        {
            Nowtime();
            Alltime();
            NowMusic();
            Vol();
        }
    }
    void Nowtime()
    {
        currentHour = (int)(audioSource.time / 3600);                        // ��
        currentMinute = (int)(audioSource.time - currentHour * 3600) / 60;  // ��
        currentSecond = (int)(audioSource.time - currentHour * 3600 - currentMinute * 60); // ��

        // ��ܷ�e����L���ɶ��]�榡�ơ^
        nowTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", currentHour, currentMinute, currentSecond);

        // �i�ױ�����G�p�G�S���b�즲�i�ױ��A�N�۰ʧ�s�i��
        if (!DragSlider.isDrag)
        {
            progressSlider.value = audioSource.time / audioSource.clip.length;
        }
    }
    void Alltime()
    {
        // slid.value = 0; // �i��O�ª����Q��l�ƶi�ױ��A�o�̤w���ѱ�
        clipHour = (int)(audioSource.clip.length / 3600);                    // ��
        clipMinute = (int)(audioSource.clip.length - clipHour * 3600) / 60; // ��
        clipSecond = (int)(audioSource.clip.length - clipHour * 3600 - clipMinute * 60); // ��

        // ��ܺq���`���ס]�榡�ơ^
        allTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", clipHour, clipMinute, clipSecond);
    }
    void NowMusic()
    {
        AudioClip clip = audioSource.clip;                       // ���o�ثe���񪺭���
        string nowMusicName = audioSource.clip.name;             // ���o�ثe�q���W��
        string[] showMusicName = nowMusicName.Split('-');        // ���]�榡���G�q�W-�q��

        // ��ܮ榡�G�u�q�W�]�q��^�v
        // musicName.text = string.Format("{0}�]{1}�^", showMusicName[0], showMusicName[1]);
        musicName.text = musicname.text;

        // index = Array.IndexOf(audioClips, clip);                 // ��X�ثe�q���b����M�椤����m
        Slider();                                                // ��s�Ʊ쪬�A�]�i��O�U�@���B�W�@���O�_�i���^
    }

    public void Backward10Seconds()
    {
        if (audioSource.clip == null) return;

        float newTime = audioSource.time - 10f;

        if (newTime <= 0)
        {
            // �p�G�˰h��p�� 0�A�h���s����
            audioSource.time = 0;
        }
        else
        {
            audioSource.time = newTime;
        }

        // audioSource.Play(); // ����]�T�O���|�]���˰h�ɬO�Ȱ����A�^
        progressSlider.value = audioSource.time / audioSource.clip.length;
    }

    public void Forward10Seconds()
    {
        if (audioSource.clip == null) return;

        float newTime = audioSource.time + 10f;

        if (newTime >= audioSource.clip.length)
        {
            // �p�G�����W�X�`���סA�h���s����
            audioSource.time = 0;
        }
        else
        {
            audioSource.time = newTime;
        }

        // audioSource.Play(); // ����]����A���~�^
        progressSlider.value = audioSource.time / audioSource.clip.length;
    }


    private void Play()
    {
        playBtn.SetActive(false);   // ���ü�����s
        pauseBtn.SetActive(true);   // ��ܼȰ����s

        // �p�G�ثe�w�g���b����A�N���������
        if (audioSource.isPlaying)
            return;

        audioSource.Play();         // ���񭵼�
    }
    public void Pause()
    {
        playBtn.SetActive(true);    // ��ܼ�����s
        pauseBtn.SetActive(false);  // ���üȰ����s

        audioSource.Pause();        // �Ȱ�����
    }
    private void Mute()
    {
        muteBtn.SetActive(false);    // ���áu�R���v���s
        unmuteBtn.SetActive(true);   // ��ܡu�����R���v���s
        audioSource.mute = true;     // �����R��
    }
    private void Unmute()
    {
        muteBtn.SetActive(true);     // ��ܡu�R���v���s
        unmuteBtn.SetActive(false);  // ���áu�����R���v���s
        audioSource.mute = false;    // ���������R��
    }
    void Slider()
    {
        // ��ثe����ɶ� = �`�ɪ��� �� �۰ʼ���U�@��
        if (currentHour == clipHour && currentMinute == clipMinute && currentSecond == clipSecond)
        {
            audioSource.time = 0;     // ����ɶ����]
            progressSlider.value = 0; // �i�ױ��k�s
            audioSource.Play();       // �����e�q��
        }
    }
    void Vol()
    {
        AudioListener.volume = volSlider.value; // �վ�D���q�]0 ~ 1�^

        // ��ܭ��q�ʤ���]�|�ˤ��J����r��^
        volText.text = Mathf.Round(volSlider.value * 100).ToString();
    }

}

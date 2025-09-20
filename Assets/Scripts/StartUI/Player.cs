using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Player : MonoBehaviour
{
    public Transform buttons; // 控制所有按鈕的集合體

    public GameObject playBtn, pauseBtn, muteBtn, unmuteBtn;
    // 控制播放、暫停、靜音、取消靜音的按鈕

    // public AudioClip[] audioClips;      // 存放歌曲的陣列
    public AudioSource audioSource;     // 音樂播放器組件

    public TextMeshProUGUI volText;                // 音量數值顯示
    public Slider volSlider;            // 音量調整滑桿

    public TextMeshProUGUI musicName;              // 顯示歌曲名稱或歌手
    public TextMeshProUGUI nowTime;                // 顯示目前播放時間
    public TextMeshProUGUI allTime;                // 顯示整首歌的總長度
    public Slider progressSlider;       // 顯示播放進度的滑桿

    public int index;                   // 當前歌曲的索引（第幾首）

    public TextMeshProUGUI musicname;

    private int currentHour, currentMinute, currentSecond;
    // 目前播放的時間（時、分、秒）

    private int clipHour, clipMinute, clipSecond;
    // 總時間（時、分、秒）

    public static Player instance;      // 靜態的 Player 實例（可用來在其他類中存取）

    void Start()
    {
        playBtn.SetActive(true);
        pauseBtn.SetActive(false);
        muteBtn.SetActive(true);
        unmuteBtn.SetActive(false);

        instance = this;

        audioSource.Stop(); // 開始運行時不播放，需播放鍵才開始播放

        foreach (Transform go in buttons)  // 遍歷所有的操作按鈕
        {
            go.GetComponent<Button>().onClick.AddListener(delegate  // 根據按鈕名稱給按鈕添加事件監聽
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
        currentHour = (int)(audioSource.time / 3600);                        // 時
        currentMinute = (int)(audioSource.time - currentHour * 3600) / 60;  // 分
        currentSecond = (int)(audioSource.time - currentHour * 3600 - currentMinute * 60); // 秒

        // 顯示當前播放過的時間（格式化）
        nowTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", currentHour, currentMinute, currentSecond);

        // 進度條條件：如果沒有在拖曳進度條，就自動更新進度
        if (!DragSlider.isDrag)
        {
            progressSlider.value = audioSource.time / audioSource.clip.length;
        }
    }
    void Alltime()
    {
        // slid.value = 0; // 可能是舊版本想初始化進度條，這裡已註解掉
        clipHour = (int)(audioSource.clip.length / 3600);                    // 時
        clipMinute = (int)(audioSource.clip.length - clipHour * 3600) / 60; // 分
        clipSecond = (int)(audioSource.clip.length - clipHour * 3600 - clipMinute * 60); // 秒

        // 顯示歌曲總長度（格式化）
        allTime.text = string.Format("{0:D2}:{1:D2}:{2:D2}", clipHour, clipMinute, clipSecond);
    }
    void NowMusic()
    {
        AudioClip clip = audioSource.clip;                       // 取得目前播放的音樂
        string nowMusicName = audioSource.clip.name;             // 取得目前歌曲名稱
        string[] showMusicName = nowMusicName.Split('-');        // 假設格式為：歌名-歌手

        // 顯示格式：「歌名（歌手）」
        // musicName.text = string.Format("{0}（{1}）", showMusicName[0], showMusicName[1]);
        musicName.text = musicname.text;

        // index = Array.IndexOf(audioClips, clip);                 // 找出目前歌曲在播放清單中的位置
        Slider();                                                // 更新滑桿狀態（可能是下一首、上一首是否可按）
    }

    public void Backward10Seconds()
    {
        if (audioSource.clip == null) return;

        float newTime = audioSource.time - 10f;

        if (newTime <= 0)
        {
            // 如果倒退後小於 0，則重新播放
            audioSource.time = 0;
        }
        else
        {
            audioSource.time = newTime;
        }

        // audioSource.Play(); // 播放（確保不會因為倒退時是暫停狀態）
        progressSlider.value = audioSource.time / audioSource.clip.length;
    }

    public void Forward10Seconds()
    {
        if (audioSource.clip == null) return;

        float newTime = audioSource.time + 10f;

        if (newTime >= audioSource.clip.length)
        {
            // 如果快轉後超出總長度，則重新播放
            audioSource.time = 0;
        }
        else
        {
            audioSource.time = newTime;
        }

        // audioSource.Play(); // 播放（防止狀態錯誤）
        progressSlider.value = audioSource.time / audioSource.clip.length;
    }


    private void Play()
    {
        playBtn.SetActive(false);   // 隱藏播放按鈕
        pauseBtn.SetActive(true);   // 顯示暫停按鈕

        // 如果目前已經正在播放，就不做任何事
        if (audioSource.isPlaying)
            return;

        audioSource.Play();         // 播放音樂
    }
    public void Pause()
    {
        playBtn.SetActive(true);    // 顯示播放按鈕
        pauseBtn.SetActive(false);  // 隱藏暫停按鈕

        audioSource.Pause();        // 暫停音樂
    }
    private void Mute()
    {
        muteBtn.SetActive(false);    // 隱藏「靜音」按鈕
        unmuteBtn.SetActive(true);   // 顯示「取消靜音」按鈕
        audioSource.mute = true;     // 音源靜音
    }
    private void Unmute()
    {
        muteBtn.SetActive(true);     // 顯示「靜音」按鈕
        unmuteBtn.SetActive(false);  // 隱藏「取消靜音」按鈕
        audioSource.mute = false;    // 取消音源靜音
    }
    void Slider()
    {
        // 當目前播放時間 = 總時長時 → 自動播放下一首
        if (currentHour == clipHour && currentMinute == clipMinute && currentSecond == clipSecond)
        {
            audioSource.time = 0;     // 播放時間重設
            progressSlider.value = 0; // 進度條歸零
            audioSource.Play();       // 播放當前歌曲
        }
    }
    void Vol()
    {
        AudioListener.volume = volSlider.value; // 調整主音量（0 ~ 1）

        // 顯示音量百分比（四捨五入後轉字串）
        volText.text = Mathf.Round(volSlider.value * 100).ToString();
    }

}

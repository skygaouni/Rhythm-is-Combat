using System;
using System.IO;
using System.Collections;
using Unity.Mathematics;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Unity.Collections;
using TMPro;
using UnityEngine.UI;

public class BeatAnalyzer_BurstFFT_noCoroutine : MonoBehaviour
{
    public AudioClip audioClip;
    public TextMeshProUGUI musicname;
    public Slider difficulty;
    private string outputFileName; // 儲存 beat 時間的檔案
    private int cooldownFrame = 30;
    float startTime, endTime, elapsed;

    void Start()
    {
        Application.targetFrameRate = 120;   // 鎖定遊戲幀率為 120
        QualitySettings.vSyncCount = 0;     // 關閉垂直同步，避免干擾
                                            // 取得 musicname 的文字（例如 "test.mp3"）
        string fileName = musicname.text;              // "test.mp3"
        string nameOnly = Path.GetFileNameWithoutExtension(fileName); // "test"

        // 從 Resources 載入音檔
        audioClip = Resources.Load<AudioClip>("Audio/" + nameOnly);

        if (audioClip == null)
        {
            Debug.LogError("❌ 找不到音檔：" + nameOnly);
            return;
        }

        cooldownFrame = ((int)difficulty.value);
        Debug.Log(cooldownFrame);

        outputFileName = nameOnly + ".txt"; // 儲存 beat 檔名
        float[] samples = new float[audioClip.samples];
        audioClip.GetData(samples, 0);
        startTime = Time.realtimeSinceStartup;
        AnalyzeBeatsSync(samples, audioClip.frequency);

    }

    void AnalyzeBeatsSync(float[] samples, int sampleRate)
    {
        int frameSize = 1024;
        int hopSize = 512;
        int lowFreqBins = 50;

        var burstFft = new BurstFft(frameSize);
        var fftInput = new NativeArray<float>(frameSize, Allocator.TempJob);
        List<float> energyList = new List<float>();
        List<int> beatFrameIndices = new List<int>();

        for (int i = 0; i + frameSize < samples.Length; i += hopSize)
        {
            for (int j = 0; j < frameSize; j++)
            {
                fftInput[j] = samples[i + j];
            }

            burstFft.Transform(fftInput);
            var spectrum = burstFft.Spectrum;

            float energy = 0f;
            for (int k = 0; k < lowFreqBins; k++)
            {
                energy += spectrum[k];
            }

            energyList.Add(energy);
        }

        fftInput.Dispose();
        burstFft.Dispose();

        int window = 10;
        List<int> allPeaks = new List<int>();

        // Step 1: 找所有符合條件的 peak
        for (int i = 0; i < energyList.Count; i++)
        {
            float avg = 0f;
            for (int j = Mathf.Max(0, i - window); j < i; j++)
                avg += energyList[j];
            avg /= window;

            if (energyList[i] > avg * 1.35f && IsPeak(energyList, i))
            {
                allPeaks.Add(i);
            }
        }

        // Step 2: 依 energy 值由大到小排序
        allPeaks.Sort((a, b) => energyList[b].CompareTo(energyList[a]));

        // Step 3: 應用 cooldownFrame，避免選到太近的 beats
        List<int> finalBeats = new List<int>();

        foreach (int idx in allPeaks)
        {
            bool tooClose = finalBeats.Exists(b => Mathf.Abs(b - idx) < cooldownFrame);
            if (!tooClose)
            {
                finalBeats.Add(idx);
                Debug.Log($"🔊 Beat at frame {idx}, energy={energyList[idx]:F2}");
            }
            else
            {
                Debug.Log($"🚫 cooldown 擋掉: i={idx}");
            }
        }

        // Step 4: 排序並寫入檔案（按時間）
        finalBeats.Sort();
        SaveFrameEnergiesToFile(energyList, hopSize, sampleRate, outputFileName);
        SaveBeatsToFile(finalBeats, hopSize, sampleRate);
    }
    /*void AnalyzeBeatsSync(float[] samples, int sampleRate)
    {
        int frameSize = 1024;
        int hopSize = 512;
        int lowFreqBins = 5;

        var burstFft = new BurstFft(frameSize);
        var fftInput = new NativeArray<float>(frameSize, Allocator.TempJob);
        List<float> energyList = new List<float>();
        List<int> beatFrameIndices = new List<int>();

        for (int i = 0; i + frameSize < samples.Length; i += hopSize)
        {
            for (int j = 0; j < frameSize; j++)
            {
                fftInput[j] = samples[i + j];
            }

            burstFft.Transform(fftInput);
            var spectrum = burstFft.Spectrum;

            float energy = 0f;
            for (int k = 0; k < lowFreqBins; k++)
            {
                energy += spectrum[k];
            }

            energyList.Add(energy);
        }

        fftInput.Dispose();
        burstFft.Dispose();

        int window = 10;
        List<int> allPeaks = new List<int>();

        // ✅ Step 1: 計算全曲平均能量
        float globalAvg = 0f;
        foreach (float e in energyList)
            globalAvg += e;
        globalAvg /= energyList.Count;

        // ✅ Step 2: 結合 localAvg + globalAvg 做 peak 偵測
        for (int i = 0; i < energyList.Count; i++)
        {
            float localAvg = 0f;
            int start = Mathf.Max(0, i - window);
            int count = i - start;

            for (int j = start; j < i; j++)
                localAvg += energyList[j];

            if (count > 0)
                localAvg /= count;
            else
                localAvg = energyList[i]; // fallback

            if (energyList[i] > globalAvg * 1.3f &&
                energyList[i] > localAvg * 1.5f &&
                IsPeak(energyList, i))
            {
                allPeaks.Add(i);
            }
        }

        // Step 3: 根據能量值排序
        allPeaks.Sort((a, b) => energyList[b].CompareTo(energyList[a]));

        // Step 4: 套用 cooldownFrame 過濾節奏點
        List<int> finalBeats = new List<int>();
        foreach (int idx in allPeaks)
        {
            bool tooClose = finalBeats.Exists(b => Mathf.Abs(b - idx) < cooldownFrame);
            if (!tooClose)
            {
                finalBeats.Add(idx);
                Debug.Log($"🔊 Beat at frame {idx}, energy={energyList[idx]:F2}");
            }
            else
            {
                Debug.Log($"🚫 cooldown 擋掉: i={idx}");
            }
        }

        // Step 5: 輸出結果
        finalBeats.Sort();
        SaveFrameEnergiesToFile(energyList, hopSize, sampleRate, outputFileName);
        SaveBeatsToFile(finalBeats, hopSize, sampleRate);
    }*/
    bool IsPeak(List<float> list, int i)
    {
        if (i <= 0 || i >= list.Count - 1)
            return false;

        // 平台起點：高於前一格，等於後一格
        if (list[i] >= list[i - 1])
        {
            int right = i;
            while (right + 1 < list.Count && list[right + 1] == list[i])
                right++;

            if (right + 1 < list.Count && list[right + 1] < list[i])
            {
                int mid = (i + right) / 2;
                return i == mid;
            }
        }

        return false;
    }
    void SaveFrameEnergiesToFile(List<float> energies, int hopSize, int sampleRate, string nameOnly)
    {
        string path = Application.dataPath + "/Resources/beatmap/";
        string filename = Path.Combine(path, nameOnly + "_energy.txt");

        using (StreamWriter writer = new StreamWriter(filename))
        {
            for (int i = 0; i < energies.Count; i++)
            {
                float time = (i * hopSize) / (float)sampleRate;
                writer.WriteLine($"{time:F2},{energies[i]:F6}");
            }
        }

        Debug.Log("✅ Frame energies written to: " + filename);
    }

    void SaveBeatsToFile(List<int> beatFrames, int hopSize, int sampleRate)
    {
        string path = Application.dataPath + "/Resources/beatmap/";
        string filepath = Path.Combine(path, outputFileName);

        using (StreamWriter writer = new StreamWriter(filepath))
        {
            foreach (int frameIndex in beatFrames)
            {
                float timeInSeconds = (frameIndex * hopSize) / (float)sampleRate;
                writer.WriteLine($"{timeInSeconds:F2}"); // 寫入秒數，保留兩位小數
            }
        }

        Debug.Log($"✅ Beat 時間已寫入 {path}");
        endTime = Time.realtimeSinceStartup;
        elapsed = endTime - startTime;
        Debug.Log("花費時間：" + elapsed + " 秒");
    }
}

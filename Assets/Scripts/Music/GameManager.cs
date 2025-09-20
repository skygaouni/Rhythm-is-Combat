using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public BulletDiagram diagram;

    public PlayerGunSelector gunSelector;

    public Animator animator;

    public AudioSource myMusic;

    public bool startPlaying;

    public Spawner spawner;

    public static GameManager instance;

    public GameObject resultScreen, PausePanel;

    private int perfect, normal, miss;

    private float acc;

    public TextMeshProUGUI perfectCount, normalCount, missCount, percentage, totalCount;

    public bool emergencyStop;

    public Transform playerTransform;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0; // ���������P�B�A�~���� targetFrameRate �ͮ�
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.Escape))
        {
            emergencyStop = !emergencyStop;
            if (emergencyStop)
            {
                myMusic.Pause();
                Time.timeScale = 0;
                PausePanel.SetActive(true);
            }
            else
            {
                myMusic.UnPause();
                Time.timeScale = 1;
                PausePanel.SetActive(false);
            }
        }*/
        if (!startPlaying)
        {
            if (Input.anyKey)
            {
                startPlaying = true;
                // spawner.hasStart = true;
                string musicPath = Application.dataPath + "/Logs/musicname.txt";
                string musicNameWithExtension = File.ReadAllText(musicPath).Trim();
                string musicName = Path.GetFileNameWithoutExtension(musicNameWithExtension);
                Debug.Log("���J���֦W�١G" + musicName);

                myMusic.clip = Resources.Load<AudioClip>("Audio/" + musicName);
                spawner.LoadBeatData(Application.dataPath + "/Resources/beatmap/" + musicName + ".txt");
                double dspStartTime = AudioSettings.dspTime + 2.0; // �w�d�@�I buffer
                myMusic.PlayScheduled(dspStartTime);
                spawner.StartSpawning(dspStartTime);
            }
        }
        else
        {
            if (!myMusic.isPlaying && Time.timeScale > 0)
            {
                // ���ּ��񵲧��A�۰ʭ��s����í��� Spawner
                double dspStartTime = AudioSettings.dspTime + 0.5;
                myMusic.PlayScheduled(dspStartTime);
                spawner.StartSpawning(dspStartTime);
            }

            if (Input.GetKeyDown(KeyCode.P) && !resultScreen.activeInHierarchy) // �Ȱ�
            {
                spawner.StopSpawning();
                myMusic.Stop();
                Time.timeScale = 0;
                perfect = ScoreManager.instance.perfectHits;
                normal = ScoreManager.instance.normalHits;
                miss = ScoreManager.instance.missHits;
                acc = (float)(perfect + normal) / (float)(perfect + normal + miss) * 100.0f;
                perfectCount.text = perfect.ToString("D6");
                normalCount.text = normal.ToString("D6");
                missCount.text = miss.ToString("D6");
                percentage.text = acc.ToString("F2") + "%";
                totalCount.text = ScoreManager.instance.score.ToString("D6");
                resultScreen.SetActive(true);
            }
            else
            {
                CheckWrongKeyPress(KeyCode.Mouse0);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                if (playerTransform != null)
                {
                    playerTransform.position = new Vector3(0, 5, 0);
                }
            }

        }
    }
    void CheckWrongKeyPress(KeyCode key)
    {
        if (Input.GetKeyDown(key) && gunSelector.activeGun.canShoot)
        {
            if (!AnyNoteCanBePressed(key))
            {
                WrongHit();
            }
        }
    }
    bool AnyNoteCanBePressed(KeyCode key)
    {
        NoteOB[] allNotes = FindObjectsOfType<NoteOB>();

        foreach (NoteOB note in allNotes)
        {
            if (note.keyTopress == key && note.canBePress)
            {
                return true; // ���i���� Note
            }
        }
        return false; // �S���i���� Note
    }



    public void NoteHit()
    {
        //Debug.Log("Hit!");
    }
    public void NoteMiss()
    {
        //Debug.Log("Miss!");
        ScoreManager.instance.missHits++;
    }
    public void WrongHit()
    {
        diagram.recordPressShootKey("WrongHit");

        //Debug.Log("Wrong Hit!");

        ScoreManager.instance.resetMultiply();
        // ScoreManager.instance.MinusScore(100);
    }
    public void PerfectHit()
    {
        diagram.recordPressShootKey("PerfectHit");

        //Debug.Log("Perfect Hit!");
        ScoreManager.instance.changeMultiply(0.1f);
        ScoreManager.instance.perfectScore();
        ScoreManager.instance.perfectHits++;

        gunSelector.activeGun.Tick(true);
    }
    public void NormalHit()
    {
        diagram.recordPressShootKey("NormalHit");

        //Debug.Log("Normal Hit!");
        ScoreManager.instance.changeMultiply(0.1f);
        ScoreManager.instance.normalScore();
        ScoreManager.instance.normalHits++;

        gunSelector.activeGun.Tick(true);
    }

}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int score = 0;
    public float multiply = 1.0f;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreText2;
    public TextMeshProUGUI multiplytext;

    [Header("ScoreValue setting")]
    public int BaseScoreValue = 100;
    public int PerfectScoreValue = 300;

    [Header("Result")]
    public int totalNotes;
    public int normalHits;
    public int perfectHits;
    public int missHits;


    void Awake()
    {
        instance = this;
    }
    public void changeMultiply(float a)
    {
        multiply += a;
        multiply = Mathf.Round(multiply * 10f) / 10f;
        multiplytext.text = "Multiply: " + multiply.ToString("F1");
    }
    public void resetMultiply()
    {
        multiply = 1.0f;
        multiplytext.text = "Multiply: " + multiply.ToString("F1");
    }

    public void perfectScore()
    {
        ScoreManager.instance.AddScore(PerfectScoreValue);
    }

    public void normalScore()
    {
        ScoreManager.instance.AddScore(BaseScoreValue);
    }

    public void AddScore(int value)
    {
        score += (int)(value * multiply);
        scoreText.text = "Score: " + score;
        scoreText2.text = "Score: " + score;
    }
    public void MinusScore(int value)
    {
        score -= value;
        scoreText.text = "Score: " + score;
        scoreText2.text = "Score: " + score;
    }
}

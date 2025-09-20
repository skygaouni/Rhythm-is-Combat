using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SetDifficulty : MonoBehaviour
{
    public Slider difficulty;
    public TextMeshProUGUI value;

    // Update is called once per frame
    void Update()
    {
        value.text = difficulty.value.ToString();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI magazineCapacity;

    [SerializeField]
    private PlayerGunSelector gunSelector;

    private void Update()
    {
        magazineCapacity.text = $"{gunSelector.activeGun.currentMagazineCapacity} / {gunSelector.activeGun.shootConfig.magazineCapacity}";

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class PlayerAction : MonoBehaviour
{
    [SerializeField]
    private PlayerGunSelector gunsleSelector;

   /* private void Update()
    {
        if(gunsleSelector.activeGun != null)
        {
            gunsleSelector.activeGun.Tick(Mouse.current.leftButton.isPressed);
        }
    }*/
}

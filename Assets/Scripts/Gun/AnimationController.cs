using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationController : MonoBehaviour
{
    [SerializeField]
    private PlayerGunSelector gunSelector;
    
    [SerializeField]
    private Animator animator;

    public void FinishReload()
    {
        Debug.Log("FinishReload");
        gunSelector.activeGun.GunFinishReload();
        animator.SetBool("Reloading", false);
    }

}

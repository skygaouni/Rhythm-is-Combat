using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class EnemyPainResponse : MonoBehaviour
{
    [SerializeField]
    private EnemyHealth health;
    private Animator animator;
    [SerializeField]
    [Range(1, 100)]
    private int MaxDamagePainThreshold = 5;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void HandlePain(int Damage)
    {
        if (health.currentHealth != 0)
        {
            // you can do some cool stuff based on the
            // amount of damage taken relative to max health
            // here we're simply setting the additive layer
            // weight based on damage vs max pain threshhold
            animator.ResetTrigger("Hit");
            animator.SetLayerWeight(1, (float)Damage / MaxDamagePainThreshold);
            animator.SetTrigger("Hit");
        }
    }

    public void HandleDeath()
    {
        animator.applyRootMotion = true;
        animator.SetTrigger("Die");

        // 啟動5秒後自動消失
        Invoke(nameof(DestroyAfterDeath), 5f);
    }

    void DestroyAfterDeath()
    {
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }
}

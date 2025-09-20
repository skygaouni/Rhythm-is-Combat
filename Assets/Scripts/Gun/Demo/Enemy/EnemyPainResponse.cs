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

    public FSM manager;
    public GameObject healthbar;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void HandlePain(int Damage)
    {
        if (health.currentHealth != 0)
        {
            manager.parameter.injury = true;
            //animator.SetLayerWeight(1, (float)Damage / MaxDamagePainThreshold);
        }
    }

    public void HandleDeath()
    {
        animator.applyRootMotion = true;
        manager.parameter.die = true;
        healthbar.SetActive(false);
        // 啟動5秒後自動消失
        Invoke(nameof(DestroyAfterDeath), 5f);
    }

    void DestroyAfterDeath()
    {
        // gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

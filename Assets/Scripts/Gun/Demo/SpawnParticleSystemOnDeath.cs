using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 強制這個元件所在的 GameObject 一定要有 IDamageable 組件
[RequireComponent(typeof(IDamageable))]
public class SpawnParticleSystemOnDeath : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem deathSystem;

    public IDamageable damageable;


    private void Awake()
    {
        damageable = GetComponent<IDamageable>();    
    }

    // 當這個元件啟用時（OnEnable），自動訂閱 onDeath 事件 -> 執行 Damageable_OnDeath()」：
    private void OnEnable()
    {
        damageable.onDeath += Damageable_OnDeath;
    }

    private void Damageable_OnDeath(Vector3 position)
    {
        // Quaternion.identity: 沒有旋轉（也就是預設朝向）
        Instantiate(deathSystem, position, Quaternion.identity);
    }

}

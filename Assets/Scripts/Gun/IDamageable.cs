using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// interface: 任何實作這個介面的人都一定要實作這些方法或屬性
public interface IDamageable
{
    // get; 只能在construct裡面才能設定值
    public int currentHealth { get; }
    public int maxHealth { get; }

    public delegate void takeDamageEvent(int Damage);
    public event takeDamageEvent onTakeDamage;

    public delegate void deathEvent(Vector3 position);
    public event deathEvent onDeath;

    public void TakeDamage(int damage);
}

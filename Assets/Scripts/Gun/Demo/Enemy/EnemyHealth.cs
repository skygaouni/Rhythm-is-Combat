using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // 加入這行來使用 Slider

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int _MaxHealth = 100;

    [SerializeField]
    private int _Health; // just to see in the inspector

    // get => _Health	外部可以讀取 currentHealth，實際回傳 _Health 的值
    // 只有類別內部可以寫入（設定）這個值
    public int currentHealth { get => _Health; private set => _Health = value; }

    public int maxHealth { get => _MaxHealth; private set => _MaxHealth = value; }

    public event IDamageable.takeDamageEvent onTakeDamage;
    public event IDamageable.deathEvent onDeath;

    [SerializeField] private Slider healthSlider;  // 拖入 Canvas 裡的 Slider
    [SerializeField] private GameObject healthUIRoot;  // 新增這個欄位來控制整個血條是否顯示

    private void OnEnable()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }

        if (healthUIRoot != null)
        {
            healthUIRoot.SetActive(false);  // 一開始先隱藏血條
        }
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Clamp(damage, 0, currentHealth);

        // 血條顯示
        if (damageTaken > 0 && healthUIRoot != null && !healthUIRoot.activeSelf)
        {
            healthUIRoot.SetActive(true);
        }

        currentHealth -= damageTaken;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (damageTaken != 0)
        {
            //  只有在 onTakeDamage 不是 null 時，才會呼叫
            onTakeDamage?.Invoke(damageTaken);
        }

        if(currentHealth == 0 && damageTaken != 0)
        {
            
            onDeath?.Invoke(transform.position);
        }
    }
}

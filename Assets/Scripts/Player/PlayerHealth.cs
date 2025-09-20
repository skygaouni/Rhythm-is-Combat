using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // 加這行才能使用 TextMeshPro

public class PlayerHealth : MonoBehaviour, IDamageable
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

    [SerializeField] private TMP_Text healthText; // << TextMeshPro 的欄位
    [SerializeField] private Slider healthSlider; // 血條 Slider
    [SerializeField] private Image fillImage;     // Fill 區塊的 Image
    public Gradient gradient;

    private void OnEnable()
    {
        currentHealth = maxHealth;
        UpdateHealthText(); // 初始顯示
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Clamp(damage, 0, currentHealth);

        currentHealth -= damageTaken;

        if (damageTaken != 0)
        {
            //  只有在 onTakeDamage 不是 null 時，才會呼叫
            onTakeDamage?.Invoke(damageTaken);
        }

        UpdateHealthText();

        if (currentHealth == 0 && damageTaken != 0)
        {

            onDeath?.Invoke(transform.position);
        }


    }
    private void UpdateHealthText()
    {
        // 更新數值與血條顏色
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;

            if (fillImage != null && gradient != null)
            {
                fillImage.color = gradient.Evaluate(healthSlider.normalizedValue);
            }
        }
    }
}

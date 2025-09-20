using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // �[�o��~��ϥ� TextMeshPro

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField]
    private int _MaxHealth = 100;

    [SerializeField]
    private int _Health; // just to see in the inspector

    // get => _Health	�~���i�HŪ�� currentHealth�A��ڦ^�� _Health ����
    // �u�����O�����i�H�g�J�]�]�w�^�o�ӭ�
    public int currentHealth { get => _Health; private set => _Health = value; }

    public int maxHealth { get => _MaxHealth; private set => _MaxHealth = value; }

    public event IDamageable.takeDamageEvent onTakeDamage;
    public event IDamageable.deathEvent onDeath;

    [SerializeField] private TMP_Text healthText; // << TextMeshPro �����
    [SerializeField] private Slider healthSlider; // ��� Slider
    [SerializeField] private Image fillImage;     // Fill �϶��� Image
    public Gradient gradient;

    private void OnEnable()
    {
        currentHealth = maxHealth;
        UpdateHealthText(); // ��l���
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Clamp(damage, 0, currentHealth);

        currentHealth -= damageTaken;

        if (damageTaken != 0)
        {
            //  �u���b onTakeDamage ���O null �ɡA�~�|�I�s
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
        // ��s�ƭȻP����C��
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

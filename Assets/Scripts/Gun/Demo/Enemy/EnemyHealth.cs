using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // �[�J�o��Өϥ� Slider

public class EnemyHealth : MonoBehaviour, IDamageable
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

    [SerializeField] private Slider healthSlider;  // ��J Canvas �̪� Slider
    [SerializeField] private GameObject healthUIRoot;  // �s�W�o�����ӱ����Ӧ���O�_���

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
            healthUIRoot.SetActive(false);  // �@�}�l�����æ��
        }
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = Mathf.Clamp(damage, 0, currentHealth);

        // ������
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
            //  �u���b onTakeDamage ���O null �ɡA�~�|�I�s
            onTakeDamage?.Invoke(damageTaken);
        }

        if(currentHealth == 0 && damageTaken != 0)
        {
            
            onDeath?.Invoke(transform.position);
        }
    }
}

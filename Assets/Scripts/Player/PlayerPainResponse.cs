using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerPainResponse : MonoBehaviour
{
    [Header("Health")]
    [SerializeField]
    private PlayerHealth health;
    private bool hurtable;

    [SerializeField]
    [Range(1, 100)]
    private int MaxDamagePainThreshold = 5;

    [Header("References")]
    public GameObject player;
    public Rigidbody rb;
    public Animator animator;
    public PlayerMovement pm;
    public ParticleSystem hurtEffect;
    public PlayerCollision playerCollision;

    [Header("Knockback")]
    public float knockBackForce;
    public float knockUpForce;

    [SerializeField]
    private LayerMask damageSourceLayers;

    public void hurt(int causeDamage)
    {
        health.TakeDamage(causeDamage);
    }

    // �B�z�P�Ǫ����骺�I���A�Ҧp�Ǫ���ԡB�I��Ǫ�����
    void OnCollisionEnter(Collision collision)
    {
        // ���o�Ĥ@�ӱ�Ĳ�I���I������
        GameObject hitPart = collision.GetContact(0).otherCollider.gameObject;

        //Debug.Log("���쪺��ڳ���G" + hitPart.name);
        //Debug.Log("���쪺 Layer�G" + LayerMask.LayerToName(hitPart.layer));

        // �P�_�o�ӳ���O�_�ݩ�|�y���ˮ`���h
        if ((damageSourceLayers.value & (1 << hitPart.layer)) != 0)
        {
            //Debug.Log("�������");

            EnemyMovement enemy = hitPart.GetComponentInParent<EnemyMovement>();

            if (!hurtable)
            {
                hurtable = true;
                health.TakeDamage(enemy.damageDealer);
            }
            

            Vector3 knockbackDirection = player.transform.position - collision.GetContact(0).point;
            knockbackDirection.y = 0f;
            knockbackDirection = knockbackDirection.normalized;
            
            StartCoroutine(KnockbackForceOverTime(knockbackDirection, knockBackForce, 0.5f));

            
        }
    }

    public void HandlePain(int Damage)
    {
        if (health.currentHealth != 0)
        {
            // you can do some cool stuff based on the
            // amount of damage taken relative to max health
            // here we're simply setting the additive layer
            // weight based on damage vs max pain threshhold
            //animator.ResetTrigger("Hit");
            //animator.SetLayerWeight(1, (float)Damage / MaxDamagePainThreshold);
            //animator.SetTrigger("Hit");
           

            hurtEffect.Play();
        }
    }

    public void HandleDeath()
    {
        animator.applyRootMotion = true;
        animator.SetTrigger("Die");

        // �Ұ�5���۰ʮ���
        Invoke(nameof(DestroyAfterDeath), 0.5f);
    }

    void DestroyAfterDeath()
    {
        gameObject.SetActive(false);
        //Destroy(gameObject);
    }


    private IEnumerator KnockbackForceOverTime(Vector3 direction, float force, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            rb.AddForce(direction * force, ForceMode.Force);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        hurtable = false;
    }
}

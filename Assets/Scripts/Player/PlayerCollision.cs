using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// put in enemy VFX
public class PlayerCollision : MonoBehaviour
{
    public bool continued;

    public bool particleCollision = false;
    public int damageCause;
    private GameObject player;

    private PlayerPainResponse playerPainResponse;
    private Rigidbody rb;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            rb = player.GetComponent<Rigidbody>();
            playerPainResponse = player.GetComponent<PlayerPainResponse>();

            Collider playerCollider = player.GetComponentInChildren<Collider>();

            if (playerCollider != null)
            {
                ParticleSystem ps = GetComponent<ParticleSystem>();
                ps.trigger.SetCollider(0, playerCollider);


            }
        }
    }

    private void OnParticleTrigger()
    {

        if (particleCollision)
            return;


        playerPainResponse.hurt(damageCause);
        particleCollision = true;


        Vector3 forceDirection = playerPainResponse.transform.position - gameObject.transform.position;
        forceDirection = forceDirection.normalized;

        if (!continued)
        {
            Debug.Log("knockBack");
            rb.AddForce(forceDirection * playerPainResponse.knockBackForce, ForceMode.Impulse);
            rb.AddForce(gameObject.transform.up * playerPainResponse.knockUpForce, ForceMode.Impulse);
        }
        //Debug.Log("�ɤl�I���쪱�a�I�]�uĲ�o�@���^");

    }

}

using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class BulletLogic : MonoBehaviour
{
    public GunScriptableObject gun;
    public TrailRenderer trail;

    public bool hasHit;
    public bool miss;
    public bool causeDamage;

    public Vector3 startPos;
    public Vector3 endPos;

    private void OnEnable()
    {
        gun = GameObject.Find("Player").GetComponent<PlayerGunSelector>().activeGun;
        hasHit = false;
        miss = false;
        causeDamage = false;
    }

    public void OnUpdate(Vector3 startPos, Vector3 missPos)
    {
        if (Vector3.Distance(transform.position, startPos) >= Vector3.Distance(missPos, startPos))
            miss = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("whatIsEnemy"))
        {
            if (hasHit) return;
            hasHit = true;

            endPos = other.gameObject.transform.position;

            float distance = Vector3.Distance(startPos, endPos);
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(gun.damageConfig.getDamage(distance));
                causeDamage = true;
            }
        }
        else if(other.gameObject.layer != LayerMask.NameToLayer("Exclude"))
        {
            miss = true;
            endPos = other.gameObject.transform.position;
        }
    }

}
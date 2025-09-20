using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartialSystemTrigger : MonoBehaviour
{
    public ParticleSystem[] partialSystem;

    public void Attack()
    {
        for (int i = 0; i < partialSystem.Length; i++)
        {
            var collision = partialSystem[i].GetComponent<PlayerCollision>();
            if (collision != null)
            {
                collision.particleCollision = false;
            }

            partialSystem[i].Play();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyHealth health;
    public EnemyMovement movement;
    public EnemyPainResponse painResponse;

    private void Start()
    {
        health.onTakeDamage += painResponse.HandlePain;
        health.onDeath += Die;
    }


    private void Die(Vector3 position)
    {
        Debug.Log("death");
        painResponse.HandleDeath();
        movement.StopMoving();   
    }
}

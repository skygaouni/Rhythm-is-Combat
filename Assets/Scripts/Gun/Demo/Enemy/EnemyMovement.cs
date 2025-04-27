using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private float StillDelay = 1f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void StopMoving()
    {
        StopAllCoroutines();
        //agent.isStopped = true;
        //agent.enabled = false;
    }
}

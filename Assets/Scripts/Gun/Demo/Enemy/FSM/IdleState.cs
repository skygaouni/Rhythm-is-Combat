using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;

public class IdleState : IState
{
    private FSM manager;
    private Parameter parameter;

    private float timer;

    public IdleState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        parameter.RVOAgent.Deactivate();

        parameter.detectBullet = false;

        //parameter.agent.isStopped = true;
        //parameter.rb.velocity = Vector3.zero;
        //parameter.rb.angularVelocity = Vector3.zero;

        int randomIdle = Random.Range(1, 3); // 1 或 2
        //parameter.animator.SetInteger("IdleIndex", randomIdle);
        //parameter.animator.SetBool("Idle", true);

        if (randomIdle == 1)
            parameter.animator.Play("Idle 1");

        else if(randomIdle == 2)
            parameter.animator.Play("Idle 2");
            
    }

    public void OnUpdate()
    {
        timer += Time.deltaTime;

        float playerDistanceFromSpawn = 0; 
        if (parameter.TargetPlayer != null)
            playerDistanceFromSpawn = Vector3.Distance(parameter.TargetPlayer.position, parameter.spawnPosition);

        float distanceFromSpawn = Vector3.Distance(manager.transform.position, parameter.spawnPosition);

        if (parameter.detectBullet && !parameter.backflipCooldown && parameter.canBackFlip)
        {
            manager.TransitionState(StateType.BackFlip);
        }
        // 有找到目標，且目標在追擊範圍
        else if (parameter.TargetPlayer != null && distanceFromSpawn <= parameter.chaseRadius && playerDistanceFromSpawn <= parameter.chaseRadius)
        {
            manager.TransitionState(StateType.React);
        }
        else if (parameter.die)
        {
            manager.TransitionState(StateType.Death);
        }
        else if (parameter.injury)
        {
            manager.TransitionState(StateType.Injury);
        }
        else if (timer >= parameter.idleTime)
        {
            manager.TransitionState(StateType.Patrol);
        }
    }

    public void OnExit()
    {
        manager.lastState = StateType.Idle;
        timer = 0;
        //parameter.animator.SetBool("Idle", false);
    }
}














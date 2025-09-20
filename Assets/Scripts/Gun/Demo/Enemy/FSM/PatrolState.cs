using RVO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : IState
{
    private FSM manager;
    private Parameter parameter;
    private EnemyMovement movement;

    private Vector3 moveTarget;
    private float isStuckTimer = 0f;

    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
        this.movement = parameter.movement;
    }

    public void OnEnter()
    {
        manager.currentStat = StateType.Patrol;

        //parameter.direction.previousPosition = manager.transform.position;
        isStuckTimer = 0f;

        //Debug.Log("EnterPatrol");
        //parameter.animator.SetTrigger("Walk(Patrol)"); // walk 動畫
        parameter.detectBullet = false;

        //parameter.agent.isStopped = false;
        moveTarget = manager.FetchMoveTargetPosition();
        //parameter.agent.SetDestination(moveTarget);
        parameter.RVOAgent.Activate();

        parameter.RVOAgent.SetTarget(moveTarget);
        
        parameter.animator.Play("Walk");
    }

    // 實施在nav mesh
    public void OnUpdate()
    {
        manager.LookDirection(parameter.movement.moveDirection);

        float playerDistanceFromSpawn = 0;
        float diatanceFormSpawn = Vector3.Distance(manager.transform.position, parameter.spawnPosition);

        if (parameter.TargetPlayer != null)
            playerDistanceFromSpawn = Vector3.Distance(parameter.TargetPlayer.position, parameter.spawnPosition);

        if (parameter.die)
        {
            manager.TransitionState(StateType.Death);
            return;
        }

        if (parameter.detectBullet  && !parameter.backflipCooldown && parameter.canBackFlip)
        {
            manager.TransitionState(StateType.BackFlip);
            return;
        }

        if (isStuckTimer < parameter.isStuckTime)
        {
            isStuckTimer += Time.deltaTime;
        }
        else
        {
            isStuckTimer = 0f;

            if (!movement.isMove && !movement.isTurning)
            {
                manager.TransitionState(StateType.Error);
            }

            /*if (Vector3.Distance(manager.transform.position, movement.previousPosition) < 0.3f)
            {
                manager.TransitionState(StateType.Error);
            }
            else
            {
                //parameter.lastPosition = manager.transform.position;
            }*/

            return;
        }


        // 有找到目標，且目標在追擊範圍且自身在追擊範圍內
        if (parameter.TargetPlayer != null && playerDistanceFromSpawn <= parameter.chaseRadius && diatanceFormSpawn <= parameter.chaseRadius)
        {
            manager.TransitionState(StateType.React);
            return;
        }
        
        if (parameter.injury)
        {
            manager.TransitionState(StateType.Injury);
            return;
        }
        // 移動到目標位置時，就切換到Idle狀態
        if (Vector3.Distance(manager.transform.position, moveTarget) < 2f)
        {
            manager.TransitionState(StateType.Idle);
            return;
        }

        
    }

    public void OnExit()
    {
        parameter.RVOAgent.Deactivate();

        manager.lastState = StateType.Patrol;
        moveTarget = manager.transform.position;
        //parameter.rb.velocity = Vector3.zero;
    }
}
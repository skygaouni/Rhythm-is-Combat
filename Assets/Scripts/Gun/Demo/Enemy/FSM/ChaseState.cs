using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseState : IState
{
    private FSM manager;
    private Parameter parameter;
    private EnemyMovement movement;

    private float isStuckTimer;

    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
        this.movement = manager.parameter.movement;
    }

    public void OnEnter()
    {
        isStuckTimer = 0f;
        manager.currentStat = StateType.Chase;

        parameter.RVOAgent.Activate();

        parameter.detectBullet = false;
        
        //parameter.agent.isStopped = false;
        //parameter.animator.SetTrigger("Run(Chase)"); // run 動畫 

        parameter.animator.Play("Run");
    }

    // 實施在nav mesh
    public void OnUpdate()
    {
        if (parameter.die)
        {
            manager.TransitionState(StateType.Death);
            return;
        }
        
        if (parameter.detectBullet && !parameter.backflipCooldown && parameter.canBackFlip)
        {
            manager.TransitionState(StateType.BackFlip);
            return;
        }

        if (parameter.injury)
        {
            manager.TransitionState(StateType.Injury);
            return;
        }

        // 是否在普攻攻擊範圍
        Collider[] meleeHits = Physics.OverlapSphere(parameter.meleeAttackPoint.position, parameter.meleeAttackRadius, parameter.targetLayer);
        // 是否在遠程攻擊範圍
        Collider[] rangedHits = Physics.OverlapSphere(parameter.rangedAttackPoint.position, parameter.rangedAttackRadius, parameter.targetLayer);

        if (meleeHits.Length > 0)
        {
            parameter.attackMode = Random.Range(1, 3);
            manager.TransitionState(StateType.Attack);
            return;
        }
        else if(rangedHits.Length > 0 && !parameter.rangeAttack)
        {
            parameter.attackMode = 3;
            parameter.rangeAttack = true;
            manager.TransitionState(StateType.Attack);
            return;
        }

        if (isStuckTimer < parameter.isStuckTime)
        {
            isStuckTimer += Time.deltaTime;
        }
        else
        {
            isStuckTimer = 0f;

            if(!movement.isMove && !movement.isTurning)
            {
                Debug.Log("isMove" + movement.isMove);
                Debug.Log("isTurning" + movement.isTurning);
                Debug.Log("Chase");
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


        // 沒找到玩家或者是已經到達追擊點
        float distanceFromSpawn = Vector3.Distance(manager.transform.position, parameter.spawnPosition);

        if (parameter.TargetPlayer == null || distanceFromSpawn > parameter.chaseRadius)
        {
            manager.TransitionState(StateType.Idle);
        }
        else
        {
            //parameter.agent.SetDestination(parameter.TargetPlayer.position);

            parameter.RVOAgent.SetTarget(parameter.TargetPlayer.transform.position);
            manager.LookDirection(parameter.TargetPlayer.position - manager.transform.position);
        }
    }

    public void OnExit()
    {
        parameter.RVOAgent.Deactivate();

        //Debug.Log("ExitChaseState");
        manager.lastState = StateType.Chase;
        //parameter.rb.velocity = Vector3.zero;
    }

}

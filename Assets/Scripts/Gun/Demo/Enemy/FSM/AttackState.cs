using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public AttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        parameter.RVOAgent.Deactivate();

        manager.currentStat = StateType.Attack;
        //Debug.Log("EnterAttackState");
        //parameter.agent.isStopped = true;
        //parameter.rb.velocity = Vector3.zero;
        //parameter.rb.angularVelocity = Vector3.zero;

        //parameter.agent.isStopped = false;
        //parameter.agent.SetDestination(parameter.TargetPlayer.position);

        //parameter.animator.SetInteger("AttackIndex", randomAttack);
        //parameter.animator.SetTrigger("Attack");

        if (parameter.attackMode == 1)
            parameter.animator.Play(parameter.attack1_Ranged);

        else if (parameter.attackMode == 2)
            parameter.animator.Play(parameter.attack2_Ranged);
       
        else if (parameter.attackMode == 3)
            parameter.animator.Play(parameter.attack3_Melee);
        
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.die)
        {
            manager.TransitionState(StateType.Death);
            return;
        }

        /*if (parameter.injury)
        {
            manager.TransitionState(StateType.Injury);
            return;
        }*/

        // 沒找到玩家或者是已經到達追擊點
        float distanceFromSpawn = Vector3.Distance(manager.transform.position, parameter.spawnPosition);

        if (distanceFromSpawn > parameter.chaseRadius + 1)
        {
            manager.TransitionState(StateType.Idle);
            return;
        }

        if (info.normalizedTime >= .95f)
        {
            if(parameter.TargetPlayer != null)
                manager.TransitionState(StateType.Chase);
            else
                manager.TransitionState(StateType.Idle);
        }
        else
        {
            if (parameter.TargetPlayer != null)
                manager.LookDirection(parameter.TargetPlayer.position - manager.transform.position);
        }
    }

    public void OnExit()
    {
        //Debug.Log("ExitAttackState");
        manager.lastState = StateType.Attack;
        if(parameter.attackMode == 3)
            manager.StartCoroutine(manager.RangeAttackCoolDown());
    }

}

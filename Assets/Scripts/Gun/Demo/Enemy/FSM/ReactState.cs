using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactState : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public ReactState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        manager.currentStat = StateType.React;

        parameter.RVOAgent.Deactivate();

        //parameter.rb.velocity = Vector3.zero;
        //parameter.agent.isStopped = true;
        //parameter.animator.SetBool("React", true);

        parameter.animator.Play("React(Scream)");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (parameter.die)
        {
            manager.TransitionState(StateType.Death);

            return;
        }

        // 沒找到玩家或者是已經到達追擊點
        //float distanceFromSpawn = Vector3.Distance(manager.transform.position, parameter.spawnPosition);

        if (parameter.TargetPlayer == null)
        {
            manager.TransitionState(StateType.Patrol);

            return;
        }
        /*else if (parameter.detectBullet)
        {
            manager.TransitionState(StateType.BackFlip);
        }*/
        /*else if (parameter.injury)
        {
            manager.TransitionState(StateType.Injury);
        }*/
        
        if (info.normalizedTime >= .95f)
            manager.TransitionState(StateType.Chase);
        
    }

    public void OnExit()
    {
        manager.lastState = StateType.React;
        //parameter.animator.SetBool("React", false);
    }

}
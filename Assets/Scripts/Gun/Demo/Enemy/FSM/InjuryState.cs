using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjuryState : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public InjuryState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        manager.currentStat = StateType.Injury;

        //parameter.agent.isStopped = true;
        //parameter.animator.SetTrigger("Injury");

        //Debug.Log("PlayGetHit");
        parameter.animator.Play("Get Hit");
    }

    public void OnUpdate()
    {
        if (parameter.die)
        {
            manager.TransitionState(StateType.Death);
            return;
        }
        
        // 沒找到玩家或者是已經到達追擊點
        float distanceFromSpawn = Vector3.Distance(manager.transform.position, parameter.spawnPosition);

        if (distanceFromSpawn > parameter.chaseRadius + 1)
        {
            manager.TransitionState(StateType.Idle);
            return;
        }

        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= .95f)
        {
            //Debug.Log(manager.lastState);
            manager.TransitionState(manager.lastState);
        }
    }

    public void OnExit()
    {
        parameter.injury = false;
        manager.lastState = StateType.Injury;
    }

}

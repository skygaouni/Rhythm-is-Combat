using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathState : IState
{
    private FSM manager;
    private Parameter parameter;

    private AnimatorStateInfo info;
    public DeathState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        manager.currentStat = StateType.Death;

        parameter.RVOAgent.Deactivate();

        //parameter.rb.velocity = Vector3.zero;
        //parameter.agent.isStopped = true;
        //parameter.animator.SetTrigger("Death");
        parameter.animator.Play("Die");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if (info.normalizedTime >= .95f)
        {
            manager.gameObject.SetActive(false);
        }
    }

    public void OnExit()
    {

    }
}

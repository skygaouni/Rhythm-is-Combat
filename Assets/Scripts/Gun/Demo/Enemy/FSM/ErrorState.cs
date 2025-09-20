using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorState : IState
{
    private FSM manager;
    private Parameter parameter;

    public ErrorState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        manager.currentStat = StateType.Error;

        //Debug.Log("EnterError");
        parameter.RVOAgent.Activate();

        manager.transform.position = parameter.spawnPosition;
        
    }

    public void OnUpdate()
    {
        manager.TransitionState(StateType.Idle);
    }

    public void OnExit()
    {
        //Debug.Log("ExitError");
        manager.lastState = StateType.Error;
    }
}

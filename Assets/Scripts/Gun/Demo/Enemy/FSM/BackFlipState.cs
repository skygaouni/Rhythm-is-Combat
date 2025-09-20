using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackFlipState : IState
{
    private FSM manager;
    private Parameter parameter;

    private float backFlipDuration = 1f;  // 後空翻總時間
    private bool isBackFlip;

    public BackFlipState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.parameter;
    }

    public void OnEnter()
    {
        manager.currentStat = StateType.Attack;

        parameter.RVOAgent.Deactivate();

        parameter.backflipCooldown = true;

        parameter.navMeshAgent.enabled = false;
        //parameter.rb.velocity = Vector3.zero;
        //parameter.rb.angularVelocity = Vector3.zero;

        parameter.animator.Play("Back Flip");
        isBackFlip = true;
        TriggerBackflip();       
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

        if(isBackFlip == false)
        {
            // 假如lastState為react state，切換到chaseState
            manager.TransitionState(manager.lastState);
        }
    }

    public void OnExit()
    {
        parameter.detectBullet = false;
        manager.lastState = StateType.BackFlip;
        parameter.navMeshAgent.enabled = true;

        manager.StartCoroutine(manager.BackFlipCoolDown());
    }

    IEnumerator DoBackflip()
    {
        //Debug.Log("DoBackFlip");

        isBackFlip = true;

        float elapsed = 0f;
        float totalRotation = 0f;
        float rotationSpeed = 360f / backFlipDuration;

        Vector3 startPos = manager.transform.position;
        Quaternion startRot = manager.transform.rotation;

        while (elapsed < backFlipDuration)
        {
            float t = elapsed / backFlipDuration;

            // 跳躍
            float height = Mathf.Sin(Mathf.PI * t) * parameter.backflipHeight;
            manager.transform.position = startPos + Vector3.up * height;

            // 旋轉
            float rt = Mathf.Clamp01(elapsed / backFlipDuration); // 從 0 到 1
            float targetAngle = rt * 360f;                         // 應該轉的角度
            float deltaAngle = targetAngle - totalRotation;        // 本幀該補多少
            manager.transform.Rotate(Vector3.right, -deltaAngle);         // 後空翻：負方向
            totalRotation = targetAngle;                          // 更新狀態

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. 最終落回地面 & 修正旋轉（萬一沒轉剛好）
        manager.transform.position = startPos;
        manager.transform.rotation = startRot;

        isBackFlip = false;
    }

    public void TriggerBackflip()
    {
        manager.StartCoroutine(DoBackflip());
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackFlipState : IState
{
    private FSM manager;
    private Parameter parameter;

    private float backFlipDuration = 1f;  // ���½�`�ɶ�
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

        // �S��쪱�a�Ϊ̬O�w�g��F�l���I
        float distanceFromSpawn = Vector3.Distance(manager.transform.position, parameter.spawnPosition);

        if (distanceFromSpawn > parameter.chaseRadius + 1)
        {
            manager.TransitionState(StateType.Idle);
            return;
        }

        if(isBackFlip == false)
        {
            // ���plastState��react state�A������chaseState
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

            // ���D
            float height = Mathf.Sin(Mathf.PI * t) * parameter.backflipHeight;
            manager.transform.position = startPos + Vector3.up * height;

            // ����
            float rt = Mathf.Clamp01(elapsed / backFlipDuration); // �q 0 �� 1
            float targetAngle = rt * 360f;                         // �����઺����
            float deltaAngle = targetAngle - totalRotation;        // ���V�Ӹɦh��
            manager.transform.Rotate(Vector3.right, -deltaAngle);         // ���½�G�t��V
            totalRotation = targetAngle;                          // ��s���A

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 3. �̲׸��^�a�� & �ץ�����]�U�@�S���n�^
        manager.transform.position = startPos;
        manager.transform.rotation = startRot;

        isBackFlip = false;
    }

    public void TriggerBackflip()
    {
        manager.StartCoroutine(DoBackflip());
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public enum StateType
{
    Idle, Patrol, Chase, React, Attack, Injury, Death, BackFlip, Error
}

[Serializable]
public class Parameter
{
    //public int health;

    //public float moveSpeed;

    //public float chaseSpeed;

    //public Rigidbody rb;

    public NavMeshAgent navMeshAgent;

    public RVOAgentWithNav RVOAgent;

    public EnemyMovement movement;

    // �d�b��a���ʪ��ɶ�
    public float isStuckTime;

    public Vector3 spawnPosition;

    public float idleTime;

    public float patrolRadius;

    public float chaseRadius;

    private Transform _targetPlayer;

    public Transform TargetPlayer
    {
        get => _targetPlayer;
        set => _targetPlayer = value;
    }

    public LayerMask targetLayer;

    public int attackMode;
    public string attack1_Ranged;
    public string attack2_Ranged;
    public string attack3_Melee;

    public Transform meleeAttackPoint;

    public float meleeAttackRadius;

    public bool rangeAttack;

    public Transform rangedAttackPoint;

    public float rangedAttackRadius;

    public float rangeAttackCoolDown;

    public bool detectBullet;
    public bool canBackFlip;
    public float backflipHeight;
    public bool backflipCooldown;
    public float backflipCooldownTime;

    public bool injury;

    public bool die;

    public Animator animator;
}


public class FSM : MonoBehaviour
{
    public Parameter parameter;

    private IState currentState;

    public StateType currentStat;
    public StateType lastState;

    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    private void Start()
    {
        //parameter.navMeshAgent.updatePosition = false;
        //parameter.navMeshAgent.updateRotation = false;
        //parameter.navMeshAgent.isStopped = true;


        parameter.spawnPosition = transform.position;
        //parameter.lastPosition = transform.position;

        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Patrol, new PatrolState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.React, new ReactState(this));
        states.Add(StateType.Attack, new AttackState(this));
        states.Add(StateType.Injury, new InjuryState(this));
        states.Add(StateType.Death, new DeathState(this));
        states.Add(StateType.BackFlip, new BackFlipState(this));
        states.Add(StateType.Error, new ErrorState(this));

        TransitionState(StateType.Idle);

        parameter.animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //Debug.Log(parameter.TargetPlayer);
        //if(currentState.ToString() == "InjuryState")
        //    Debug.Log(currentState.ToString());
        //Debug.Log(parameter.TargetPlayer.transform.position);
        currentState.OnUpdate();
    }

    public void TransitionState(StateType type)
    {
        if (currentState! != null)
            currentState.OnExit();



        currentState = states[type];

        currentState.OnEnter();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            parameter.TargetPlayer = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            parameter.TargetPlayer = null;
        }
    }

    // �զ�G�����ϰ�
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(parameter.meleeAttackPoint.position, parameter.meleeAttackRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(parameter.rangedAttackPoint.position, parameter.rangedAttackRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(parameter.spawnPosition, parameter.chaseRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(parameter.spawnPosition, parameter.patrolRadius);
    }

    // �վ㨵�ޥb�|���W�LpatrolRadius�d��
    public Vector3 FetchMoveTargetPosition()
    {

        float distanceFromSpawn;

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * parameter.patrolRadius;
        randomDirection.y = transform.position.y; // ��������
        Vector3 rawPoint = parameter.spawnPosition + randomDirection;
        Vector3 targetPosition = rawPoint;

        NavMeshHit hit;
        int findRadius = 1;
        int maxRadius = 10;

        while (findRadius <= maxRadius)
        {
            if (NavMesh.SamplePosition(rawPoint, out hit, findRadius, NavMesh.AllAreas))
            {
                //Vector3 hitPosition = new Vector3(hit.position.x, parameter.spawnPosition.y, hit.position.z);
                distanceFromSpawn = Vector3.Distance(hit.position, parameter.spawnPosition);
                if (distanceFromSpawn < parameter.patrolRadius)
                {
                    targetPosition = hit.position;
                    break;

                }

            }
            findRadius++;
        }

        return targetPosition;
    }

    public void LookDirection(Vector3 moveDirection)
    {
        //transform.LookAt(futureTargetPos);

        moveDirection.y = 0; // �i��G�����W�U����

        if (moveDirection.sqrMagnitude > 0.001f) // �T�O���O�s�V�q
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    public IEnumerator BackFlipCoolDown()
    {
        float timer = 0;

        while (timer <= parameter.backflipCooldownTime)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        parameter.backflipCooldown = false;
    }

    public IEnumerator RangeAttackCoolDown()
    {
        float timer = 0;

        while (timer <= parameter.rangeAttackCoolDown)
        {
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        //Debug.Log("ResetRangedAttack"); 
        parameter.rangeAttack = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyMovement : MonoBehaviour
{
    public Vector3 previousPosition { get; private set; }

    private Quaternion previousRotation;

    public Vector3 moveDirection { get; private set; }

    public int damageDealer;

    public bool isMove {  get; private set; }

    public bool isTurning { get; private set; }

    public float moveThreshold = 0.1f;
    public float isMoveStuckTime;
    private float isMoveStuckTimer;

    public float turnThreshold = 1f;
    public float isTurnStuckTime;
    private float isTurnStuckTimer;

    void Start()
    {
        moveDirection = Vector3.zero;
        previousPosition = transform.position;
    }

    void Update()
    {
        // 移動判斷
        if (isMoveStuckTimer < isMoveStuckTime)
        {
            isMoveStuckTimer += Time.deltaTime;
        }
        else
        {
            isMoveStuckTimer = 0f;

            if (Vector3.Distance(transform.position, previousPosition) < moveThreshold)
            {
                //Debug.Log("Don't move");
                isMove = false;

                return;
            }
            else if (Vector3.Distance(transform.position, previousPosition) >= moveThreshold)
            {
                //Debug.Log("Move");
                isMove = true;
                moveDirection = transform.position - previousPosition;
                moveDirection = moveDirection.normalized;
                previousPosition = transform.position;

                return;
            }
        }


        // 轉向判斷
        if (isTurnStuckTimer < isTurnStuckTime)
        {
            isTurnStuckTimer += Time.deltaTime;
        }
        else
        {
            isTurnStuckTimer = 0f;

            if (Quaternion.Angle(transform.rotation, previousRotation) < turnThreshold)
            {
                isTurning = false;

                return;
            }
            else if (Quaternion.Angle(transform.rotation, previousRotation) >= turnThreshold)
            {
                isTurning = true;

                previousRotation = transform.rotation;
                return;
            }
        }

        

        // Debug 顯示
        //Debug.DrawRay(transform.position, normalizedDirection, Color.green);
        //Debug.Log($"Move Dir: {normalizedDirection}, Speed: {moveDirection.magnitude / Time.deltaTime:F2}");
    }

    public void StopMoving()
    {
        StopAllCoroutines();
        //agent.isStopped = true;
        //agent.enabled = false;
    }
}




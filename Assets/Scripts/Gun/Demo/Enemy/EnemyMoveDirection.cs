using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveDirection : MonoBehaviour
{
    public Vector3 previousPosition { get; private set; }
    public Vector3 moveDirection { get; private set; }

    void Start()
    {
        moveDirection = Vector3.zero;
        previousPosition = transform.position;
    }

    void Update()
    {
        if(Vector3.Distance(transform.position , previousPosition) >= 1f)
        {
            moveDirection = transform.position - previousPosition;
            moveDirection = moveDirection.normalized;
            previousPosition = transform.position;
        }

        // Debug Εγ₯ά
        //Debug.DrawRay(transform.position, normalizedDirection, Color.green);
        //Debug.Log($"Move Dir: {normalizedDirection}, Speed: {moveDirection.magnitude / Time.deltaTime:F2}");
    }

}

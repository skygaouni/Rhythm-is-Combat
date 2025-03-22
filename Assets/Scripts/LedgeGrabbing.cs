using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

// 學習ProBuilder
public class LedgeGrabbing : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement pm;
    public Transform orientation;
    public Transform cam;
    public Rigidbody rb;

    [Header("Ledge Grabbing")]
    public float moveToLedgeSpeed;
    public float maxLedgeGrabDistance;

    public float maxTimeOnLedge;
    private float timerOnLedge;

    public bool holding; // 當player離ledge足夠近(player不用抓到ledge)，holding就會被設為true，此時timer就會開始計時

    [Header("Ledge Jumping")]
    public KeyCode jumpKey = KeyCode.Space;
    public float ledgeJumpForwardForce;
    public float ledgeJumpUpwardForce;

    [Header("Ledge Detection")]
    public float ledgeDetectionLength;
    public float ledgeSphereCastRadius;
    public LayerMask whatIsLedge;

    private Transform lastLedge;
    private Transform currLedge;

    private RaycastHit ledgeHit;

    [Header("ExitingLedge")]
    public bool exitingLedge; // 離開ledge後，需要經過exitLedgeTime後才能再到下一個Ledge或原Ledge
    public float exitLedgeTime;
    private float exitLedgeTimer;

    private void Update()
    {
        LedgeDetection();    
        StateMachine();
    }

    private void StateMachine()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical"); 
        bool anyInpuyKeyPressed = horizontalInput != 0 || verticalInput != 0;

        // State 1 - Holding onto ledge
        if (holding)
        {
            FreezeRigidBodyOnLedge();

            timerOnLedge -= Time.deltaTime;

            // 當玩家在ledge的時間超出預設時間且玩家按下WASD，離開ledge
            if (timerOnLedge <= 0 && anyInpuyKeyPressed) ExitLedgeHold();

            if (Input.GetKeyDown(jumpKey)) LedgeJump();
        }
        // State 2 - Exiting Ledge
        else if (exitingLedge)
        {
            if (exitLedgeTimer > 0) exitLedgeTimer -= Time.deltaTime;
            else exitingLedge = false;
        }
    }

    private void LedgeDetection()
    {
        
        bool ledgeDetected = Physics.SphereCast(transform.position, ledgeSphereCastRadius, cam.forward, out ledgeHit, ledgeDetectionLength, whatIsLedge);

        if (!ledgeDetected) return;

        float distanceToLedge = Vector3.Distance(transform.position, ledgeHit.transform.position);

        // 刪除可能不會有問題，但效能會變差
        if (ledgeHit.transform == lastLedge) return;

        if (distanceToLedge < maxLedgeGrabDistance && !holding)
        {
            Debug.Log("LedgeDetection");
            EnterLedgeHold();
        }   
    }

    private void LedgeJump()
    {
        Debug.Log("LedgeJump");

        if (pm.grounded) return;
        ExitLedgeHold();

        Invoke(nameof(DelayedJumpForce), 0.05f);
    }

    private void DelayedJumpForce()
    {
        Vector3 forceToApply = cam.forward * ledgeJumpForwardForce + orientation.up * ledgeJumpUpwardForce;
        rb.velocity = Vector3.zero;
        rb.AddForce(forceToApply, ForceMode.Impulse);
    }

    private void EnterLedgeHold()
    {
        holding = true;

       // 不知道用途
        // pm.unlimited = true;
        pm.restricted = true;

        currLedge = ledgeHit.transform;
        lastLedge = ledgeHit.transform;

        // 可能可以省略
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    private void FreezeRigidBodyOnLedge()
    {
        rb.useGravity = false;
        Vector3 directionToLedge = (currLedge.position - transform.position).normalized; 
        float distanceToLedge = Vector3.Distance(transform.position, currLedge.position);

        // move player towards ledge
        if(distanceToLedge > 1f)
        {
            if (rb.velocity.magnitude < moveToLedgeSpeed)
                rb.AddForce(directionToLedge * moveToLedgeSpeed * 1000f * Time.deltaTime);
            //else
              //  rb.velocity = rb.velocity.normalized * moveToLedgeSpeed;
            
        }
        else
        {
            Debug.Log("freeze"); 
            if (!pm.freeze) pm.freeze = true;
            if(pm.unlimited) pm.unlimited = false;
        }

        // Exiting if something goes wrong
        if(distanceToLedge > maxLedgeGrabDistance) ExitLedgeHold();
    }

    private void ExitLedgeHold()
    {
        //Debug.Log("ExitLedgeHold");

        exitingLedge = true;
        exitLedgeTimer = exitLedgeTime;

        holding = false;
        timerOnLedge = maxTimeOnLedge;

        pm.freeze = false;
        pm.restricted = false;

        rb.useGravity = true;

        StopAllCoroutines();
        Invoke(nameof(ResetLastLedge), 1f);
    }

    private void ResetLastLedge()
    {
        lastLedge = null;
    }
}

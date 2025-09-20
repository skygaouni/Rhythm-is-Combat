using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Climbing : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Rigidbody rb;
    public PlayerMovement pm;
    private LedgeGrabbing lg;
    public LayerMask whatIsWall;

    [Header("Climbing")]
    public float climbSpeed; //q: 與PlayerMovement裡面的差別
    public float maxClimbTime;
    private float climbTimer;

    //private bool climbing;

    [Header("ClimbJumping")]
    public float climbJumpUpForce;
    public float climbJumpBackForce;

    public KeyCode jumpKey = KeyCode.Space;
    public int climpJumps;
    private int climbJumpsLeft;

    [Header("Detection")]
    public float detectionLength;
    public float sphereCastRadius;
    public float maxWallLookAngle;
    private float wallLookAngle;

    private RaycastHit frontWallHit;
    private bool wallFront;

    private Transform lastWall;
    private Vector3 lastWallNormal;
    public float minWallNormalAngleChange;

    [Header("Exiting")]
    public bool exitingWall;  //跳離開牆的瞬間為true，跳離exitWalltime時間後會歸零為false
    public float exitWallTime; //這段時間都不能碰牆
    private float exitWallTimer;

    private void Start()
    {
        lg = GetComponent<LedgeGrabbing>();    
    }

    private void Update()
    {
        //if (wallFront) Debug.Log(wallFront);
        WallCheck();
        StateMachine();

        if (pm.climbing && !exitingWall) ClimbingMovement();
    }

    private void StateMachine()
    {
        // State 0 - Ledge Grabbing
        if (lg.holding)
        {
            if(pm.climbing) StopClimbing();

            // 不這樣的話，climb會還在繼續
        }

        // State 1 - Climbing
        else if (wallFront && Input.GetKey(KeyCode.W) && wallLookAngle < maxWallLookAngle && !exitingWall)
        {
            Debug.Log("climbing");

            if (!pm.climbing && climbTimer > 0) StartClimbing();

            // timer
            if (climbTimer > 0) climbTimer -= Time.deltaTime;
            if (climbTimer <= 0) StopClimbing();

        }

        // State 2 - Exiting
        else if (exitingWall)
        {
            if (pm.climbing) StopClimbing();

            // timer
            if (exitWallTimer > 0) exitWallTimer -= Time.deltaTime;
            if (exitWallTimer <= 0) exitingWall = false;

        }
        // State 3 - None
        else
        {
            if (pm.climbing) StopClimbing();
        }

        if (wallFront && Input.GetKeyDown(jumpKey) && climbJumpsLeft > 0 && pm.readyToJump)
            ClimbJump();

    }
    // 檢查面前是不是有牆，且這道牆是否與原先爬的牆相同
    private void WallCheck()
    {
        wallFront = Physics.SphereCast(transform.position, sphereCastRadius, orientation.forward, out frontWallHit, detectionLength, whatIsWall);
        Debug.DrawRay(transform.position, orientation.forward * detectionLength, Color.red);

        wallLookAngle = Vector3.Angle(orientation.forward, -frontWallHit.normal);
        bool newWall = frontWallHit.transform != lastWall || Mathf.Abs(Vector3.Angle(lastWallNormal, frontWallHit.normal)) > minWallNormalAngleChange;

        if ((wallFront && newWall) || pm.grounded)
        {
            climbTimer = maxClimbTime;
            climbJumpsLeft = climpJumps;
        }
    }

    private void StartClimbing()
    {
        pm.climbing = true;
        pm.climbing = true;

        lastWall = frontWallHit.transform;
        lastWallNormal = frontWallHit.normal;


        // camera fov setting
    }

    private void ClimbingMovement()
    {
        Debug.Log("ClimbingMovement");
        rb.velocity = new Vector3(rb.velocity.x, climbSpeed, rb.velocity.z);

        // sound effect
    }

    private void StopClimbing()
    {
        Debug.Log("StopClimbing");
        //climbing = false;
        pm.climbing = false;

        // particle effect
    }

    private void ClimbJump()
    {
        if (pm.grounded) return;
        if (lg.holding || lg.exitingLedge) return;
        if (pm.state == PlayerMovement.MovementState.wallrunning) return;

        exitingWall = true;
        exitWallTimer = exitWallTime;

        Debug.Log(frontWallHit.normal);

        // 將frontWallHit投影至地面，防止爬到頂端那刻除了transform.up的向上的力還多施加frontWallHit.normal的y分量的力
        // e.x. frontWallHit.normal為(0.00, 0.97, -0.24),(0.00, 0.63, 0.78)
        Vector3 forceToApply = transform.up * climbJumpUpForce + GetSlopeMoveDirection(frontWallHit.normal) * climbJumpBackForce;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceToApply, ForceMode.Impulse);

        climbJumpsLeft--;
    }

    // 將moveDirection透過ProjectOnPlane投影到地面 -> moveDirection 垂直於y軸
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, orientation.up).normalized;
    }
}

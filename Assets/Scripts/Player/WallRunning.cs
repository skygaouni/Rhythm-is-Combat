using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;

// q1: multiple wall jump on the same wall
// q2: maxWallRunTime������g�LexitWallTime�S���W�}�l�s��WallRun�A�o�˷|�Ϧb�ۦP��WallRun�U�ǩǪ��C
// (Solved)q3: �S��k�b������ݨ�(�q����AddForce���O�q�Ӥp) -> �N�b�Ť����t�שw��walSpeed�w�ѨM�O�q�j�p�����D
public class WallRunning : MonoBehaviour
{
    [Header("WallRunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallRunForce;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallClimbSpeed;
    public float maxWallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    public KeyCode jumpkey = KeyCode.Space;
    public KeyCode upwardsWallRunKey = KeyCode.LeftShift;
    public KeyCode downwardsWallRunKey = KeyCode.LeftControl;
    private bool upwardsWallRunning;
    private bool downwardsWallRunning;
    private float horizontalInput;
    private float verticalInput;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("Exiting")]
    private bool exitingWall; //�����}��������true�A����exitWalltime�ɶ���|�k�s��false
    public float exitWalltime;
    private float exitWalltimer;

    [Header("Gravity")]
    public bool useGravity;
    public float gravityCounterForce;

    [Header("References")]
    public Transform orientation;
    public PlayerCam cam;
    public Camera camCamera;
    private PlayerMovement pm;
    private LedgeGrabbing lg;
    private Rigidbody rb;

    private float originalFov;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
        lg = GetComponent<LedgeGrabbing>();
        originalFov = camCamera.fieldOfView;
    }

    private void Update()
    {
        CheckForWall();
        StateMachine();
    }

    private void FixedUpdate()
    {
        if(pm.wallrunning)
            WallRunningMovement();
    }

    private void CheckForWall()
    {
        // hit.collider	������ �I���� (Collider)�C
        // hit.collider.gameObject ������ �C������C
        // hit.point �������@�ɮy��(Vector3)�A�Y�g�u�P����ۥ檺�I�C
        // hit.normal ���������k�u��V(Vector3)�A�i�H�ΨӧP�_�a�Ϊ����סC
        // hit.distance �g�u�o�g�I�������I���Z���C
        // hit.rigidbody �p�G���������� Rigidbody�A�i�H�s�����C
        // hit.transform �������骺 Transform(�M hit.collider.gameObject.transform �@��)�C
        // �g�u�_�I�]�����m�^, �g�u��V�]����k���^, �x�s�����������T, �g�u���ס]�˴��d��^, Layer Mask�]���w�n�������������O�^
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);


    }

    // �n����a���W�@�w���פ~��WallRunning 
    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        // Geting Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        upwardsWallRunning = Input.GetKey(upwardsWallRunKey);
        downwardsWallRunning = Input.GetKey(downwardsWallRunKey);

        // State 1 - WallRunning 
        // verticalInput > 0 -> press W key
        if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround() && !exitingWall)
        {
            //Debug.Log("StartWallRunning");
            if (!pm.wallrunning)
                StartWallRun();

            if (wallRunTimer > 0)
                wallRunTimer -= Time.deltaTime;
            
            if(wallRunTimer <= 0 && pm.wallrunning)
            {
                exitingWall = true;
                exitWalltimer = exitWalltime;
            }

            if (pm.state == PlayerMovement.MovementState.wallrunning && Input.GetKeyDown(jumpkey) && pm.readyToJump)
            {
                Debug.Log(pm.state); 
                WallJump();
            }
                
 
        }
        // State 2 - Exiting
        else if (exitingWall)
        {
            if (pm.wallrunning)
                StopWallRun();

            if (exitWalltimer > 0)
                exitWalltimer -= Time.deltaTime;

            if(exitWalltimer <= 0)
                exitingWall = false; 
        }
        // State 3 - None
        else
        {
            if(pm.wallrunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        Debug.Log("StartWallRun");
        pm.wallrunning = true;
        wallRunTimer = maxWallRunTime;

        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // apply camera effects
        cam.DoFov(originalFov);
        if (wallLeft)  cam.DoTilt(-5f);
        if (wallRight) cam.DoTilt(5f);
    }

    private void WallRunningMovement()
    {
        //Debug.Log("WallRunningMovement");

        rb.useGravity = useGravity;
        
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);
        
        // ��������𪺤O(wall forward)�P���a�¦V�ۦP
        if((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        // forward force
        rb.AddForce(wallForward * wallRunForce, ForceMode.Force);

        // upwards/downwards force
        if (upwardsWallRunning)
            rb.velocity = new Vector3(rb.velocity.x, wallClimbSpeed, rb.velocity.z);
        if(downwardsWallRunning)
            rb.velocity = new Vector3(rb.velocity.x, -wallClimbSpeed, rb.velocity.z);

        // push to wall force
        if (!(wallLeft && horizontalInput > 0) && !(wallRight && verticalInput < 0))
        {
            rb.AddForce(-wallForward * 100, ForceMode.Force);
            //Debug.Log("Add Force");
        }
        
        if(useGravity)
            rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
    }

    private void StopWallRun()
    {
        //Debug.Log("StopWallRun");
        pm.wallrunning = false;

        // reset camera effects
        cam.DoFov(80f);
        cam.DoTilt(0);
    }

    // �b��W��
    private void WallJump()
    {
        Debug.Log("WallRunningJump");

        if (pm.grounded) return;
        if (lg.holding || lg.exitingLedge) return;

        //enter exiting wall state
        exitingWall = true;
        exitWalltimer = exitWalltime; 
        
        //Debug.Log("WallJump");
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
        Vector3 forceApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        // reset y velocity add jump force
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(forceApply, ForceMode.Impulse);

    }
}


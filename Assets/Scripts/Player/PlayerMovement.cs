using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerMovement : MonoBehaviour
{
    //[Header("Player IK")]
    //public Transform upperbodyTarget; // �����s�y
    //public Transform upperbodyHint;
    

    [Header("Movement")]
    public float moveSpeed; // �u�ޥ������ʳt�סA����y��V�t��
    public float currentSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;
    public float climbSpeed;
    public float airMinSpeed;
    public float swingSpeed;

    public float desiredMoveSpeed;
    public float lastDesiredMoveSpeed;

    public float speedIncreasedMultiplier;
    public float speedDecreasedMultiplier;
    public float slopeIncreasedMultiplier;
    public float slopeDecreasedMultiplier;

    [Header("Drag")]
    public float groundDrag;
    public float slopeDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldownTime;
    private float jumpCooldownTimer;
    public float airMultiplier;
    public bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    public bool onSlope;

    [Header("References")]
    public Climbing climbingScript;
    public Transform orientation;
    public Transform camHolder;
    private Animator animator;
    private PlayerGunSelector gunSelector;

    float horizontalInput;
    float verticalInput;

    public Vector3 moveDirection; // x�b�Pz�b���X��V

    Rigidbody rb;

    [Header("Player State")]
    public MovementState state;

    public enum MovementState
    {
        freeze,
        unlimited,
        grappling,
        swinging,
        walking,
        sprinting,
        wallrunning,
        climbing,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool wallrunning;
    public bool climbing;

    public bool freeze;
    public bool activeGrapple;
    public bool swinging;

    public bool unlimited;
    public bool restricted; // ���� MovePlayer

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        gunSelector = GetComponent<PlayerGunSelector>();

        moveSpeed = walkSpeed;
        desiredMoveSpeed = walkSpeed;
        lastDesiredMoveSpeed = walkSpeed;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // �����]�����z�v�T�ӱ���

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //Debug.Log(rb.velocity.y);
        // ground check 
        // ~~�{�b�� onSlope() �禡�P�_~~
        // Physics.Raycast(�g�u�_�l�I, �g�u��V, �g�u����, LayerMask�]�ϼh�B�n�^�ΨӹL�o�g�u���˴���H)
        grounded = Physics.Raycast(transform.position, Vector3.down, 0.2f, whatIsGround);

        Myinput();
        SpeedControl();
        StateHandler();
        AnimatorControllers();

        if (grounded && !activeGrapple && !swinging)
            rb.drag = groundDrag;
        else if(OnSlope())
            rb.drag = slopeDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        //bodyTarget.rotation = camHolder.transform.rotation;
        MovePlayer();
    }

    private void Myinput()
    {
        if (Keyboard.current.rKey.isPressed && 
            gunSelector.activeGun.currentMagazineCapacity != gunSelector.activeGun.shootConfig.magazineCapacity)
        {
            int layerIndex = animator.GetLayerIndex("Gun Layer");
            AnimatorStateInfo gunLayerStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);

            if (!gunLayerStateInfo.IsName("Reloading"))
            {
                gunSelector.activeGun.canShoot = false;
                animator.SetBool("Reloading", true);
            }
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");// �]A/D �� ��/���^
        verticalInput = Input.GetAxisRaw("Vertical");// �]W/S �� ��/���^

        if(grounded || OnSlope())
        {
            // start jump
            if (Input.GetKey(jumpKey) && readyToJump)
            {
                //Debug.Log("Jump");
                readyToJump = false;
                Jump();
                //Invoke(nameof(ResetJump), jumpCooldown);
            }

            // start crouch
            else if (Input.GetKeyDown(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
                rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            }

            // stop crouch
            else if (Input.GetKeyUp(crouchKey))
            {
                transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            }
        }
    }

    bool keepMomentum;
    private void StateHandler()
    {

        // mode - jumping
        if (!readyToJump) ResetJump();

        // mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            rb.velocity = Vector3.zero;
            desiredMoveSpeed = 0f;
        }
        // mode - Unlimited
        else if (unlimited)
        {
            state = MovementState.unlimited;
            desiredMoveSpeed = 999f;
            
        }
        else if (activeGrapple)
        {
            state = MovementState.grappling;
        }
        else if (swinging)
        {
            state = MovementState.swinging;
            desiredMoveSpeed = swingSpeed;
        }
        // mode - climbing
        else if (climbing)
        {
            state = MovementState.climbing;
            desiredMoveSpeed = climbSpeed;
        }
        else if(wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed ;
        }
        else if(grounded || OnSlope())
        {
            //mode - Sliding
            if (sliding)
            {
                state = MovementState.sliding;

                if (OnSlope() && rb.velocity.y < 0.1f)
                {
                    desiredMoveSpeed = slideSpeed;
                    keepMomentum = true;
                    //Debug.Log("slideSpeed");
                }
                else
                {
                    //Debug.Log("sliding with sprintSpeed");
                    desiredMoveSpeed = sprintSpeed;
                    //Debug.Log("sprintSpeed");
                }
                //Debug.Log("sliding");
            }
            // mode - Crouching 
            else if (Input.GetKey(crouchKey))  
            {
                state = MovementState.crouching;
                desiredMoveSpeed = crouchSpeed;
                //Debug.Log("moveSpeed = " + crouchSpeed);
            }
            // mode - Sprinting
            else if (Input.GetKey(sprintKey))
            {
                state = MovementState.sprinting;
                desiredMoveSpeed = sprintSpeed;
            }
            // mode - Walking
            else
            {
                state = MovementState.walking;

                desiredMoveSpeed = walkSpeed;
                
            }

        }
        // mode - Air
        else
        {
            state = MovementState.air;

            if (moveSpeed < airMinSpeed)
                desiredMoveSpeed = airMinSpeed;
        }

        bool desiredMoveSpeedHasChanged = desiredMoveSpeed != lastDesiredMoveSpeed;

        if(desiredMoveSpeedHasChanged)
        {
            // �u���q�שY�W�ƤU�Ӫ��t�׷|�C�C����
            if(keepMomentum)
            {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else
            {
                moveSpeed = desiredMoveSpeed;
            }
        }
        lastDesiredMoveSpeed = desiredMoveSpeed;

        //deactivate keepMomentum
        if(Mathf.Abs(desiredMoveSpeed - moveSpeed) < 0.1f) keepMomentum = false;
    }

    // smoothly lerp movementSpeed to desired value
    private IEnumerator SmoothlyLerpMoveSpeed()
    {  
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;
        bool speedIncreased = true ? desiredMoveSpeed - moveSpeed > 0 : desiredMoveSpeed - moveSpeed <= 0;


        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);
            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                if(speedIncreased)
                    time += Time.deltaTime * speedIncreasedMultiplier * slopeIncreasedMultiplier * slopeAngleIncrease;
                else
                    time += Time.deltaTime * speedDecreasedMultiplier * slopeDecreasedMultiplier * slopeAngleIncrease;
            }
            else
            {
                if(speedIncreased)
                    time += Time.deltaTime * speedIncreasedMultiplier;
                else
                    time += Time.deltaTime * speedDecreasedMultiplier;
            }
                

            yield return null;
        }
        moveSpeed = desiredMoveSpeed;
    }
    private void MovePlayer()
    {
        // ����bgrabbing���ɭԲ��ʳy���ؼп�
        if (activeGrapple) return;

        if (swinging) return;

        if (restricted) return;

        if (climbingScript.exitingWall) return;

        //Debug.Log("MovePlayer");
        // calculate movement direction  
        // p.s. orientation �O���󪺱����V(���V)
        // quaternion �O���󪺱��ਤ��
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        //Debug.Log(moveDirection);

        if (OnSlope() && !exitingSlope)
        {
            //Debug.Log("onSlope");
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            // �I�[������׭����������O�H�����Að
            if (rb.velocity.y != 0)
            {
                //rb.AddForce(Vector3.down * 80f, ForceMode.Force);
                Vector3 SlopeNormalForce = Vector3.Cross(moveDirection.normalized, slopeHit.normal).normalized;
                rb.AddForce(SlopeNormalForce * 80f, ForceMode.Force);
            }
                
        }

        // AddForce(�I�O���j�p�P��V, ForceMode.Force �� ����I�[�O�A�Ϩ���v���[�t)
        else if(grounded) 
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        
            

        if(!wallrunning)
            rb.useGravity = !OnSlope();
        
    }

    private void SpeedControl()
    {
        if(activeGrapple) return;

        if(OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
            currentSpeed = rb.velocity.magnitude;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            currentSpeed = flatVel.magnitude;

            // limit velocity if needed (���|�@���Ĩ�)
            if(flatVel.magnitude > moveSpeed)
            {
                //Debug.Log("11");
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);    
            }
        }
            
    }

    private void Jump()
    {
        //Debug.Log("Jump");
        exitingSlope = true;

        //reset y velocity to make sure the jump height is the same 
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        exitingSlope = false;

        if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

        if (jumpCooldownTimer <= 0)
        {
            jumpCooldownTimer = jumpCooldownTime;
            readyToJump = true;
        }
    }

    // �b�U�@���I���ɱҥβ��ʡv���X�С]�}���^
    private bool enableMovementOnNextTouch;
    private Vector3 velocityToSet;
    public void jumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        //Debug.Log(velocityToSet);

        // �W�[delay�A����speed control�ܬ�inactive�e�NSetVelocity
        Invoke(nameof(SetVelocity), 0.1f);

        // ����P�_�X���A3���j�����ResetRestrictions
        //Invoke(nameof(ResetRestrictions), 3f);
    }

    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        //Debug.Log("ResetRestrictions");
        activeGrapple = false;
    }

    //���ΥD�ʩI�s�A�]��Unity �|�b�C����������A�D���ˬd�Ҧ����z���p(�Ҧp�I���BĲ�o�B�i�J�d��B�ƹ��I������)
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("OnCollisionEnter");
        if(enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().StopGrapple();
        }
    }

    public bool OnSlope()
    {
        // slopeHit���Y�����k�u�V�q
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //grounded = true;
            onSlope = true;
            return angle < maxSlopeAngle && angle != 0;
        }
        //grounded = false;
        onSlope = false;
        return false;
    }

    // �NmoveDirection�z�LProjectOnPlane��v��slopeHit.normal -> moveDirection ������ slopeHit
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {   
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    // watch https://www.youtube.com/watch?v=IvT8hjy6q4o
    //trajectoryHeight: �ߪ��u���I����(�۹�player�A�D�@�ɮy�а���)
    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y; 
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        //�q�_�I(�o�̩w�_�I���a��)�V�W����ߪ��u�̰��I�һݪ���t�]Y �b��V�^
        float timeToUp = Mathf.Sqrt(-2 * (trajectoryHeight - 0) / gravity);
        float timeToDown = Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (timeToUp + timeToDown);

        //Debug.Log(displacementY - trajectoryHeight);

        return velocityXZ + velocityY;
    }

    private void AnimatorControllers()
    {
        float xVelocity = Vector3.Dot(moveDirection.normalized, transform.right);
        float zVelocity = Vector3.Dot(moveDirection.normalized, transform.forward);

        animator.SetFloat("xVelocity", xVelocity, .1f, Time.deltaTime);
        animator.SetFloat("zVelocity", zVelocity, .1f, Time.deltaTime);
    }

    public void StopMoving()
    {
        StopAllCoroutines();
    }
}


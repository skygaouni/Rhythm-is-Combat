using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed; // 只管平面移動速度，不管y方向速度
    public float currentSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;
    public float wallrunSpeed;

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
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

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

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection; // x軸與z軸的合方向

    Rigidbody rb;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air
    }

    public bool sliding;
    public bool wallrunning;

    private void Start()
    {
        moveSpeed = walkSpeed;
        desiredMoveSpeed = walkSpeed;
        lastDesiredMoveSpeed = walkSpeed;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // 防止角色因為物理影響而旋轉

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        //Debug.Log(rb.velocity.y);
        // ground check 
        // ~~現在由 onSlope() 函式判斷~~
        // Physics.Raycast(射線起始點, 射線方向, 射線長度, LayerMask（圖層遮罩）用來過濾射線的檢測對象)
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        Myinput();
        SpeedControl();
        StateHandler();

        if (grounded)
            rb.drag = groundDrag;
        else if(OnSlope())
            rb.drag = slopeDrag;
        else
            rb.drag = 0;
    }

    private void FixedUpdate()
    {
        
        MovePlayer();
    }

    private void Myinput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");// （A/D 或 ←/→）
        verticalInput = Input.GetAxisRaw("Vertical");// （W/S 或 ↑/↓）
        
        if(grounded || OnSlope())
        {
            // start jump
            if (Input.GetKey(jumpKey) && readyToJump)
            {
                readyToJump = false;
                Jump();
                Invoke(nameof(ResetJump), jumpCooldown);
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

    private void StateHandler()
    {
        if(wallrunning)
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
                    //Debug.Log("slideSpeed");
                }
                else
                {
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
        }

        // check if desiredMoveSpeed has changed drastically
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 4f )
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
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
        // calculate movement direction 
        // p.s. orientation 是物件的旋轉方向(面向)
        // quaternion 是物件的旋轉角度
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            //Debug.Log("onSlope");
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            // 施加垂直於斜面的模擬重力以防止顛簸
            if (rb.velocity.y != 0)
            {
                //rb.AddForce(Vector3.down * 80f, ForceMode.Force);
                Vector3 SlopeNormalForce = Vector3.Cross(moveDirection.normalized, slopeHit.normal).normalized;
                rb.AddForce(SlopeNormalForce * 80f, ForceMode.Force);
            }
                
        }

        // AddForce(施力的大小與方向, ForceMode.Force → 持續施加力，使角色逐漸加速)
        else if(grounded) 
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        if(!wallrunning)
            rb.useGravity = !OnSlope();
        
    }

    private void SpeedControl()
    {
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

            // limit velocity if needed (不會一直衝刺)
            if(flatVel.magnitude > moveSpeed)
            {
                Debug.Log("11");
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);    
            }
        }
            
    }

    private void Jump()
    {
        exitingSlope = true;

        //reset y velocity to make sure the jump height is the same 
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    public bool OnSlope()
    {
        // slopeHit為坡面的法線向量
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
    
    // 將moveDirection透過ProjectOnPlane投影到slopeHit.normal -> moveDirection 垂直於 slopeHit
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {   
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }
}


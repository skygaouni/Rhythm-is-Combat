using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Sliding : MonoBehaviour
{
    [Header("References")]
    public Transform orientaztion;
    public Transform playerObj;
    private Rigidbody rb;
    private PlayerMovement pm;

    [Header("Sliding")]
    public float maxSlidingTime;
    public float slideForce;
    private float slideTimer;

    public float slideYScale;
    private float startYScale;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.LeftControl;
    private float horizontalInput;
    private float verticalInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();

        startYScale = playerObj.localScale.y;
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A + D
        verticalInput = Input.GetAxisRaw("Vertical");// W + S

        if (Input.GetKeyDown(slideKey) && (horizontalInput != 0 || verticalInput != 0))
        {
            StartSlide();
        }
            

        if(Input.GetKeyUp(slideKey) && pm.sliding && pm.state == PlayerMovement.MovementState.sliding)
            StopSlide();
    }

    private void FixedUpdate()
    {
        if (pm.sliding && pm.state == PlayerMovement.MovementState.sliding)
            SlidingMovement();
    }

    private void StartSlide()
    {
        pm.sliding = true;

        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);

        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

        slideTimer = maxSlidingTime;
    }

    private void SlidingMovement()
    {
        Vector3 inputDirection = orientaztion.forward * verticalInput + orientaztion.right * horizontalInput;

        // sliding normal -> 在地面上走或是沿斜坡往上
        if (!pm.OnSlope() || rb.velocity.y > -0.1f)
        {
            rb.AddForce(inputDirection.normalized * slideForce, ForceMode.Force);

            // Time.deltaTime 是 上一幀到現在的時間間隔
            slideTimer -= Time.deltaTime;
        }
        else
        {
            // 從斜坡滑下來的時候不用計時，可以一直滑
            rb.AddForce(pm.GetSlopeMoveDirection(inputDirection) * slideForce, ForceMode.Force);
        }

        if (slideTimer <= 0)
            StopSlide();
    }

    private void StopSlide()
    {
        pm.sliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);
    }
}

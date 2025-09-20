using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;
using UnityEngine.UIElements;

public class Swinging : MonoBehaviour
{
    private Vector3 currentGrapplePosition;

    [Header("References")]
    public LineRenderer lr;
    public Transform gunTip, cam, player;
    public LayerMask whatIsGrappleable;
    public PlayerMovement pm;

    [Header("Swinging")]
    private float maxSwingDistance = 25f;
    private Vector3 swingPoint;
    private SpringJoint joint; // 類似彈簧

    [Header("Input")]
    public KeyCode swingKey = KeyCode.Mouse3;

    [Header("OdmGear")]
    public Transform orientaiton;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendRate;

    [Header("Prediction")]
    public RaycastHit predictionHit;
    public float predictionSphereCastRadius;
    public Transform predictionPoint;

    private void Update()
    {
        if (Input.GetKeyDown(swingKey)) StartSwing();
        if (Input.GetKeyUp(swingKey)) StopSwing();

        CheckForSwingPoints();

        if(joint != null) OdmGearMovement();
    }

    private void LateUpdate()
    {
        DrawRope();    
    }

    private void StartSwing()
    {
        // return if predictionHit ont found 
        //if (predictionHit.point == Vector3.zero) return;
        Debug.Log("StartSwing");

        RaycastHit hit;
        if(Physics.Raycast(cam.position, cam.forward, out hit, maxSwingDistance, whatIsGrappleable))
        {
            GetComponent<Grappling>().StopGrapple();
            pm.ResetRestrictions();

            pm.swinging = true;

            swingPoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false; // 關掉 Unity 自動設定連接點
            joint.connectedAnchor = swingPoint; // 手動指定鉤繩要拉過去的目標點

            float distanceFromPoint = Vector3.Distance(player.position, swingPoint);

            //玩家角色與鉤繩目標點 (swingPoint) 之間可活動的距離範圍
            joint.maxDistance = distanceFromPoint * 0.8f; // 彈簧最長拉伸距離
            joint.minDistance = distanceFromPoint * 0.25f; // 彈簧最短壓縮長度 -> 不設為0是因為避免太靠近造成穿牆等bug

            //customize values as you like
            joint.spring = 4.5f; // 設定彈簧的拉力強度
            joint.damper = 7f; // 設定彈簧的阻尼系數 —> 用來抑制彈簧的晃動或來回彈跳。
            joint.massScale = 4.5f; // 設定這個彈簧對物體質量的影響程度 (>1.0：不管物體多重，彈簧都更能拉得動它 ; <1.0：彈簧會比較容易受質量影響)

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }
    }

    public void StopSwing()
    {
        pm.swinging = false;

        lr.positionCount = 0; // 清空線條，讓繩子消失不再渲染。
        Destroy(joint); // 清空彈簧
    }

    void DrawRope()
    {
        // if not grapping, don't draw rope
        if (!joint) return;
        //Debug.Log("DrawRope");

        currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, swingPoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, swingPoint);
    }

    // 三維機動裝置系統
    private void OdmGearMovement()
    {
        //private float horizontalInput = Input.GetAxisRaw("Horizontal");// （A/D 或 ←/→）
        //private float verticalInput = Input.GetAxisRaw("Vertical");// （W/S 或 ↑/↓）
        //private moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //right 
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientaiton.right * horizontalThrustForce * Time.deltaTime);
        //left
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientaiton.right * horizontalThrustForce * Time.deltaTime);
        //forward 
        if (Input.GetKey(KeyCode.W))
        {
            rb.AddForce(orientaiton.forward * forwardThrustForce * Time.deltaTime);
        }
        // shorten cable
        if (Input.GetKey(KeyCode.Space))
        {
            Vector3 directionToPoint = swingPoint - transform.position;
            rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

            float distanceFromPoint = Vector3.Distance(transform.position, swingPoint);

            joint.maxDistance = distanceFromPoint * 0.8f;
            joint.minDistance = distanceFromPoint * 0.25f;
        }

        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            float extendDistanceFromPoint = Vector3.Distance(transform.position, swingPoint) + extendRate;

            joint.maxDistance = extendDistanceFromPoint * 0.8f;
            joint.minDistance = extendDistanceFromPoint * 0.25f;
        }
    }

    private void CheckForSwingPoints()
    {
        if (joint != null) return;

        RaycastHit sphereCastHit;
        Physics.SphereCast(cam.position, predictionSphereCastRadius, cam.forward, 
            out sphereCastHit, maxSwingDistance, whatIsGrappleable);

        RaycastHit raycastHit;
        Physics.Raycast(cam.position, cam.forward, out raycastHit, maxSwingDistance, whatIsGrappleable);

        Vector3 realHitPoint;
        // Option 1 - Direct Hit
        if(raycastHit.point != Vector3.zero)
            realHitPoint = raycastHit.point;
        
        // Option 2 - Indirect (Predicted) Hit
        else if(sphereCastHit.point != Vector3.zero)
            realHitPoint = sphereCastHit.point;

        // Option3 - Miss
        else 
            realHitPoint = Vector3.zero;

        // realHitPoint found 
        if(realHitPoint != Vector3.zero)
        {
            //predictionPoint.gameObject.SetActive(true);
            predictionPoint.position = realHitPoint; 
        }
        // realHitPoint not found
        else
        {
            predictionPoint.gameObject.SetActive(false);
        }

        predictionHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

}

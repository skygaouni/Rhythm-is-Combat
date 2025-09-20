using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class Grappling: MonoBehaviour
{
    [Header("References")]
    private PlayerMovement pm;
    public Transform cam;
    public Transform gunTip;
    public LayerMask whatIsGrappleable;
    public LineRenderer lr; 

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime; // 從開始出勾到真正執行出鉤的時間 -> 這段時間可以用來做出鉤的動作 
    public float overShootYAxis; // 拋物線高度固定離 (起始點高度 > 勾住的點高度 ? 起始點高度 : 勾住的點高度) 高overShootYAxis

    private Vector3 grapplePoint;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.Mouse1;

    private bool grappling;

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(grappleKey))
        {
            //Debug.Log("Press grappleKey");
            StartGrapple();
        }
        if(grapplingCdTimer > 0) grapplingCdTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if(grappling) lr.SetPosition(0, gunTip.position);    
    }

    private void StartGrapple()
    {
        if (grapplingCdTimer > 0) return;

        GetComponent<Swinging>().StopSwing();

        grappling = true;

        pm.freeze = true;

        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;
            Debug.Log("ExecuteGrapple");

            Invoke(nameof(ExecuteGrapple), grappleDelayTime);
        }
        else 
        {
            grapplePoint = cam.position + cam.forward * maxGrappleDistance;

            Invoke(nameof(StopGrapple), grappleDelayTime);
        }

        lr.enabled = true;
        //將 LineRenderer 的第 1 個節點（點）的位置設為 grapplePoint
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        pm.freeze = false;

        //1f為角色高度一半
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overShootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overShootYAxis;

        //Debug.Log("grapplePoint.y : " + grapplePoint.y);
        //Debug.Log("highestPointOnArc : " + highestPointOnArc);


        pm.jumpToPosition(grapplePoint, highestPointOnArc);

        //已經再pm.OnCollisionEnter裡面呼叫過
        //Invoke(nameof(StopGrapple), 1f);
    }

    //若在鉤的過程中碰到東西，或者勾到目標時會被呼叫
    public void StopGrapple()
    {
        Debug.Log("StopGrapple");

        pm.freeze = false;

        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }


}

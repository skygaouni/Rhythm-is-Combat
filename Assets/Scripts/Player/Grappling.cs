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
    public float grappleDelayTime; // �q�}�l�X�Ĩ�u������X�_���ɶ� -> �o�q�ɶ��i�H�ΨӰ��X�_���ʧ@ 
    public float overShootYAxis; // �ߪ��u���שT�w�� (�_�l�I���� > �Ħ��I���� ? �_�l�I���� : �Ħ��I����) ��overShootYAxis

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
        //�N LineRenderer ���� 1 �Ӹ`�I�]�I�^����m�]�� grapplePoint
        lr.SetPosition(1, grapplePoint);
    }

    private void ExecuteGrapple()
    {
        pm.freeze = false;

        //1f�����Ⱚ�פ@�b
        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = grapplePoint.y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overShootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overShootYAxis;

        //Debug.Log("grapplePoint.y : " + grapplePoint.y);
        //Debug.Log("highestPointOnArc : " + highestPointOnArc);


        pm.jumpToPosition(grapplePoint, highestPointOnArc);

        //�w�g�Apm.OnCollisionEnter�̭��I�s�L
        //Invoke(nameof(StopGrapple), 1f);
    }

    //�Y�b�_���L�{���I��F��A�Ϊ̤Ĩ�ؼЮɷ|�Q�I�s
    public void StopGrapple()
    {
        Debug.Log("StopGrapple");

        pm.freeze = false;

        grappling = false;

        grapplingCdTimer = grapplingCd;

        lr.enabled = false;
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectBullet : MonoBehaviour
{
    public FSM manager;
    public Transform pos;

    private void Update()
    {
        transform.position = pos.position;
        transform.rotation = pos.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            //Debug.Log("DetectBullet");
            manager.parameter.detectBullet = true;
        }
    }
}

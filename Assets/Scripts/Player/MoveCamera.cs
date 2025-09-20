using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;
    public float yOffest;

    private void Update()
    {
        Vector3 targetPos = cameraPosition.position;
        targetPos.y += yOffest;
        transform.position = targetPos;
    }
}

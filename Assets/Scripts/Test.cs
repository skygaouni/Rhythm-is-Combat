using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float moveSpeed;    // Start is called before the first frame update
    public float lastDesiredMoveSpeed;
    void Start()
    {
        moveSpeed = 35;
    }

    private void Update()
    {
        // check if desiredMoveSpeed has changed drastically
        if (Mathf.Abs(30 - lastDesiredMoveSpeed) > 4f)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = 30;
            //Debug.Log(moveSpeed + "," + desiredMoveSpeed);
        }
        lastDesiredMoveSpeed = 30;
    }
    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        float time = 0;
        float difference = Mathf.Abs(30 - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, 30, time / difference);

            
            float slopeAngleIncrease = 1 + (45 / 90f);

            time += Time.deltaTime * slopeAngleIncrease;
            

            yield return null;
        }

        moveSpeed = 30;
    }
}

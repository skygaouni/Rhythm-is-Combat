using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    private Animator animator;

    [Header("Input")]
    public KeyCode fireKey = KeyCode.Mouse0;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(fireKey))
            Fire();
    }

    private void Fire()
    {
        Debug.Log("Fire");
        animator.SetTrigger("fire");
    }
}

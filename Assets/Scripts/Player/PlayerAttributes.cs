using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttributes : MonoBehaviour
{
    public PlayerHealth health;
    public PlayerMovement movement;
    public PlayerPainResponse painResponse;
    public bool isDied = false;
    public GameObject Gameover;

    private void Start()
    {
        health.onTakeDamage += painResponse.HandlePain;
        health.onDeath += Die;
    }

    private void Die(Vector3 position)
    {
        isDied = true;
        painResponse.HandleDeath();
        movement.StopMoving();
        Gameover.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}

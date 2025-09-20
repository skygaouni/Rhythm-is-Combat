using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogWarning("�䤣�� Player�I�нT�w�� Tag �� 'Player' ������b�������C");
        }
    }

    void Update()
    {
        if (playerTransform != null)
        {
            transform.LookAt(playerTransform.position);
        }
    }
}
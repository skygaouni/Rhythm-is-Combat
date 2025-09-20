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
            Debug.LogWarning("找不到 Player！請確定有 Tag 為 'Player' 的物件在場景中。");
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
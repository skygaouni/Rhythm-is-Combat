using UnityEngine;

public class BodyDirectionController : MonoBehaviour
{
    [Header("References")]
    public Transform holder;

    public float distanceFront = 2f;
    public float distanceDown = 2f;
    public float distanceRight = 1.3f;

    void LateUpdate()
    {
        transform.position = holder.position + holder.forward * distanceFront +
                holder.right * distanceRight - holder.up * distanceDown;

    }
}

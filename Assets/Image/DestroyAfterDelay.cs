using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    public float delay = 2f; // �w�] 2 ��

    void Start()
    {
        Destroy(gameObject, delay);
    }
}

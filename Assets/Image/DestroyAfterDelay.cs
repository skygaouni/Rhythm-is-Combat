using UnityEngine;

public class DestroyAfterDelay : MonoBehaviour
{
    public float delay = 2f; // ¹w³] 2 ¬í

    void Start()
    {
        Destroy(gameObject, delay);
    }
}

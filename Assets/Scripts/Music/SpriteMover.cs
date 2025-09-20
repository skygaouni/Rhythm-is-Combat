using UnityEngine;

public class SpriteMover : MonoBehaviour
{
    public Vector2 moveDirection = Vector2.left;  // 2D��V�G�Ҧp (1,1)�B(0,1) ��
    public float moveDistance = 5.0f;
    public float moveSpeed = 3.0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // �O����l��m�A�O�d Z �b
        startPosition = transform.position;

        // �p��ؼЦ�m�A�u�� XY�AZ �O������
        targetPosition = startPosition + new Vector3(moveDirection.normalized.x, moveDirection.normalized.y, 0f) * moveDistance;
    }

    void Update()
    {
        // ���ʡ]Z �b�T�w�^
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // ��F�ؼЫ�P��
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // ScoreManager.instance.MinusScore(100);
            Destroy(gameObject);
        }
    }
}

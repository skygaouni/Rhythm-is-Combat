using UnityEngine;

public class SpriteMover : MonoBehaviour
{
    public Vector2 moveDirection = Vector2.left;  // 2D方向：例如 (1,1)、(0,1) 等
    public float moveDistance = 5.0f;
    public float moveSpeed = 3.0f;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // 記錄原始位置，保留 Z 軸
        startPosition = transform.position;

        // 計算目標位置，只動 XY，Z 保持不變
        targetPosition = startPosition + new Vector3(moveDirection.normalized.x, moveDirection.normalized.y, 0f) * moveDistance;
    }

    void Update()
    {
        // 移動（Z 軸固定）
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 到達目標後銷毀
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            // ScoreManager.instance.MinusScore(100);
            Destroy(gameObject);
        }
    }
}

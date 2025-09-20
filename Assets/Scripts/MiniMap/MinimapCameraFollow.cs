using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform target;         // 指定 CamaraHolder
    public float height = 20f;       // 小地圖鏡頭高度
    public Vector3 fixedRotation = new Vector3(90f, 0f, 0f); // 固定俯視角度

    void LateUpdate()
    {
        if (target == null) return;

        // 跟隨 XZ 位置，但固定 Y 高度
        transform.position = new Vector3(target.position.x, height, target.position.z);

        // 固定旋轉，不跟隨目標旋轉
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}

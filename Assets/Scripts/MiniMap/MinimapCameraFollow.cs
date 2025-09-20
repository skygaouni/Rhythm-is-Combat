using UnityEngine;

public class MinimapCameraFollow : MonoBehaviour
{
    public Transform target;         // ���w CamaraHolder
    public float height = 20f;       // �p�a�����Y����
    public Vector3 fixedRotation = new Vector3(90f, 0f, 0f); // �T�w��������

    void LateUpdate()
    {
        if (target == null) return;

        // ���H XZ ��m�A���T�w Y ����
        transform.position = new Vector3(target.position.x, height, target.position.z);

        // �T�w����A�����H�ؼб���
        transform.rotation = Quaternion.Euler(fixedRotation);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.Animations.Rigging;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation; // the information of where the player's looking(不管上下)
    public Transform camHolder;
    public Transform player;
    //public Animator animator;
    public RigBuilder rigBuilder;


    float xRotation;
    float yRotation;

    private float currentYRotation; // 目前角色真正的平滑朝向

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // 游標鎖定在螢幕中央，無法移動但仍可接收滑鼠增量輸入
        Cursor.visible = false;
    }

    private void Update()
    {
        

        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -75f, 75f); // 防止上下轉動預防反人類

        //rotate cam and orientation
        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0); // 應用在第一人稱視角相機
        orientation.rotation = Quaternion.Euler(0, yRotation, 0); // orientation僅記錄玩家朝向資訊

        //player.transform.rotation = orientation.rotation;
        player.transform.rotation = Quaternion.Euler(0, orientation.eulerAngles.y, 0);
    }

    private void LateUpdate()
    {
        
        /*float rotationSpeed = 360f; // 旋轉速度，可以自己調

        player.transform.rotation = Quaternion.Slerp(
            player.transform.rotation,
            orientation.rotation,
            rotationSpeed * Time.deltaTime
        );*/

        // 清除舊的Rig Handles
        //animator.UnbindAllStreamHandles();
        //animator.UnbindAllSceneHandles();

        
    }

    public void DoFov(float endValue)
    {
        // 讓相機的 FOV 在 0.25 秒內 平滑地變化到 endValue
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}

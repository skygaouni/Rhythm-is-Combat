using Uni.Guns.Demo;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerGunSelector : MonoBehaviour
{
    [SerializeField]
    private GunType selectedGunType;
    [SerializeField]
    private Transform gunParent; // 要把槍模型掛上去的位置
    [SerializeField]
    private List<GunScriptableObject> guns;// 所有可用的槍支資料（ScriptableObject 列表）
    [SerializeField]
    private PlayerIK inverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject activeGun; // 目前已經裝備的槍支 ScriptableObject

    private void Start()
    {
        GunScriptableObject gunObject = guns.Find(gunObject => gunObject.type == selectedGunType);

        if (gunObject == null)
        {
            // why filled in is gunObject not selectedGunType
            Debug.LogError($"No GunScriptable found for Guntype: {selectedGunType}");
            return;
        }

        activeGun = gunObject;
        gunObject.Spawn(gunParent, this);

        // some magic for IK
        //inverseKinematics.Setup(gunParent);
    }
}

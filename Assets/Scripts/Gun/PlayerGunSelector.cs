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
    private Transform gunParent; // �n��j�ҫ����W�h����m
    [SerializeField]
    private List<GunScriptableObject> guns;// �Ҧ��i�Ϊ��j���ơ]ScriptableObject �C��^
    [SerializeField]
    private PlayerIK inverseKinematics;

    [Space]
    [Header("Runtime Filled")]
    public GunScriptableObject activeGun; // �ثe�w�g�˳ƪ��j�� ScriptableObject

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

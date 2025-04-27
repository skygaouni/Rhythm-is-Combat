using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;


/// <summary>
/// 儲存每一把槍的資料與功能模組
/// </summary>
[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptabeObject : ScriptableObject
{
    

    //public ImpactType impactType;
    public GunType type;
    public string name;
    public GameObject modelPrefab;
    public Vector3 spawnPoint;
    public Vector3 spawnRotation;

    public ShootConfigScriptableObject shootConfig;
    public TrailConfigScriptableObject trailConfig;

    private MonoBehaviour activeMonoBehaviour;
    private GameObject model;

    private float lastShootTime;
    private float initialClickTime;
    private float stopShootingTime;
    private bool lastFrameWantedToShoot;

    private ParticleSystem shootSystem; // ParticleSystem 是用來產生各種粒子效果的組件
    private ObjectPool<TrailRenderer> trailPool; //子彈飛行軌跡的物件池

    /// <summary>
    /// 把槍模型實體化出來，掛在角色上
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="activeMonoBehaviour"></param>
    public void Spawn(Transform parent, MonoBehaviour activeMonoBehaviour)
    {
        Debug.Log("Spawn");
        this.activeMonoBehaviour = activeMonoBehaviour;

        lastShootTime = 0;
        trailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        model = Instantiate(modelPrefab);
        model.transform.SetParent(parent, false); // false: 把模型的本地座標當成新相對位置
        model.transform.localPosition = spawnPoint; // localPosition: 相對座標
        model.transform.localRotation = Quaternion.Euler(spawnRotation);

        shootSystem = model.GetComponentInChildren<ParticleSystem>();
    }

    /// <summary>
    /// 控制槍的開火、火光、拖尾軌跡、擊中偵測、連射擴散、停火後回正
    /// ✔️ 每次開槍會增加散射程度（根據按住的時間）
    /// ✔️ 如果一段時間沒射擊，擴散程度會逐漸恢復
    /// ✔️ 最終再根據擴散值來調整子彈射出的方向
    /// </summary>
    public void Shoot()
    {
        // 距離上次射擊的時間超過(冷卻時間 + 一幀），表示「一段時間沒開火」了 ➜ 準備恢復準心！
        if (Time.time - lastShootTime - shootConfig.FireRate > Time.deltaTime)
        {
            // 上次射擊持續的時間
            // 0 < lastduration <  shootConfig.maxSpreadTime(按住開火鍵超出最大擴散時間)
            float lastDuration = Mathf.Clamp(0, stopShootingTime - initialClickTime, shootConfig.maxSpreadTime);

            // 計算準心回復進度(還沒回復的比例) -> 若 <= 0 代表準心完全回復
            // lerpTime = 0.25 代表已經恢復 75% → 還剩 25% 擴散
            float lerpTime = (shootConfig.recoilRecoverySpeed - (Time.time - stopShootingTime))
                / shootConfig.recoilRecoverySpeed;

            // 計算最早開火的子彈的時間，當停火後 initialClickTime 會越來越大，使 lastDration 越來越小
            initialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));
        }

        Debug.Log("Shoot");
        if (Time.time > shootConfig.FireRate + lastShootTime)
        {
            lastShootTime = Time.time;
            shootSystem.Play();

            Vector3 spreadAmount = shootConfig.GetSpread(Time.time - initialClickTime);
            model.transform.forward += model.transform.TransformDirection(spreadAmount);

            Vector3 shootDirection = model.transform.forward;
            

            if (Physics.Raycast(
                    shootSystem.transform.position,
                    shootDirection,
                    out RaycastHit hit,
                    float.MaxValue,
                    shootConfig.hitMask
               ))
            {
                Debug.Log("PlayTrail");
                activeMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        shootSystem.transform.position,
                        hit.point,
                        hit
                    )
                );
            }
            else {
                Debug.Log("PlayTrail1");
                activeMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        shootSystem.transform.position,
                        shootSystem.transform.position + (shootDirection * trailConfig.missDistance),
                        new RaycastHit()
                    )
                );
            }

        }
    }

    public void Tick(bool wantedToShoot)
    {
        // 讓模型的 localRotation（本地角度）逐步逼近 spawnRotation
        model.transform.localRotation = Quaternion.Lerp(
            model.transform.localRotation,
            Quaternion.Euler(spawnRotation),
            Time.deltaTime * shootConfig.recoilRecoverySpeed
        );

        if(wantedToShoot)
        {
            lastFrameWantedToShoot = true;
            Shoot();
        }
        else if(!wantedToShoot && lastFrameWantedToShoot)
        {
            stopShootingTime = Time.time;
            lastFrameWantedToShoot = false;
        }
    }

    /// <summary>
    /// 拖尾動畫協程，模擬子彈飛行過程
    /// 1. Get trail from pool ➜
    /// 2. 設定位置 ➜
    /// 3. 等一幀避免殘留 ➜
    /// 4. 啟用拖尾 ➜
    /// 5. 每幀移動 trail 到終點 ➜
    /// 6. 等拖尾時間淡出 ➜
    /// 7. 回收
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="hit"></param>
    /// <returns></returns>
    private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
    {
        

        TrailRenderer instance = trailPool.Get(); 
        instance.gameObject.SetActive(true);
        instance.transform.position = startPoint;
        yield return null; // 暫停一幀然後再繼續往下執行，避免 trail 被重複使用時，還殘留上一次的軌跡位置，導致拖尾瞬間拉出一條奇怪的線

        instance.emitting = true;

        float distance = Vector3.Distance( startPoint, endPoint );
        float remainingDistance = distance;
        while ( remainingDistance > 0 )
        {
            instance.transform.position = Vector3.Lerp(
                startPoint,
                endPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );
            remainingDistance -= trailConfig.simulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = endPoint;

        /*if(hit.collider != null)
        {
            SurfaceManager.Instance.HandleImpact(
                hit.transform.gameObject,
                endPoint,
                hit.normal,
                ImpactType,
                0
            );
        }*/


        yield return new WaitForSeconds(trailConfig.duration); // 拖尾結束後暫停一下，讓玩家看到殘影
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        trailPool.Release(instance);
    }

    private TrailRenderer CreateTrail()
    {
        Debug.Log("CreateTrail");
        GameObject instance = new GameObject("Bullet Trail"); //  建立一個新的空物件，名字是 "Bullet Trail"
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = trailConfig.Color; // 顏色變化（漸層）
        trail.material = trailConfig.material;
        trail.widthCurve = trailConfig.widthCurve; // 寬度變化
        trail.time = trailConfig.duration; 
        trail.minVertexDistance = trailConfig.minVertexDistance; // 每隔多遠加一個點，距離越小越平滑，但效能越差

        trail.emitting = false; // 預設不啟用發射（因為是用 Object Pool，要等真的射擊時才打開）
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // 關掉陰影投射，可提升效能

        return trail;
    }

    /// <summary>
    /// Expected to be called every frame
    /// </summary>
    /// <param name="WantsToShoot">Whether or not the player is trying to shoot</param>
    /*public void Tick(bool WantsToShoot)
    {
        model.transform.localRotation = Quaternion.Lerp(
            model.transform.localRotation,
            Quaternion.Euler(spawnRotation),
            Time.deltaTime * shootConfig.RecoilRecoverySpeed
        );

        if (WantsToShoot)
        {
            lastFrameWantedToShoot = true;
            TryToShoot();
        }

        if (!WantsToShoot && lastFrameWantedToShoot)
        {
            stopShootingTime = Time.time;
            lastFrameWantedToShoot = false;
        }
    }*/
}

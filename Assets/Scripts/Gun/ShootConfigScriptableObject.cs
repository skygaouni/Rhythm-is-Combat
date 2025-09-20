using System.Linq;
using UnityEngine;

// q1: 研究一下 Texture2D.GetRaw
[CreateAssetMenu(fileName = "ShootConfig", menuName = "Guns/Shoot Configuration", order = 2)]
public class ShootConfigScriptableObject : ScriptableObject
{
    public LayerMask hitMask;
    public float FireRate = 0.25f; // 兩次射擊之間最少要間隔多久，才能開下一發
    public float recoilRecoverySpeed = 1f;
    public float maxSpreadTime = 1f;
    public int magazineCapacity = 26; // 彈匣容量

    public BulletSpreadType spreadType = BulletSpreadType.Simple;

    [Header("Simple Spread")]
    public Vector3 Spread = new Vector3(0.1f, 0.1f, 0.1f);

    [Header("Textrue-Based Spread")]
    [Range(0.001f, 5f)]
    public float spreadMultiplier = 0.1f;
    public Texture2D spreadTexture;

    public Vector3 GetSpread(float shootTime = 0)
    {
        Vector3 spread = Vector3.zero;
        
        if(spreadType == BulletSpreadType.Simple)
        {
            spread = Vector3.Lerp(
                    Vector3.zero,
                    new Vector3(
                        UnityEngine.Random.Range(-Spread.x, Spread.x),
                        UnityEngine.Random.Range(-Spread.y, Spread.y),
                        UnityEngine.Random.Range(-Spread.z, Spread.z)
                    ),
                    Mathf.Clamp01(shootTime / maxSpreadTime) // 射擊時間越長，得到的隨機性愈大
                );

        }
        else if(spreadType == BulletSpreadType.TextureBased)
        {
            spread = GetTextureDirection(shootTime);
            spread *= spreadMultiplier;
        }

        

        return spread;
    }

    /// <summary>
    /// manner: 根據 spreadTexture 的灰階分布，隨機取一個點作為子彈的發射方向
    /// introduce: 案住滑鼠的時間越長，就對越大的區域進行採樣，使用灰度，像素越白，選到該像素的可能性愈高
    /// 計算從中心到該像素的方向
    /// </summary>
    /// <param name="shootTime"></param>
    /// <returns></returns>
    private Vector3 GetTextureDirection(float shootTime)
    {
        // 貼圖的中心點位置
        Vector2 halfSize = new Vector2(spreadTexture.width / 2f, spreadTexture.height / 2f);

        // 從貼圖中心向外取樣的區域大小
        int halfSquareExtents = Mathf.CeilToInt(Mathf.Lerp(1, halfSize.x, Mathf.Clamp01(shootTime / maxSpreadTime)));

        // 取樣的左下角
        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;

        // q1
        Color[] sampleColors = spreadTexture.GetPixels(
            minX,
            minY,
            halfSquareExtents * 2,
            halfSquareExtents * 2
        );

        // 不在乎顏色，只在乎灰度(越亮越容易被選中)
        float[] colorAsGrey = System.Array.ConvertAll(sampleColors, (color) => color.grayscale);
        float totalGreyValue = colorAsGrey.Sum();

        float grey = Random.Range(0, totalGreyValue);
        int i = 0; // 找 grey 的落點
        for(; i < colorAsGrey.Length; i++)
        {
            grey -= colorAsGrey[i];
            if (grey <= 0) break;
        }

        // 被選中點的真實位置
        int x = minX + i % (halfSquareExtents * 2);
        int y = minY + i / (halfSquareExtents * 2);

        Vector2 targetPosition = new Vector2(x, y);

        // 除以 halfSize.x 讓 direction 長度變為 -1 ~ 1
        Vector2 direction = (targetPosition - halfSize) / halfSize.x;

        return direction;
    }
}

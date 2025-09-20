using System.Linq;
using UnityEngine;

// q1: ��s�@�U Texture2D.GetRaw
[CreateAssetMenu(fileName = "ShootConfig", menuName = "Guns/Shoot Configuration", order = 2)]
public class ShootConfigScriptableObject : ScriptableObject
{
    public LayerMask hitMask;
    public float FireRate = 0.25f; // �⦸�g�������̤֭n���j�h�[�A�~��}�U�@�o
    public float recoilRecoverySpeed = 1f;
    public float maxSpreadTime = 1f;
    public int magazineCapacity = 26; // �u�X�e�q

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
                    Mathf.Clamp01(shootTime / maxSpreadTime) // �g���ɶ��V���A�o�쪺�H���ʷU�j
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
    /// manner: �ھ� spreadTexture ���Ƕ������A�H�����@���I�@���l�u���o�g��V
    /// introduce: �צ�ƹ����ɶ��V���A�N��V�j���ϰ�i��ļˡA�ϥΦǫסA�����V�աA���ӹ������i��ʷU��
    /// �p��q���ߨ�ӹ�������V
    /// </summary>
    /// <param name="shootTime"></param>
    /// <returns></returns>
    private Vector3 GetTextureDirection(float shootTime)
    {
        // �K�Ϫ������I��m
        Vector2 halfSize = new Vector2(spreadTexture.width / 2f, spreadTexture.height / 2f);

        // �q�K�Ϥ��ߦV�~���˪��ϰ�j�p
        int halfSquareExtents = Mathf.CeilToInt(Mathf.Lerp(1, halfSize.x, Mathf.Clamp01(shootTime / maxSpreadTime)));

        // ���˪����U��
        int minX = Mathf.FloorToInt(halfSize.x) - halfSquareExtents;
        int minY = Mathf.FloorToInt(halfSize.y) - halfSquareExtents;

        // q1
        Color[] sampleColors = spreadTexture.GetPixels(
            minX,
            minY,
            halfSquareExtents * 2,
            halfSquareExtents * 2
        );

        // ���b�G�C��A�u�b�G�ǫ�(�V�G�V�e���Q�襤)
        float[] colorAsGrey = System.Array.ConvertAll(sampleColors, (color) => color.grayscale);
        float totalGreyValue = colorAsGrey.Sum();

        float grey = Random.Range(0, totalGreyValue);
        int i = 0; // �� grey �����I
        for(; i < colorAsGrey.Length; i++)
        {
            grey -= colorAsGrey[i];
            if (grey <= 0) break;
        }

        // �Q�襤�I���u���m
        int x = minX + i % (halfSquareExtents * 2);
        int y = minY + i / (halfSquareExtents * 2);

        Vector2 targetPosition = new Vector2(x, y);

        // ���H halfSize.x �� direction �����ܬ� -1 ~ 1
        Vector2 direction = (targetPosition - halfSize) / halfSize.x;

        return direction;
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Gun Trail Config", order = 4)]
public class TrailConfigScriptableObject : ScriptableObject
{
    public Material material;
    public AnimationCurve widthCurve; // 拖尾的 寬度動畫曲線
    public float duration = 0.5f; // 拖尾存在的時間（秒）
    public float minVertexDistance; // 拖尾軌跡中兩點之間的最小距離
    public Gradient Color; // 和material差別?

    public float missDistance = 100f; //子彈100公尺後消失
    public float simulationSpeed = 100f; 

}

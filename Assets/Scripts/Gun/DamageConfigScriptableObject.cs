using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using static UnityEngine.ParticleSystem;

[CreateAssetMenu(fileName = "Damage Config", menuName = "Guns/Damage Config", order = 1)]
public class DamageConfigScriptableObject : ScriptableObject
{
    public MinMaxCurve damageCurve;

    // 當在 Inspector 中第一次新增這個元件（Component）到物件上時，會自動呼叫
    private void Reset()
    {
        // mode: Constant、Curve、TwoCurves、TwoConstants
        damageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public int getDamage(float distance = 0)
    {
        return Mathf.CeilToInt(damageCurve.Evaluate(distance, Random.value));
    }
}

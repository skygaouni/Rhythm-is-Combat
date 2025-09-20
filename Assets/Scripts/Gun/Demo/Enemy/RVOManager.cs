using RVO;
using UnityEngine;

public class RVOManager : MonoBehaviour
{
    private static RVOManager _instance;

    public static RVOManager Instance => _instance;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else Destroy(gameObject);

        

    }

    void LateUpdate()
    {

        //Debug.Log($"Simulator agent count: {Simulator.Instance.getNumAgents()}");
        Simulator.Instance.doStep(); // ¥u¶]¤@¦¸
    }
}

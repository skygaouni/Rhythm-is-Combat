using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Unity.AI.Navigation.Samples 
{

    public class SurfaceNavMesh : MonoBehaviour
    {
        public NavMeshSurface navMeshSurface;

        // Start is called before the first frame update
        void Start()
        {


            //GloballyUpdatedNavMeshSurface.RequestNavMeshUpdate();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                Debug.Log("­«·s¯MµH NavMesh");
                navMeshSurface.BuildNavMesh();
            }
        }
    }
}




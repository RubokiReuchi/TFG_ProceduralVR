using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class JointBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NavMeshSurface navMesh = GetComponent<NavMeshSurface>();
        if (navMesh) navMesh.BuildNavMesh();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

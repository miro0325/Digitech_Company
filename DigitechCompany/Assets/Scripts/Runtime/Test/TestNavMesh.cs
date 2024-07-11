using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class TestNavMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.LogError(IsNavMeshBaked(GetComponent<NavMeshSurface>()));
    }

    bool IsNavMeshBaked(NavMeshSurface surface)
    {
        // NavMeshSurface가 null이 아니고 navMeshData가 null이 아니면 구워진 상태
        return surface != null && surface.navMeshData != null;
    }
}

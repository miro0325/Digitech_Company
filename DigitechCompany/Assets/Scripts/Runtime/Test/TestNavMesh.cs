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
        // NavMeshSurface�� null�� �ƴϰ� navMeshData�� null�� �ƴϸ� ������ ����
        return surface != null && surface.navMeshData != null;
    }
}

using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections.Generic;
using Photon.Pun;

public class InMap : MonoBehaviour
{
    [SerializeField] private MapMoveDoor toGround;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Door[] doors;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private Bounds[] mapBounds;
    [SerializeField, HideInInspector] private Transform wayPointParent;

    public MapMoveDoor ToGround => toGround;
    public Transform EnterPoint => enterPoint;
    public Door[] Doors => doors;
    public Bounds[] MapBounds => mapBounds;
    public Transform[] WayPoints => wayPoints;

    // [Button]
    // private  void PreBuildWayPoint()
    // {
    //     List<Transform> points = new();
    //     foreach(var mapbound in mapBounds)
    //     {
    //         var point = new GameObject("WayPoint").transform;
    //         point.transform.SetParent(wayPointParent);
    //         if(NavMesh.SamplePosition(mapbound.center, out var hit, 3, LayerMask.NameToLayer("Ground")))
    //             point.transform.position = hit.position;
    //         points.Add(point);
    //     }
    //     wayPoints = points.ToArray();
    // }

    [Button]
    private void PreBuildMap()
    {
        mapBounds = GetComponentsInChildren<MeshRenderer>()
            .Where(x => x.CompareTag("Room"))
            .Select(m => m.bounds)
            .ToArray();

        doors = GetComponentsInChildren<Door>();
    }

    public List<int> ReAllocateDoors()
    {
        List<int> viewids = new();

        var count = 0;
        foreach(var door in doors)
        {
            door.photonView.ViewID = 0;
            if(PhotonNetwork.AllocateViewID(door.photonView))
                viewids.Add(door.photonView.ViewID);
            else
            {
                viewids.Add(0);
                Debug.LogError($"Index: {count} door cannot allocate view id. return value set 0");
            }
            count++;
        }
        return viewids;
    }

    public void ReBindDoors(List<int> viewids)
    {
        var count = 0;
        foreach(var door in doors)
        {
            door.photonView.ViewID = viewids[count];
            count++;
        }
    }
}
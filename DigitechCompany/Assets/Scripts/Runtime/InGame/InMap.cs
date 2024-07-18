using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections.Generic;
using Photon.Pun;

public class InMap : MonoBehaviour, IPVReAllocatable
{
    [SerializeField] private PhotonView[] reAllocates;
    [SerializeField] private MapMoveDoor toGround;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Door[] doors;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private Transform[] wallPoints;
    [SerializeField] private Bounds[] mapBounds;
    [SerializeField, HideInInspector] private Transform wayPointParent;

    public MapMoveDoor ToGround => toGround;
    public Transform EnterPoint => enterPoint;
    public Door[] Doors => doors;
    public Bounds[] MapBounds => mapBounds;
    public Transform[] WayPoints => wayPoints;
    public Transform[] WallPoints => wallPoints;

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
        reAllocates = GetComponentsInChildren<PhotonView>();
    }

    public List<int> GetDoorViewIDs()
    {
        return doors.Select(door => door.photonView.ViewID).ToList();
    }

    public void SetActiveDoors(bool active)
    {
        foreach(var door in doors) door.gameObject.SetActive(active);
    }

    public string ReAllocatePhotonViews()
    {
        List<int> result = new();
        foreach(var pv in reAllocates)
        {
            pv.ViewID = 0;
            result.Add(PhotonNetwork.AllocateViewID(pv) ? pv.ViewID : 0);
        }
        return result.ToJson();
    }

    public void ReBindPhotonViews(string allocatedDate)
    {
        List<int> data = allocatedDate.ToList<int>();
        for(int i = 0; i < data.Count; i++)
            if(data[i] != 0) reAllocates[i].ViewID = data[i];
    }

    public string GetReAllocatedData()
    {
        List<int> result = new();
        foreach(var pv in reAllocates)
            result.Add(pv.ViewID);
        return result.ToJson();
    }
}
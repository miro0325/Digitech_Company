using UnityEngine;
using NaughtyAttributes;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine.AI;
using System.Collections.Generic;

public class InMap : MonoBehaviour
{
    [SerializeField] private MapMoveDoor toGround;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Door[] doors;
    [SerializeField] private Transform[] wayPoints;
    [SerializeField] private Bounds[] mapBounds;

    public MapMoveDoor ToGround => toGround;
    public Transform EnterPoint => enterPoint;
    public Door[] Doors => doors;
    public Bounds[] MapBounds => mapBounds;

    [Button]
    private void PreBuildMap()
    {
        mapBounds = GetComponentsInChildren<MeshRenderer>()
            .Where(x => x.CompareTag("Room"))
            .Select(m => m.bounds)
            .ToArray();

        doors = GetComponentsInChildren<Door>();
    }

    public Transform[] GetWayPoints()
    {
        return wayPoints;
    }
}
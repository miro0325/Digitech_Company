using UnityEngine;

public class OutMap : MonoBehaviour
{
    [SerializeField] private MapMoveDoor toMap;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private Transform arrivePoint;

    public MapMoveDoor ToMap => toMap;
    public Transform EnterPoint => enterPoint;
    public Transform ArrivePoint => arrivePoint;
}
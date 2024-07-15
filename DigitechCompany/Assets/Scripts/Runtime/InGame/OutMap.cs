using UnityEngine;

public class OutMap : MonoBehaviour
{
    [System.Serializable]
    public class EnvironmentSetting
    {
        public Color morning;
        public Color midnight;
        public Color night;
    }

    [SerializeField] private EnvironmentSetting environmentSetting;
    [SerializeField] private MapMoveDoor toMap;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private Transform arrivePoint;

    public MapMoveDoor ToMap => toMap;
    public Transform EnterPoint => enterPoint;
    public Transform ArrivePoint => arrivePoint;
}
using UnityEngine;

public class OutMap : MonoBehaviour
{
    [System.Serializable]
    public class EnvironmentSetting
    {
        public Material morning;
        public Material midnight;
        public Material night;
    }

    [SerializeField] private EnvironmentSetting environmentSetting;
    [SerializeField] private MapMoveDoor toMap;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private Transform arrivePoint;

    public MapMoveDoor ToMap => toMap;
    public Transform EnterPoint => enterPoint;
    public Transform ArrivePoint => arrivePoint;
    public EnvironmentSetting EnvirSetting => environmentSetting;
}
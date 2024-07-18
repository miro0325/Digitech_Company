using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class OutMap : MonoBehaviour, IPVReAllocatable
{
    [System.Serializable]
    public class EnvironmentSetting
    {
        public Material morning;
        public Material midnight;
        public Material night;
    }

    [SerializeField] private PhotonView[] reAllocates;
    [SerializeField] private EnvironmentSetting environmentSetting;
    [SerializeField] private MapMoveDoor toMap;
    [SerializeField] private Transform enterPoint;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private Transform arrivePoint;

    public MapMoveDoor ToMap => toMap;
    public Transform EnterPoint => enterPoint;
    public Transform ArrivePoint => arrivePoint;
    public Transform DeliveryPoint => deliveryPoint;
    public EnvironmentSetting EnvirSetting => environmentSetting;

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
using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class NetworkObjectManager : MonoBehaviourPun
{
    internal readonly Dictionary<string, NetworkObject> networkObjects = new();

    internal NetworkObject InstantiateNetworkObjectInternal(string path, Vector3 pos, Quaternion rot)
    {
        var obj = Instantiate(Resources.Load<NetworkObject>(path), pos, rot);
        var pv = obj.GetComponent<PhotonView>();

        if (PhotonNetwork.AllocateViewID(pv)) //allocate view id
        {
            var guid = Guid.NewGuid().ToString();
            networkObjects.Add(guid, obj);
            obj.guid = guid;

            obj.OnCreate();

            //send rpc to another
            photonView.RPC(nameof(InstantiateNetworkObjectRPC), RpcTarget.OthersBuffered, path, guid, pv.ViewID, pos, rot);
            return obj;
        }
        else
        {
            Debug.LogWarning("Failed to allocate a ViewId.");
            Destroy(obj);
            return null;
        }
    }

    [PunRPC]
    private void InstantiateNetworkObjectRPC(string path, string guid, int viewId, Vector3 pos, Quaternion rot)
    {
        var obj = Instantiate(Resources.Load<NetworkObject>(path), pos, rot);
        var pv = obj.GetComponent<PhotonView>();
        pv.ViewID = viewId;

        networkObjects.Add(guid, obj);
        obj.guid = guid;

        obj.OnCreate();
    }

    internal void DestoryNetworkObjectInternal(string guid)
    {
        photonView.RPC(nameof(DestoryNetworkObjectRpc), RpcTarget.AllBuffered, guid);
    }

    [PunRPC]
    private void DestoryNetworkObjectRpc(string guid)
    {
        if (photonView.IsMine)
        {
            if (!networkObjects.ContainsKey(guid)) return;
            
            var obj = networkObjects[guid];
            if (obj) PhotonNetwork.Destroy(obj.photonView);
        }

        if (networkObjects.ContainsKey(guid))
            networkObjects.Remove(guid);
    }
}
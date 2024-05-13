using System;
using System.Collections.Generic;
using Photon.Pun;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkObject : MonoBehaviourPun
{
    private static NetworkObjectManager manager;
    private static NetworkObjectManager Manager
        => manager ??= FindObjectOfType<NetworkObjectManager>();

    public static NetworkObject GetNetworkObject(string guid)
        => Manager.networkObjects[guid];

    public static NetworkObject Instantiate(string path, Vector3 pos = default, Quaternion rot = default)
        => Manager.InstantiateNetworkObjectInternal(path, pos, rot);

    public static void Destory(string guid)
        => Manager.DestoryNetworkObjectInternal(guid);

    public string guid;
    public virtual void OnCreate() { }
}
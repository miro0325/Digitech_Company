using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;

public class NetworkObject : MonoBehaviourPun
{
    private static NetworkObjectManager _networkObjectManager;
    private static NetworkObjectManager networkObjectManager => _networkObjectManager ??= ServiceLocator.ForGlobal().Get<NetworkObjectManager>();

    /// <summary>
    /// Instantiate prefab from resources folder<br/>
    /// Send RPC everyone (not buffered) <br/>
    /// You can use this method when you need to spawn object and sync manually with SyncInstantiate
    /// </summary>
    /// <param name="prefab">resource fild path</param>
    /// <param name="pos">position</param>
    /// <param name="quat">rotation</param>
    /// <returns></returns>
    public static NetworkObject Instantiate(string prefab, Vector3 pos = default, Quaternion quat = default)
        => networkObjectManager.InstantiateNetworkObjectInternal(prefab, pos, quat, false);

    /// <summary>
    /// Instantiate prefab from resources folder<br/>
    /// Send RPC everyone <br/>
    /// This is same as PhotonNetwork.Instantiate()
    /// </summary>
    /// <param name="prefab">resource fild path</param>
    /// <param name="pos">position</param>
    /// <param name="quat">rotation</param>
    /// <returns></returns>
    public static NetworkObject InstantiateBuffered(string prefab, Vector3 pos = default, Quaternion quat = default)
        => networkObjectManager.InstantiateNetworkObjectInternal(prefab, pos, quat, true);

    /// <summary>
    /// Destory object with view id
    /// </summary>
    /// <param name="viewId">view id to destory</param>
    public static void Destory(int viewId)
        => networkObjectManager.DestroyNetworkObjectInternal(viewId);

    /// <summary>
    /// Instantiate prefab from resources folder and sync using view id<br/>
    /// If photon view is not exist, Instantiate new prefab at prefab path and sync view id
    /// You can use this method with NetworkObject.Instantiate()
    /// </summary>
    /// <param name="viewId">view id to sync</param>
    /// <param name="prefab">resource fild path</param>
    /// <returns></returns>
    public static NetworkObject Sync(string prefab, int viewId, Vector3 pos = default, Quaternion quat = default)
        => networkObjectManager.SyncNetworkObjectInternal(viewId, prefab, pos, quat);

    public virtual void OnCreate() { }
}
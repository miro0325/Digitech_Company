using Photon.Pun;
using UnityEngine;

public class NetworkObject : MonoBehaviourPun
{
    private static NetworkObjectManager networkObjectManager;
    private static NetworkObjectManager NetworkObjectManager
    {
        get
        {
            if(!networkObjectManager)
                networkObjectManager = ServiceLocator.ForGlobal().Get<NetworkObjectManager>();
            return networkObjectManager;
        }
    }

    /// <summary>
    /// Instantiate prefab from resources folder<br/>
    /// Send RPC to everyone (not buffered) <br/>
    /// You can use this when you need to cache object with SyncInstantiate
    /// </summary>
    /// <param name="prefab">resource fild path</param>
    /// <param name="pos">position</param>
    /// <param name="quat">rotation</param>
    /// <returns></returns>
    public static NetworkObject Instantiate(string prefab, Vector3 pos = default, Quaternion quat = default)
        => NetworkObjectManager.InstantiateNetworkObjectInternal(prefab, pos, quat, false);
    
    /// <summary>
    /// Instantiate prefab from resources folder<br/>
    /// Send RPC to everyone <br/>
    /// This is same as PhotonNetwork.Instantiate()
    /// </summary>
    /// <param name="prefab">resource fild path</param>
    /// <param name="pos">position</param>
    /// <param name="quat">rotation</param>
    /// <returns></returns>
    public static NetworkObject InstantiateBuffered(string prefab, Vector3 pos = default, Quaternion quat = default)
        => NetworkObjectManager.InstantiateNetworkObjectInternal(prefab, pos, quat, true);

    /// <summary>
    /// Destory object with view id
    /// </summary>
    /// <param name="viewId">view id to destory</param>
    public static void Destory(int viewId)
        => NetworkObjectManager.DestroyNetworkObjectInternal(viewId);

    /// <summary>
    /// Instantiate prefab from resources folder and sync using view id<br/>
    /// You can use this with NetworkObject.Instantiate()
    /// </summary>
    /// <param name="prefab">resource fild path</param>
    /// <param name="viewId">view id to sync</param>
    /// <returns></returns>
    public static NetworkObject SyncInstantiate(string prefab, int viewId)
        => NetworkObjectManager.SyncNetworkObjectInternal(prefab, viewId);

    public virtual void OnCreate() { }
}
using Photon.Pun;
using UnityEngine;

public class NetworkObjectManager : MonoBehaviourPun, IService
{
    private void Awake()
    {
        ServiceLocator.ForGlobal().Register(this);
    }

    internal NetworkObject InstantiateNetworkObjectInternal(string prefab, Vector3 pos, Quaternion quat, bool isBufferd)
    {
        var @object = Instantiate(Resources.Load<NetworkObject>(prefab));
        var pv = @object.GetComponent<PhotonView>();

        if (PhotonNetwork.AllocateViewID(pv))
        {
            @object.transform.position = pos;
            @object.transform.rotation = quat;

            PhotonNetwork.RegisterPhotonView(photonView);
            
            @object.OnCreate();

            photonView.RPC(nameof(InstantiateNetworkObjectRpc), isBufferd ? RpcTarget.OthersBuffered : RpcTarget.Others, prefab, pv.ViewID, pos, quat);

            return @object;
        }
        else
        {
            Debug.LogError("Failed to allocate a ViewId.");

            Destroy(@object);
            return null;
        }
    }

    [PunRPC]
    private void InstantiateNetworkObjectRpc(string prefab, int viewId, Vector3 pos, Quaternion quat)
    {
        var @object = Instantiate(Resources.Load<NetworkObject>(prefab));
        var pv = @object.GetComponent<PhotonView>();
        
        pv.ViewID = viewId;
        @object.transform.position = pos;
        @object.transform.rotation = quat;

        PhotonNetwork.RegisterPhotonView(photonView);

        @object.OnCreate();
    }

    internal void DestroyNetworkObjectInternal(int viewId)
    {
        photonView.RPC(nameof(DestoryNetworkObjectRpc), RpcTarget.All, viewId);
    }

    [PunRPC]
    private void DestoryNetworkObjectRpc(int viewId)
    {
        var pv = PhotonView.Find(viewId);
        if(pv && pv.IsMine) PhotonNetwork.Destroy(pv);
    }

    internal NetworkObject SyncNetworkObjectInternal(string prefab, int viewId)
    {
        var @object = Instantiate(Resources.Load<NetworkObject>(prefab));
        var pv = @object.GetComponent<PhotonView>();
        pv.ViewID = viewId;
        
        PhotonNetwork.RegisterPhotonView(photonView);

        @object.OnCreate();
        return @object;
    }
}
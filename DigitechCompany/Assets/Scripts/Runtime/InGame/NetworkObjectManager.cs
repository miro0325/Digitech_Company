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
        var @object = Instantiate(Resources.Load<NetworkObject>(prefab), pos, quat);
        var pv = @object.GetComponent<PhotonView>();

        if (PhotonNetwork.AllocateViewID(pv))
        {
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
        var @object = Instantiate(Resources.Load<NetworkObject>(prefab), pos, quat);
        var pv = @object.GetComponent<PhotonView>();

        pv.ViewID = viewId;

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
        if (pv && pv.IsMine) PhotonNetwork.Destroy(pv);
    }

    internal NetworkObject SyncNetworkObjectInternal(int viewId, string prefab, Vector3 pos, Quaternion quat)
    {
        var pv = PhotonView.Find(viewId);
        if (pv != null) //already object exist
        {
            return pv.GetComponent<NetworkObject>();
        }
        else //instantiate new one
        {
            var @object = Instantiate(Resources.Load<NetworkObject>(prefab), pos, quat);
            var getPv = @object.GetComponent<PhotonView>();
            getPv.ViewID = viewId;

            PhotonNetwork.RegisterPhotonView(photonView);

            @object.OnCreate();
            return @object;
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class Container : MonoBehaviourPun
{
    private ResourceLoader _resourceLoader;
    private ResourceLoader resourceLoader => _resourceLoader ??= ServiceLocator.ForGlobal().Get<ResourceLoader>();

    [SerializeField] private MeshRenderer spawnArea;
    [SerializeField] private Transform holder;

    public void SpawnItems(List<string> items)
    {
        foreach (var item in items)
        {
            var spawnPos =
                new Vector3(
                    Random.Range(-holder.localScale.x / 2, holder.localScale.x / 2),
                    Random.Range(-holder.localScale.y / 2, holder.localScale.y / 2),
                    Random.Range(-holder.localScale.z / 2, holder.localScale.z / 2)
                );
            var pv = NetworkObject.Instantiate($"Prefabs/Items/{item}").photonView;
            photonView.RPC(nameof(SetPositionRpc), RpcTarget.All, pv.ViewID, spawnPos);
        }
    }

    [PunRPC]
    private void SetPositionRpc(int pvid, Vector3 spawnPos)
    {
        Debug.Log(spawnPos);
        var item = PhotonView.Find(pvid);
        item.transform.position = holder.transform.position + spawnPos;
    }

    public void DestoryRemainItems()
    {
        for(int i = holder.childCount - 1; i >= 0; i--)
            Destroy(holder.GetChild(i).gameObject);
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (other.transform.parent == null)
        {
            other.transform.SetParent(holder);
            if(other.TryGetComponent<InGamePlayer>(out var player))
                player.SetParent(photonView.ViewID);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (ReferenceEquals(other.transform.parent, holder))
        {
            other.transform.SetParent(null);
            if(other.TryGetComponent<InGamePlayer>(out var player))
                player.SetParent(0);
        }
    }
}

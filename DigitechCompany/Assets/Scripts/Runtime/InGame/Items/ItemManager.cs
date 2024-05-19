using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class NetworkItemData
{
    public int viewId;
    public string key;
    public Vector3 position;
    public float layRotation;
}

public class ItemManager : MonoBehaviourPun, IService//, IPunObservable
{
    private ResourceLoader resourceLoader;

    private HashSet<ItemBase> items = new();

    public HashSet<ItemBase> Items => items;

    public void SpawnItem(int difficulty, Bounds[] spawnAreas)
    {
        int wholeItemAmount = 35 * difficulty;
        int averageItemAmount = Mathf.Max(wholeItemAmount / spawnAreas.Length, 2);

        foreach (var area in spawnAreas)
        {
            int spawnItemAmount = Random.Range(0, averageItemAmount * 2);

            for (int i = 0; i < spawnItemAmount; i++)
            {
                var randomPos =
                    new Vector3
                    (
                        Random.Range(area.min.x, area.max.x),
                        Random.Range(-2, 2), //need to fix
                        Random.Range(area.min.z, area.max.z)
                    );

                if (NavMesh.SamplePosition(randomPos, out var hit, 3, ~0)) //~0 is all layer 
                {
                    var itemKeys = resourceLoader.itemPrefabs.Keys.ToArray();
                    var randomItemKey = itemKeys[Random.Range(0, itemKeys.Length)];
                    var item = NetworkObject.Instantiate($"Prefabs/Items/{randomItemKey}", hit.position + Vector3.up, Quaternion.identity).GetComponent<ItemBase>();
                    item.SetLayRotation(Random.Range(0, 360));
                    item.Initialize(randomItemKey);
                    items.Add(item);
                    // itemNetworkDatas.Add(item.photonView.ViewID, item.Key);
                }
            }
        }
        var item2 = NetworkObject.Instantiate($"Prefabs/Items/Key", Vector3.up, Quaternion.identity).GetComponent<ItemBase>();
        item2.SetLayRotation(Random.Range(0, 360));
        item2.Initialize("Key");
        items.Add(item2);
    }

    public void SyncItem()
    {
        photonView.RPC(nameof(RequestSyncItemData), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        // foreach(var itemdata in itemNetworkDatas)
        // {
        //     var item = NetworkObject.SyncInstantiate($"Prefabs/Items/{itemdata.Value}", itemdata.Key) as ItemBase;
        //     item.Initialize(itemdata.Value);
        //     items.Add(item);
        // }
    }

    [PunRPC]
    private void RequestSyncItemData(Photon.Realtime.Player player)
    {
        var array = new NetworkItemData[items.Count];
        var count = 0;
        foreach(var item in items)
            array[count++] = new NetworkItemData()
            {
                viewId = item.photonView.ViewID,
                key = item.Key,
                position = item.transform.position,
                layRotation = item.LayRotation
            };

        photonView.RPC(nameof(OnReceiveSyncItemData), player, array.ToJson());
    }

    [PunRPC]
    private void OnReceiveSyncItemData(string networkItemDataJson)
    {
        var datas = JsonSerializer.JsonToArray<NetworkItemData>(networkItemDataJson);
        for (int i = 0; i < datas.Length; i++)
        {
            var data = datas[i];
            var item = NetworkObject.SyncInstantiate($"Prefabs/Items/{data.key}", data.viewId) as ItemBase;
            item.Initialize(data.key);
            item.transform.position = data.position;
            item.SetLayRotation(data.layRotation);
            items.Add(item);
        }
    }

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
        resourceLoader = ServiceLocator.GetEveryWhere<ResourceLoader>();
    }

    // public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    // {
    //     if(stream.IsWriting)
    //     {
    //         stream.SendNext(itemNetworkDatas.Count);
    //         foreach(var itemInfo in itemNetworkDatas)
    //         {
    //             stream.SendNext(itemInfo.Key);
    //             stream.SendNext(itemInfo.Value);
    //         }
    //     }
    //     else
    //     {
    //         itemNetworkDatas.Clear();
    //         var iteration = (int)stream.ReceiveNext();
    //         for(int i = 0; i < iteration; i++)
    //         {
    //             var key = (int)stream.ReceiveNext();
    //             var value = (string)stream.ReceiveNext();
    //             itemNetworkDatas.Add(key, value);
    //         }
    //     }
    // }
}

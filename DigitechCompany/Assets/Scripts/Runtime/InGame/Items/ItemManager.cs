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
    //service
    private ResourceLoader _resourceLoader;
    private ResourceLoader resourceLoader => _resourceLoader ??= ServiceLocator.ForGlobal().Get<ResourceLoader>();
    private DataContainer _dataContainer;
    private DataContainer dataContainer => _dataContainer ??= ServiceLocator.ForGlobal().Get<DataContainer>();
    private Basement _basement;
    private Basement basement => _basement ??= ServiceLocator.For(this).Get<Basement>();

    //field
    private string itemDataJson;
    private Dictionary<int, ItemBase> items = new();

    //property
    public string ItemDataJson => itemDataJson;
    public Dictionary<int, ItemBase> Items => items;

    //function
    /// <summary>
    /// This function must be executed only when you are a host.
    /// </summary>
    /// <param name="difficulty"></param>
    /// <param name="spawnAreas"></param>
    /// <returns>Items data</returns>
    public void SpawnItem(int difficulty, Bounds[] spawnAreas)
    {
        int maxItemCountInMap = 35 * difficulty;
        int maxItemCountPerRoom = 6;
        int expectRandomRoomCount = maxItemCountInMap / maxItemCountPerRoom;

        int targetRandomRoomCount = (int)Random.Range(expectRandomRoomCount * 1.25f, expectRandomRoomCount * 1.25f);

        List<Bounds> bounds = spawnAreas.ToList();
        for(int i = 0; i < targetRandomRoomCount; i++)
        {
            int targetRoomIndex = Random.Range(0, bounds.Count);
            var randomRoom = bounds[targetRoomIndex];
            bounds.RemoveAt(targetRoomIndex);
            
            int targetItemCount = Random.Range(3, maxItemCountPerRoom + 1);
            for (int j = 0; j < targetItemCount; j++)
            {
                var randomPos =
                    new Vector3
                    (
                        Random.Range(randomRoom.min.x, randomRoom.max.x),
                        Random.Range(randomRoom.center.y, randomRoom.center.y),
                        Random.Range(randomRoom.min.z, randomRoom.max.z)
                    ) * 0.8f;

                if (NavMesh.SamplePosition(new Vector3(0, -50, 0) + randomPos, out var hit, 3, LayerMask.NameToLayer("Ground"))) //~0 is all layer 
                {
                    var itemKeys = dataContainer.loadData.itemDatas
                        .Where(item => item.Value.isAvailable && item.Value.type == ItemType.Sell)
                        .Select(item => item.Key)
                        .ToArray();
                    var randomItemKey = itemKeys[Random.Range(0, itemKeys.Length)];
                    SpawnItem(hit.position, randomItemKey);
                }
            }
        }

        BuildItemDataJson();
    }

    public void SpawnItem(Vector3 spawnPos,string key)
    {
        if (!resourceLoader.itemPrefabs.ContainsKey(key)) return;
        var item = NetworkObject.Instantiate($"Prefabs/Items/{key}", spawnPos + Vector3.up, Quaternion.identity) as ItemBase;
        item.SetLayRotation(Random.Range(0, 360));
        item.Initialize(key);
        items.Add(item.photonView.ViewID, item);
    }

    public void BuildItemDataJson()
    {
        var networkItemDatas = new NetworkItemData[items.Count];
        var count = 0;
        foreach(var kvp in items)
            networkItemDatas[count++] = new()
            {
                viewId = kvp.Key,
                key = kvp.Value.Key,
                position = kvp.Value.transform.position,
                layRotation = kvp.Value.LayRotation
            };

        itemDataJson = networkItemDatas.ToJson();
    }

    /// <summary>
    /// This function must be executed only when you are a client.
    /// </summary>
    /// <param name="difficulty"></param>
    /// <param name="spawnAreas"></param>
    /// <returns>Items data</returns>
    public void SyncItem(string networkItemDataJson)
    {
        if(string.IsNullOrEmpty(networkItemDataJson)) return;

        var datas = JsonSerializer.JsonToArray<NetworkItemData>(networkItemDataJson);
        
        for (int i = 0; i < datas.Length; i++)
        {
            var data = datas[i];
            var item = NetworkObject.Sync($"Prefabs/Items/{data.key}", data.viewId) as ItemBase;
            item.Initialize(data.key);
            item.transform.position = data.position;
            item.SetLayRotation(data.layRotation);
            items.Add(item.photonView.ViewID, item);
        }
    }

    /// <summary>
    /// This function must be executed only when you are a client.
    /// </summary>
    /// <param name="withoutBasement">Whether to clear the items in the foundation</param>
    public void DestoryItems(bool withoutBasement)
    {
        Dictionary<int ,ItemBase> excepts = new();
        if(withoutBasement) excepts = basement.WholeItems;
        
        foreach(var kvp in items.ToArray())
        {
            if(!excepts.ContainsKey(kvp.Key))
                NetworkObject.Destory(kvp.Value.photonView.ViewID);
        }

        items.Clear();
        if(withoutBasement)
            items = excepts;
    }

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }
}

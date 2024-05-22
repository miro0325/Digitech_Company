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
    private ResourceLoader resourceLoader;

    //field
    private HashSet<ItemBase> items = new();

    //property
    public HashSet<ItemBase> Items => items;

    //function
    /// <summary>
    /// This function must be executed only when you are a host.
    /// </summary>
    /// <param name="difficulty"></param>
    /// <param name="spawnAreas"></param>
    /// <returns>Items data</returns>
    public string SpawnItem(int difficulty, Bounds[] spawnAreas)
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
                        Random.Range(area.center.y - 1, area.center.y + 2),
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
                }
            }
        }

        var item2 = NetworkObject.Instantiate($"Prefabs/Items/Key", Vector3.up, Quaternion.identity).GetComponent<ItemBase>();
        item2.SetLayRotation(Random.Range(0, 360));
        item2.Initialize("Key");
        items.Add(item2);

        var networkItemData = new NetworkItemData[items.Count];
        var count = 0;
        foreach(var item in items)
            networkItemData[count++] = new()
            {
                viewId = item.photonView.ViewID,
                key = item.Key,
                position = item.transform.position,
                layRotation = item.LayRotation
            };

        return networkItemData.ToJson();
    }

    /// <summary>
    /// This function must be executed only when you are a client.
    /// </summary>
    /// <param name="difficulty"></param>
    /// <param name="spawnAreas"></param>
    /// <returns>Items data</returns>
    public void SyncItem(string networkItemDataJson)
    {
        var datas = JsonSerializer.JsonToArray<NetworkItemData>(networkItemDataJson);
        for (int i = 0; i < datas.Length; i++)
        {
            var data = datas[i];
            var item = NetworkObject.Sync(data.viewId, $"Prefabs/Items/{data.key}") as ItemBase;
            item.Initialize(data.key);
            item.transform.position = data.position;
            item.SetLayRotation(data.layRotation);
            items.Add(item);
        }
    }

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    private void Start()
    {
        resourceLoader = ServiceLocator.GetEveryWhere<ResourceLoader>();
    }
}

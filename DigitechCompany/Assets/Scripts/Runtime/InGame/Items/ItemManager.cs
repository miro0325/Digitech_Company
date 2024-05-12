using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class ItemManager : MonoBehaviour
{
    private ResourceLoader resourceLoader;

    private List<ItemBase> items = new();

    public IReadOnlyList<ItemBase> Items => items;

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
                        Random.Range(area.min.y, area.max.y), 
                        Random.Range(area.min.z, area.max.z)
                    );

                if(NavMesh.SamplePosition(randomPos, out var hit, 3, ~0)) //~0 is all layer 
                {
                    var itemKeys = resourceLoader.itemPrefabs.Keys.ToArray();
                    var randomItemKey = itemKeys[Random.Range(0, itemKeys.Length)];
                    var item = NetworkObject.Instantiate($"Prefabs/Items/{randomItemKey}").GetComponent<ItemBase>();
                    item.transform.position = hit.position + Vector3.up;
                    item.LayRotation = Random.Range(0, 360);
                    item.Initialize(randomItemKey);
                    items.Add(item);
                }
            }
        }
    }

    private void Awake()
    {
        Services.Register(this);
        resourceLoader = Services.Get<ResourceLoader>();
    }
}

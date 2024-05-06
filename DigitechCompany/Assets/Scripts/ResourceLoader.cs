using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader : MonoBehaviour
{
    public Dictionary<int, ItemBase> Items => items;
    
    private readonly static string ITEM_PATH = "Prefabs/Items";
    
    private Dictionary<int,ItemBase> items = new Dictionary<int,ItemBase>();

    private void Awake()
    {
        Services.Register(this, true);
        ResourceLoad();
    }
    
    private void ResourceLoad()
    {
        var itemPrefabs = Resources.LoadAll<ItemBase>(ITEM_PATH);
        for(int i = 0; i < itemPrefabs.Length; i++)
        {
            items.Add(i+1, itemPrefabs[i]);
        }
    }
}

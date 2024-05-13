using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader : MonoBehaviour
{
    public Dictionary<string,ItemBase> itemPrefabs = new();

    private void Awake()
    {
        Services.Register(this, true);
        ResourceLoad();
    }

    private void ResourceLoad()
    {
        foreach(var item in Resources.LoadAll<ItemBase>("Prefabs/Items"))
            itemPrefabs.Add(item.name, item);
    }
}

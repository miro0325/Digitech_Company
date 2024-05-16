using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader : MonoBehaviour, IService
{
    public Dictionary<string,ItemBase> itemPrefabs = new();

    private void Awake()
    {
        ServiceLocator.ForGlobal().Register(this);
        ResourceLoad();
    }

    private void ResourceLoad()
    {
        foreach(var item in Resources.LoadAll<ItemBase>("Prefabs/Items"))
            itemPrefabs.Add(item.name, item);
    }
}

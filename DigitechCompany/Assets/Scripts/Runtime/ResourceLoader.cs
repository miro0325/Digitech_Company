using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceLoader : MonoBehaviour, IService
{
    public Dictionary<string, ItemBase> itemPrefabs = new();
    public Dictionary<string, Sprite> itemIcons = new();

    private void Awake()
    {
        ServiceLocator.ForGlobal().Register(this);
        ResourceLoad();
    }

    private void ResourceLoad()
    {
        foreach (var item in Resources.LoadAll<ItemBase>("Prefabs/Items"))
            itemPrefabs.Add(item.name, item);

        foreach (var icon in Resources.LoadAll<Sprite>("Sprites/ItemIcons"))
            itemIcons.Add(icon.name, icon);
    }
}

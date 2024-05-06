using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Dictionary<int, ItemBase> OriginItems => originItems;
    
    private DataContainer dataContainer;
    private ResourceLoader loader;

    [SerializeField]
    private Dictionary<int,ItemBase> originItems = new();
    private void Awake()
    {
        Services.Register(this, true);
    }

    private void InitItems()
    {
        foreach (var itemData in dataContainer.itemData)
        {
            Debug.Log(loader.Items.ContainsKey(itemData.id));
            if (!loader.Items.ContainsKey(itemData.id) || loader.Items[itemData.id] == null) continue;
            var item = Instantiate(loader.Items[itemData.id],transform);
            item.transform.localPosition = Vector3.zero;
            // item.Init(itemData);
            originItems.Add(itemData.id,item);
        }
    }

    private void Start()
    {
        dataContainer = Services.Get<DataContainer>();
        loader = Services.Get<ResourceLoader>();
        Debug.Log(loader.gameObject);
        InitItems();
    }

    private void Update()
    {
        
    }
}

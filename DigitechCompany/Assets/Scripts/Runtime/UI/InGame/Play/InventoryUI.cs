using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();
    private ResourceLoader _resourceLoader ;
    private ResourceLoader resourceLoader => _resourceLoader ??= ServiceLocator.ForGlobal().Get<ResourceLoader>();

    [SerializeField] private GameObject slotPrefab;

    private List<RectTransform> inventoryBoxes = new();
    private List<Image> itemIconImages = new();

    private void Start()
    {
        for(int i = 0; i < player.Inventory.Size; i++)
        {
            var slot = Instantiate(slotPrefab, transform);
            inventoryBoxes.Add(slot.transform as RectTransform);
            itemIconImages.Add(slot.transform.GetChild(0).GetComponent<Image>());
        }
    }

    private void Update()
    {
        for(int i = 0; i < player.Inventory.Size; i++)
        {
            inventoryBoxes[i].localScale =
                Vector3.Lerp(inventoryBoxes[i].localScale, player.Inventory.Index == i ? Vector3.one * 1.1f : Vector3.one, Time.deltaTime * 5f);

            if(player.Inventory[i] == null)
            {
                itemIconImages[i].color = default;
            }
            else
            {
                itemIconImages[i].color = Color.white;
                itemIconImages[i].sprite = resourceLoader.itemIcons[player.Inventory[i].Key];
            }
        }
    }
}

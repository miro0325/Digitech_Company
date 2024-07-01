using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemResultSlotUI : MonoBehaviour
{
    private ResourceLoader _resourceLoader;
    private ResourceLoader resourceLoader => _resourceLoader ??= ServiceLocator.ForGlobal().Get<ResourceLoader>();

    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI data;

    public void Initialize(ItemBase item)
    {
        icon.sprite = resourceLoader.itemIcons[item.Key];
        itemName.text = item.ItemData.name;
        data.text = $"{item.SellPrice:#,###.#}$, {item.ItemData.weight}kg";

        var graphics = new Graphic[] { icon, itemName, data };
        foreach (var graphic in graphics)
        {
            graphic.color = new Color(1, 1, 1, 0);
            
            var targetPos = graphic.rectTransform.anchoredPosition;
            graphic.rectTransform.anchoredPosition += Vector2.up * 20f;
            graphic.rectTransform.DOAnchorPos(targetPos, 0.5f).SetEase(Ease.OutQuad);
            graphic.DOColor(Color.white, 0.5f);
        }
    }
}
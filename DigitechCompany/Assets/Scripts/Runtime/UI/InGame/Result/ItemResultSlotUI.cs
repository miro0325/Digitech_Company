using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemResultSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI price;
    [SerializeField] private TextMeshProUGUI weight;

    public void Initialize(ItemBase item)
    {
        
    }
}
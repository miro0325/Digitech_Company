using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScanUI : MonoBehaviour
{
    [SerializeField] private Image itemNameBackground;
    [SerializeField] private Image itemInfoBackground;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI itemWeightText;

    private RectTransform rectTransform;

    public void StartDisplay(ItemBase item)
    {
        StartCoroutine(DisplayRoutine(item));
        IEnumerator DisplayRoutine(ItemBase item)
        {
            yield return null;
        }   
    }

    private void Start()
    {
        rectTransform = transform as RectTransform;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class ScanInfomationUI : MonoBehaviour
{
    [SerializeField] private RectTransform itemNameBackground;
    [SerializeField] private RectTransform itemInfoBackground;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI itemWeightText;

    private Camera mainCamera;
    private ItemBase targetItem;
    private Image image;
    private RectTransform rectTransform;
    private Coroutine displayRoutine;
    private Action<ScanInfomationUI> poolAction;

    public void Initialize(Action<ScanInfomationUI> poolAction)
    {
        this.poolAction = poolAction;
    }

    public void StartDisplay(ItemBase item)
    {
        targetItem = item;
        gameObject.SetActive(true);
        displayRoutine = StartCoroutine(DisplayRoutine(item));

        IEnumerator DisplayRoutine(ItemBase item)
        {
            //set text
            itemNameText.text = item.ItemData.name;
            itemPriceText.text = $"{item.SellPrice:#,##0}$";
            itemWeightText.text = $"{item.ItemData.weight:#,##0.0}kg";

            //variables
            var circleTargetColor = image.color;
            var circleStartColor = circleTargetColor;
            circleStartColor.a = 0;
            var circleTargetSize = rectTransform.sizeDelta;
            var circleStartSize = circleTargetSize * 3;
            var itemNameBackgroundTargetSize = itemNameBackground.sizeDelta;
            var itemNameBackgroundStartSize = new Vector2(0, itemNameBackgroundTargetSize.y);
            var itemInfoBackgroundTargetSize = itemInfoBackground.sizeDelta;
            var itemInfoBackgroundStartSize = new Vector2(0, itemInfoBackgroundTargetSize.y);

            //initialize
            image.color = circleStartColor;
            rectTransform.sizeDelta = circleStartSize;
            itemNameBackground.sizeDelta = itemNameBackgroundStartSize;
            itemInfoBackground.sizeDelta = itemInfoBackgroundStartSize;

            //animation
            image.DOColor(circleTargetColor, 0.25f).SetEase(Ease.InQuart);
            rectTransform.DOSizeDelta(circleTargetSize, 0.25f).SetEase(Ease.InQuart);
            yield return new WaitForSeconds(0.25f);

            itemNameBackground.DOSizeDelta(itemNameBackgroundTargetSize, 0.25f).SetEase(Ease.OutQuart);
            itemInfoBackground.DOSizeDelta(itemInfoBackgroundTargetSize, 0.25f).SetEase(Ease.OutQuart);
            yield return new WaitForSeconds(1f);

            targetItem = null;
            gameObject.SetActive(false);
            poolAction?.Invoke(this);
        }
    }

    private void Update()
    {
        if (targetItem.InHand)
        {
            StopCoroutine(displayRoutine);
            transform.DOKill();

            targetItem = null;
            gameObject.SetActive(false);
            poolAction?.Invoke(this);
        }

        if (targetItem)
        {
            rectTransform.anchoredPosition =
                mainCamera.WorldToScreenPoint(targetItem.transform.position);
        }
    }

    private void Awake()
    {
        mainCamera = Camera.main;
        image = GetComponent<Image>();
        rectTransform = transform as RectTransform;
    }
}

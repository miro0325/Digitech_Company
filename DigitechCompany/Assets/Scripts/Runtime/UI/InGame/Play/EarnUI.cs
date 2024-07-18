using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;

public class EarnUI : MonoBehaviour
{
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();

    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform itemListPanel;
    [SerializeField] private ItemResultSlotUI itemSlotPrefab;
    [SerializeField] private TextMeshProUGUI wholeEarn;

    private List<ItemResultSlotUI> slots = new();

    private void Start()
    {
        gameManager
            .ObserveEveryValueChanged(gm => gm.EarnedData)
            .Skip(0)
            .Subscribe(earnData => OpenTask(earnData).Forget());
        panel.gameObject.SetActive(false);
    }

    private async UniTask OpenTask(GameManager.EarnData earnData)
    {
        foreach (var slot in slots) Destroy(slot.gameObject);
        slots.Clear();

        foreach (var item in earnData.items)
        {
            var newSlot = Instantiate(itemSlotPrefab, itemListPanel);
            newSlot.Initialize(item);
            slots.Add(newSlot);
        }

        wholeEarn.text = $"{earnData.wholeEarn:#,###.#}$";

        panel.gameObject.SetActive(true);
        transform.localScale = new Vector3(1, 0.01f, 1);
        transform.DOScaleY(1, 0.25f).SetEase(Ease.OutBack);

        await UniTask.NextFrame();

        itemListPanel.anchoredPosition = Vector2.zero;
        if(itemListPanel.sizeDelta.y > 386)
            itemListPanel.DOAnchorPosY(itemListPanel.sizeDelta.y - 386, 1.5f).SetEase(Ease.OutQuad);
        
        await UniTask.WaitForSeconds(4f);
        panel.gameObject.SetActive(false);
    }
}
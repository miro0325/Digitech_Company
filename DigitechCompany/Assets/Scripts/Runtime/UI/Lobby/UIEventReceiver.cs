using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEventReceiver : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    private Action pointerEnterAction;
    private Action pointerClickAction;

    public void Initialize(Action pointerEnterAction, Action pointerClickAction)
    {
        this.pointerEnterAction = pointerEnterAction;
        this.pointerClickAction = pointerClickAction;
    }

    public void OnPointerClick(PointerEventData eventData) => pointerClickAction?.Invoke();
    public void OnPointerEnter(PointerEventData eventData) => pointerEnterAction?.Invoke();
}
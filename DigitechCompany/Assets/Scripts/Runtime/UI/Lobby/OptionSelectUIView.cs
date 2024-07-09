using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class OptionSelectUIView : UIView
{
    private PopupUI _popupUI;
    private PopupUI popupUI => _popupUI ??= ServiceLocator.ForGlobal().Get<PopupUI>();

    [SerializeField] protected UIEventReceiver[] options;
    [SerializeField] protected RectTransform arrow;

    protected ReactiveProperty<int> index = new();

    protected abstract Action GetOptionAction(int index);

    protected virtual void Start()
    {
        for(int i = 0; i < options.Length; i++)
        {
            var index = i;
            options[i].Initialize(
                () => this.index.Value = index,
                GetOptionAction(index)
            );
        }
        
        index.Skip(1).Subscribe(x => arrow.position = options[x].transform.position + Vector3.left * 20);
    }

    protected virtual void Update()
    {
        if(popupUI.IsOpen) return;

        if(Keyboard.current.wKey.wasPressedThisFrame) AddIndex(true);
        if(Keyboard.current.sKey.wasPressedThisFrame) AddIndex(false);
        if(Keyboard.current.upArrowKey.wasPressedThisFrame) AddIndex(true);
        if(Keyboard.current.downArrowKey.wasPressedThisFrame) AddIndex(false);
        if(Keyboard.current.enterKey.wasPressedThisFrame) GetOptionAction(index.Value)?.Invoke(); 
        if(Keyboard.current.spaceKey.wasPressedThisFrame) GetOptionAction(index.Value)?.Invoke(); 
    }

    protected void AddIndex(bool positive)
    {
        var value = Mathf.Clamp(index.Value + (positive ? -1 : 1), 0, options.Length - 1);
        Debug.Log(value);
        index.Value = value;
    }

    public override void Open()
    {
        gameObject.SetActive(true);
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class PopupUI : MonoBehaviour, IService
{
    public class ButtonData
    {
        public readonly string display;
        public readonly Action clickAction;

        public ButtonData(string display, Action clickAction)
        {
            this.display = display;
            this.clickAction = clickAction;
        }
    }

    [SerializeField] private GameObject active;
    [SerializeField] private TextMeshProUGUI explain;
    [SerializeField] private PopupButton[] buttons;

    public bool IsOpen { get; private set; }

    private void Awake()
    {
        ServiceLocator.ForGlobal().Register(this);
        active.SetActive(false);
    }

    public void Open(string explain, params ButtonData[] buttonDatas)
    {
        IsOpen = true;
        active.SetActive(true);
        this.explain.text = explain;
        for(int i = 0; i < buttons.Length; i++)
        {
            if(i < buttonDatas.Length)
            {
                buttons[i].gameObject.SetActive(true);
                buttons[i].Initialize(buttonDatas[i].display, buttonDatas[i].clickAction);
            }
            else
            {
                buttons[i].gameObject.SetActive(false);
            }
        }
    }

    public void Close()
    {
        IsOpen = false;
        active.SetActive(false);
    }
}

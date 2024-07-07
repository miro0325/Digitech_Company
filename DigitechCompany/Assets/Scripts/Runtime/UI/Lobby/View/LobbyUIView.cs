using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyUIView : OptionSelectUIView
{
    private void Awake()
    {
        uiManager.RegisterView(this);
        Close();
    }

    protected override Action SetOption(int index)
    {
        switch(index)
        {
            case 0: return () => uiManager.Open<LobbyMultiPlayOptionUIView>();
            case 1: return () => uiManager.Open<SettingUIView>();
        }
        return null;
    }
}

using System;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyMultiPlayOptionUIView : OptionSelectUIView
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
            case 1:
                return null;
        }
        return null;
    }
}
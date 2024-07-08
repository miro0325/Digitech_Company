using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyUIView : OptionSelectUIView
{
    private PopupUI _popupUI;
    private PopupUI popupUI => _popupUI ??= ServiceLocator.ForGlobal().Get<PopupUI>();

    private void Awake()
    {
        uiManager.RegisterView(this);
        Close();
    }

    protected override Action GetOptionAction(int index)
    {
        switch (index)
        {
            case 0: return () => uiManager.Open<LobbyMultiplayOptionUIView>();
            case 1: return () => uiManager.Open<SettingUIView>();
            case 2:
                return () =>
                {
                    popupUI.Open(
                        "������ �����ðڽ��ϱ�?",
                        new PopupUI.ButtonData(
                            "������",
                            () =>
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#endif
                                Application.Quit();
                            }
                        ),
                        new PopupUI.ButtonData("���ư���")
                    );
                };
        }
        return null;
    }
}

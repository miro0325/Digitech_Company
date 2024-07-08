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
                        "정말로 나가시겠습니까?",
                        new PopupUI.ButtonData(
                            "나가기",
                            () =>
                            {
#if UNITY_EDITOR
                                UnityEditor.EditorApplication.isPlaying = false;
#endif
                                Application.Quit();
                            }
                        ),
                        new PopupUI.ButtonData("돌아가기")
                    );
                };
        }
        return null;
    }
}

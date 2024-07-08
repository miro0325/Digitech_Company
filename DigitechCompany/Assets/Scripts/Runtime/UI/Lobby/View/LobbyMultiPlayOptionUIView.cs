using System;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyMultiplayOptionUIView : OptionSelectUIView
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
        switch(index)
        {
            case 0:
                return () =>
                {
                    popupUI.Open(
                        "�̰���",
                        new PopupUI.ButtonData(
                            "�ݱ�",
                            null
                        )
                    );
                };
            case 1:
                return () => ServerConnectTask().Forget();
            case 2:
                return () => uiManager.CloseRecently();
        }
        return null;
    }

    private async UniTask ServerConnectTask()
    {
        popupUI.Open("������...");
        PhotonNetwork.ConnectUsingSettings();
        await UniTask.WaitForSeconds(0.1f);
        await UniTask.WaitUntil(() => PhotonNetwork.IsConnected);
        uiManager.Open<LobbyMultiplayView>();
        popupUI.Close();
    }
}
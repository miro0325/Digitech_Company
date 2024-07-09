using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using UniRx;
using UnityEngine;
using UnityEngine.InputSystem;

public class LobbyMultiplayOptionUIView : OptionSelectUIView, IConnectionCallbacks, ILobbyCallbacks
{
    private PopupUI _popupUI;
    private PopupUI popupUI => _popupUI ??= ServiceLocator.ForGlobal().Get<PopupUI>();

    private void Awake()
    {
        uiManager.RegisterView(this);
        Close();
    }

    public virtual void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public virtual void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    protected override Action GetOptionAction(int index)
    {
        switch (index)
        {
            case 0:
                return () =>
                {
                    popupUI.Open(
                        "미개봉",
                        new PopupUI.ButtonData(
                            "닫기",
                            null
                        )
                    );
                };
            case 1:
                return () =>
                {
                    popupUI.Open(
                        "접속중...",
                        new PopupUI.ButtonData(
                            "취소",
                            () =>
                            {
                                PhotonNetwork.LeaveLobby();
                                PhotonNetwork.Disconnect();
                            }
                        )
                    );
                    Debug.Log(PhotonNetwork.ConnectUsingSettings());
                };
            case 2:
                return () => uiManager.CloseRecently();
        }
        return null;
    }

    public void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void OnJoinedLobby()
    {
        uiManager.Open<LobbyMultiplayView>();
        popupUI.Close();
    }

    public void OnDisconnected(DisconnectCause cause)
    {
        if(cause != DisconnectCause.None && cause != DisconnectCause.DisconnectByClientLogic)
        popupUI.Open(
            $"서버 연결에 실패하였습니다.\n사유: {cause}",
            new PopupUI.ButtonData("확인")
        );
    }

    public void OnConnected() { }
    public void OnRegionListReceived(RegionHandler regionHandler) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string debugMessage) { }
    public void OnLeftLobby() { }
    public void OnRoomListUpdate(List<RoomInfo> roomList) { }
    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { }
}
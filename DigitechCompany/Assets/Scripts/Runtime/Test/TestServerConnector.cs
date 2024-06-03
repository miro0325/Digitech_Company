using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TestServerConnector : MonoBehaviourPunCallbacks
{
    private async void Start()
    {
        await ServiceLocator.ForGlobal().Get<DataContainer>().Load();
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom("Test", null, Photon.Realtime.TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("test-gamescene2");
    }
}

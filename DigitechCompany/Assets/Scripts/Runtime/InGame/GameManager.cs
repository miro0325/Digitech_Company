using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IService, IPunObservable
{
    //service
    private ItemManager itemManager;

    //inspector
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private HashSet<int> playerViewIDs = new();
    private bool isGameStart;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(isGameStart);
        }
        else
        {
            isGameStart = (bool)stream.ReceiveNext();
        }
    }

    private void Awake()
    {
        NetworkObject.InstantiateBuffered("Prefabs/Player", Vector3.up, Quaternion.identity);
        // NetworkObject.Instantiate("Prefabs/TestShovel", new Vector3(1, 8));
    }

    private void Start()
    {
        itemManager = ServiceLocator.For(this).Get<ItemManager>();

        photonView.RPC(nameof(NotifyPlayerJoin), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    private void NotifyPlayerJoin(Photon.Realtime.Player player)
    {
        photonView.RPC(nameof(OnGameInfoReceive), player, isGameStart);
        isGameStart = true;
    }

    [PunRPC]
    private void OnGameInfoReceive(bool isHost)
    {
        if(!isHost)
        {
            itemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());
        }
        else
        {
            itemManager.SyncItem();
        }
    }
}
using System;
using Photon.Pun;
using UnityEngine;

public class InGameLoader : MonoBehaviourPun, IService
{
    //service
    private GameManager gameManager;
    private ItemManager itemManager;

    //field
    private bool isHost;

    //property
    public bool IsHost => isHost;

    //event
    public event Action OnLoadComplete;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    private void Start()
    {
        gameManager = ServiceLocator.For(this).Get<GameManager>();
        itemManager = ServiceLocator.For(this).Get<ItemManager>();
        
        photonView.RPC(nameof(RequestLoad), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    private void RequestLoad(Photon.Realtime.Player player)
    {
        photonView.RPC(nameof(OnLoad), player, !isHost);
        if(!isHost) isHost = true;
    }

    [PunRPC]
    private void OnLoad(bool isHost)
    {
        if(isHost) //initialize
        {
            
        }
        else //sync
        {

        }

        gameManager.SpawnPlayer();
        OnLoadComplete?.Invoke();
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks, IService, IPunObservable
{
    //service
    private TestBasement testBasement;
    private ItemManager itemManager;

    //inspector
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private bool isGameWaiting = true;
    private bool isGameLoading;

    //property
    public bool IsGameWaiting => isGameWaiting;
    public bool IsGameLoading => isGameLoading;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    public void SpawnPlayer()
    {
        testBasement = ServiceLocator.GetEveryWhere<TestBasement>();
        itemManager = ServiceLocator.GetEveryWhere<ItemManager>();

        NetworkObject.InstantiateBuffered("Prefabs/Player", testBasement.transform.position + Vector3.up * 2, Quaternion.identity);
    }

    public void StartGame()
    {
        isGameWaiting = false;
        itemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());
        photonView.RPC(nameof(StartGameRpc), RpcTarget.Others);
    }

    [PunRPC]
    private void StartGameRpc()
    {
        itemManager.SyncItem();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext(isGameLoading);
            stream.SendNext(isGameWaiting);
        }
        else
        {
            isGameLoading = (bool)stream.ReceiveNext();
            isGameWaiting = (bool)stream.ReceiveNext();
        }
    }
}
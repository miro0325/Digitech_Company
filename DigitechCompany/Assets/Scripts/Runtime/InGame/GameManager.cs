using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;

public enum SyncTarget
{
    Item,

    End
}

public enum GameState
{
    Waiting,
    Loading,
    Processing
}

public class GameManager : MonoBehaviourPun, IService, IPunObservable
{
    //service
    private TestBasement testBasement;
    private TestBasement TestBasement
    {
        get
        {
            if (ReferenceEquals(testBasement, null))
                testBasement = ServiceLocator.For(this).Get<TestBasement>();
            return testBasement;
        }
    }
    private ItemManager itemManager;
    private ItemManager ItemManager
    {
        get
        {
            if (ReferenceEquals(itemManager, null))
                itemManager = ServiceLocator.For(this).Get<ItemManager>();
            return itemManager;
        }
    }
    private SpectatorView spectatorView;
    private SpectatorView SpectatorView
    {
        get
        {
            if(ReferenceEquals(spectatorView, null))
                spectatorView = ServiceLocator.For(this).Get<SpectatorView>();
            return spectatorView;
        }
    }
    private InGamePlayer player;
    private InGamePlayer Player
    {
        get
        {
            if(ReferenceEquals(player, null))
                player = ServiceLocator.For(this).Get<InGamePlayer>();
            return player;
        }
    }

    //inspector
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private GameState state;
    private Dictionary<Player, bool[]> syncDatas = new();

    //property
    public GameState State => state;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    private void Start()
    {
        NetworkObject.InstantiateBuffered("Prefabs/Player", TestBasement.transform.position + Vector3.up * 2, Quaternion.identity);
        // player.gameObject.SetActive(false);
        // photonView.RPC(nameof(PlayerJoinRpc), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    private void PlayerJoinRpc(Player newPlayer)
    {
        Debug.Log("Join");
        syncDatas.Add(newPlayer, new bool[(int)SyncTarget.End]);
    } 

    private void GameInstantiateRpc(GameState gameState)
    {
        state = gameState;
        switch (state)
        {
            case GameState.Waiting:
                Player.gameObject.SetActive(true);
                break;
            case GameState.Loading:
                
                break;
            case GameState.Processing:
                break;
            default:
                break;
        }
    }

    public void RequestStartGame()
    {
        if (state != GameState.Waiting) return;
        state = GameState.Loading;
        
        if(photonView.IsMine) StartGameRoutine().Forget();
        else photonView.RPC(nameof(StartGameRpc), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void StartGameRpc()
    {
        if (state != GameState.Waiting) return;
        state = GameState.Loading;

        StartGameRoutine().Forget();
    }

    /// <summary>
    /// This function must be executed only when you are a host.
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid StartGameRoutine()
    {
        //initialize
        var itemDataJson = ItemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());

        //send rpc
        photonView.RPC(nameof(SyncRpc), RpcTarget.Others, (int)SyncTarget.Item, itemDataJson);

        await UniTask.WaitUntil(() =>
        {
            foreach(var syncData in syncDatas)
                foreach(var b in syncData.Value)
                    if(!b) return false;
            return true;
        });

        state = GameState.Processing;
        Debug.Log("Complete Crew");
        testBasement.MoveDown();
    }

    [PunRPC]
    private void SyncRpc(int syncTarget, string data)
    {
        switch ((SyncTarget)syncTarget)
        {
            case SyncTarget.Item:
                ItemManager.SyncItem(data);
                photonView.RPC(nameof(NotifySyncComplete), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, syncTarget);
                break;
            default:
                break;
        }
    }

    [PunRPC]
    private void NotifySyncComplete(Player player, int syncTarget)
    {
        syncDatas[player][syncTarget] = true;
    }

    public void RequestEndGame()
    {
        if(TestBasement.State != TestBasementState.Down) return;
        EndGameRoutine().Forget();
    }

    /// <summary>
    /// This function must be executed only when you are a host.
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid EndGameRoutine()
    {
        TestBasement.MoveUp();

        await UniTask.WaitUntil(() => TestBasement.State == TestBasementState.Up);

        ItemManager.DestoryItems(true);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)state);
        }
        else
        {
            state = (GameState)(int)stream.ReceiveNext();
        }
    }
}
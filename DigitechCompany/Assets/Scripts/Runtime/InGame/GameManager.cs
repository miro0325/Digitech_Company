using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

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

public class GameManager : MonoBehaviourPunCallbacks, IService, IPunObservable
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

    //inspector
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private GameState state;
    private Dictionary<Photon.Realtime.Player, bool[]> syncDatas = new();

    //property
    public GameState State => state;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    private void Start()
    {
        if (photonView.IsMine) InitializeGame();
        else SyncGame();

        NetworkObject.InstantiateBuffered("Prefabs/Player", TestBasement.transform.position + Vector3.up * 2, Quaternion.identity);
    }

    private void InitializeGame()
    {

    }

    private void SyncGame()
    {

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
    private void NotifySyncComplete(Photon.Realtime.Player player, int syncTarget)
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

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("Join");
        syncDatas.Add(newPlayer, new bool[(int)SyncTarget.End]);
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
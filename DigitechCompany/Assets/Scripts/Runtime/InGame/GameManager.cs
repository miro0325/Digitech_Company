using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;

public enum SyncTarget
{
    Item
}

public enum GameState
{
    Waiting,
    Loading,
    Progressing
}

public class GameManager : MonoBehaviourPunCallbacks, IService, IPunObservable
{
    //service
    private TestBasement testBasement;
    private ItemManager itemManager;

    //inspector
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private GameState gameState;
    private Dictionary<Photon.Realtime.Player, bool[]> syncData;

    //property
    public GameState GameState => gameState;

    //event
    public event Action OnInitializeComplete;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
    }

    private void Start()
    {
        testBasement = ServiceLocator.For(this).Get<TestBasement>();
        itemManager = ServiceLocator.For(this).Get<ItemManager>();

        if(photonView.IsMine) InitializeGame();
        else SyncGame();

        NetworkObject.InstantiateBuffered("Prefabs/Player", testBasement.transform.position + Vector3.up * 2, Quaternion.identity);

        OnInitializeComplete?.Invoke();
    }

    private void InitializeGame()
    {

    }

    private void SyncGame()
    {

    }

    public void RequestStartGame()
    {
        photonView.RPC(nameof(StartGameRpc), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void StartGameRpc()
    {
        if(gameState != GameState.Waiting) return;
        gameState = GameState.Loading;

        StartGameRoutine().Forget();
    }

    /// <summary>
    /// This function must be executed only when you are a host.
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid StartGameRoutine()
    {
        //initialize
        var itemDataJson = itemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());

        //send rpc
        photonView.RPC(nameof(SyncRpc), RpcTarget.Others, (int)SyncTarget.Item, itemDataJson);
    }

    [PunRPC]
    private void SyncRpc(int syncTarget, string data)
    {
        switch ((SyncTarget)syncTarget)
        {
            case SyncTarget.Item:
                break;
            default:
                break;
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        Debug.Log("Join");
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if(stream.IsWriting)
        {
            stream.SendNext((int)gameState);
        }
        else
        {
            gameState = (GameState)(int)stream.ReceiveNext();
        }
    }
}
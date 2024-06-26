using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.InputSystem;

public enum SyncTarget
{
    Item,
    TestCapture,

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
    [Serializable]
    public class TestTrapData
    {
        public string prefab;
        public Vector3 position;
        public Vector3 rotation;

        [HideInInspector] public int viewid;
    }

    //service
    private ItemManager _itemManager;
    private ItemManager itemManager => _itemManager ??= ServiceLocator.For(this).Get<ItemManager>();
    private SpectatorView _spectatorView;
    private SpectatorView spectatorView => _spectatorView ??= ServiceLocator.For(this).Get<SpectatorView>();
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();

    //inspector
    [SerializeField] private MeshRenderer[] rooms;
    [SerializeField] private TestBasement testBasement;
    [SerializeField] private TestTrapData[] trapDatas;

    //field
    private bool gameEndSign;
    private bool gameStartSign;
    private Dictionary<Player, bool[]> syncDatas = new();
    /* sync */
    private GameState state;
    /* sync */
    Dictionary<Player, int> inGamePlayerViewIds = new();
    /* sync */
    private HashSet<int> alivePlayers = new();

    //property
    public GameState State => state;
    public HashSet<int> AlivePlayers => alivePlayers;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);

        var viewId = NetworkObject.InstantiateBuffered("Prefabs/Player", testBasement.transform.position + Vector3.up * 2, Quaternion.identity).photonView.ViewID;
        photonView.RPC(nameof(NotifyPlayerJoinRpc), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, viewId);
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void NotifyPlayerJoinRpc(Player newPlayer, int inGamePlayerViewId)
    {
        syncDatas.Add(newPlayer, new bool[(int)SyncTarget.End]);
        inGamePlayerViewIds.Add(newPlayer, inGamePlayerViewId);
        photonView.RPC(nameof(GameInstantiateRpc), newPlayer, (int)state);
    }

    [PunRPC]
    private void GameInstantiateRpc(int gameState)
    {
        if (photonView.IsMine) GameRoutine().Forget();

        state = (GameState)gameState;

        switch (state)
        {
            case GameState.Waiting:
                player.Revive();
                break;
            case GameState.Loading:
            case GameState.Processing:
                photonView.RPC(nameof(RequestSyncRpc), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
                break;
        }
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void RequestSyncRpc(Player player)
    {
        photonView.RPC(nameof(SyncRpc), player, (int)SyncTarget.Item, itemManager.ItemDataJson);
    }

    [PunRPC]
    private void SyncRpc(int syncTarget, string data)
    {
        switch ((SyncTarget)syncTarget)
        {
            case SyncTarget.Item:
                itemManager.SyncItem(data);
                break;
            case SyncTarget.TestCapture:
                trapDatas = JsonSerializer.JsonToArray<TestTrapData>(data);
                for (int i = 0; i < trapDatas.Length; i++)
                    NetworkObject.Sync(trapDatas[i].prefab, trapDatas[i].viewid, trapDatas[i].position, Quaternion.Euler(trapDatas[i].rotation));
                break;
            default:
                break;
        }
        photonView.RPC(nameof(NotifySyncComplete), RpcTarget.MasterClient, PhotonNetwork.LocalPlayer, syncTarget);
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void NotifySyncComplete(Player player, int syncTarget)
    {
        syncDatas[player][syncTarget] = true;
    }

    public void RequestStartGame()
    {
        if (state != GameState.Waiting) return;
        photonView.RPC(nameof(StartGameRpc), RpcTarget.MasterClient);
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void StartGameRpc()
    {
        state = GameState.Loading;
        gameStartSign = true;
    }

    public void RequestEndGame()
    {
        if (testBasement.State != TestBasementState.Down) return;
        photonView.RPC(nameof(EndGameRpc), RpcTarget.MasterClient);
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void EndGameRpc()
    {
        gameEndSign = true;
    }

    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private async UniTaskVoid GameRoutine()
    {
        while (true)
        {
            state = GameState.Waiting;

            Debug.Log(state);
            Debug.Log(testBasement.State);

            gameStartSign = false;
            await UniTask.WaitUntil(() => gameStartSign);

            alivePlayers = inGamePlayerViewIds.Values.ToHashSet();

            itemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());
            syncDatas[PhotonNetwork.LocalPlayer][(int)SyncTarget.Item] = true;
            for (int i = 0; i < trapDatas.Length; i++)
                trapDatas[i].viewid = NetworkObject.Instantiate(trapDatas[i].prefab, trapDatas[i].position, Quaternion.Euler(trapDatas[i].rotation)).photonView.ViewID;
            syncDatas[PhotonNetwork.LocalPlayer][(int)SyncTarget.TestCapture] = true;

            //send rpc
            photonView.RPC(nameof(SyncRpc), RpcTarget.Others, (int)SyncTarget.Item, itemManager.ItemDataJson);
            photonView.RPC(nameof(SyncRpc), RpcTarget.Others, (int)SyncTarget.TestCapture, trapDatas.ToJson());

            await UniTask.WaitUntil(() =>
            {
                foreach (var syncData in syncDatas)
                    foreach (var b in syncData.Value)
                        if (!b) return false;
                return true;
            });

            state = GameState.Processing;
            testBasement.MoveDown();

            //game end
            gameEndSign = false;
            await UniTask.WaitUntil(() => gameEndSign || alivePlayers.Count == 0);

            testBasement.MoveUp();
            await UniTask.WaitUntil(() => testBasement.State == TestBasementState.Up);
            itemManager.DestoryItems(true);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)state);
            stream.SendNext(alivePlayers.Count);
            foreach (var player in alivePlayers)
                stream.SendNext(player);
            stream.SendNext(inGamePlayerViewIds.Count);
            foreach (var viewid in inGamePlayerViewIds)
            {
                stream.SendNext(viewid.Key);
                stream.SendNext(viewid.Value);
            }
        }
        else
        {
            state = (GameState)(int)stream.ReceiveNext();
            var count = (int)stream.ReceiveNext();
            alivePlayers.Clear();
            for (int i = 0; i < count; i++)
                alivePlayers.Add((int)stream.ReceiveNext());
            count = (int)stream.ReceiveNext();
            inGamePlayerViewIds.Clear();
            for (int i = 0; i < count; i++)
                inGamePlayerViewIds.Add((Player)stream.ReceiveNext(), (int)stream.ReceiveNext());
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        syncDatas.Remove(otherPlayer);
        alivePlayers.Remove(inGamePlayerViewIds[otherPlayer]);
        inGamePlayerViewIds.Remove(otherPlayer);
    }
}
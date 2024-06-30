using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.InputSystem;
using MSLIMA.Serializer;
using Sherbert.Framework.Generic;

public enum SyncTarget
{
    Item,
    TestTrap,

    End
}

public enum GameState
{
    StartWait,
    Load,
    Process,
    End,
    DisplayResult
}

public class GameManager : MonoBehaviourPunCallbacks, IService, IPunObservable
{
    [Serializable]
    public class TestTrapData
    {
        public string prefab;
        public Vector3 position;
        public Vector3 rotation;
        public int viewid;
    }

    [Serializable]
    public class PlayerData
    {
        public bool isAlive;
        public bool[] sync;
        public int inGamePlayerViewId;
        public string playerName;

        public static byte[] Serialize(object customObject)
        {
            var data = (PlayerData)customObject;
            var bytes = new byte[0];
            Serializer.Serialize(data.isAlive, ref bytes);
            Serializer.Serialize(data.sync, ref bytes);
            Serializer.Serialize(data.inGamePlayerViewId, ref bytes);
            Serializer.Serialize(data.playerName, ref bytes);
            return bytes;
        }
        public static object Deserialize(byte[] bytes)
        {
            var data = new PlayerData();
            int offset = 0;
            data.isAlive = Serializer.DeserializeBool(bytes, ref offset);
            data.sync = Serializer.DeserializeBoolArray(bytes, ref offset);
            data.inGamePlayerViewId = Serializer.DeserializeInt(bytes, ref offset);
            data.playerName = Serializer.DeserializeString(bytes, ref offset);
            return data;
        }
    }

    //service
    private ItemManager _itemManager;
    private ItemManager itemManager => _itemManager ??= ServiceLocator.For(this).Get<ItemManager>();
    private SpectatorView _spectatorView;
    private SpectatorView spectatorView => _spectatorView ??= ServiceLocator.For(this).Get<SpectatorView>();
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();
    private Basement _basement;
    private Basement basement => _basement ??= ServiceLocator.For(this).Get<Basement>();

    //inspector
    [SerializeField] private MeshRenderer[] rooms;
    [SerializeField] private TestTrapData[] trapDatas;

    //field
    private int inGamePlayerViewId;
    private bool gameEndSign;
    private bool gameStartSign;
    private GameState state;
    [SerializeField] private SerializableDictionary<Player, PlayerData> playerDatas = new();

    //property
    public GameState State => state;
    public SerializableDictionary<Player, PlayerData> PlayerDatas => playerDatas;
    public bool HasAlivePlayer => playerDatas.Where(p => p.Value.isAlive).Any();

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
        Serializer.RegisterCustomType<PlayerData>((byte)'C');
        inGamePlayerViewId = NetworkObject.InstantiateBuffered("Prefabs/Player").photonView.ViewID;
    }

    private void Start()
    {
        player.transform.SetPositionAndRotation(basement.transform.position + Vector3.up * 2, Quaternion.identity);
        photonView.RPC(nameof(SendPlayerJoinToAllRpc), RpcTarget.All, PhotonNetwork.LocalPlayer, inGamePlayerViewId);
    }

    [PunRPC]
    private void SendPlayerJoinToAllRpc(Player newPlayer, int inGamePlayerViewId)
    {
        Debug.LogError($"Player: {newPlayer} join");
        var newPlayerData = new PlayerData()
        {
            sync = new bool[(int)SyncTarget.End],
            inGamePlayerViewId = inGamePlayerViewId
        };

        playerDatas.Add(newPlayer, newPlayerData);

        //Delay needs due to photon serialization
        if(photonView.IsMine) this.Invoke(() => photonView.RPC(nameof(SendGameStateToClientRpc), newPlayer, (int)state), 1f);
    }

    public void SendPlayerState(Player player, bool isAlive)
    {
        photonView.RPC(nameof(SendPlayerStateToAllRpc), RpcTarget.All, player, isAlive);
    }

    [PunRPC]
    private void SendPlayerStateToAllRpc(Player player, bool isAlive)
    {
        Debug.LogError($"Player: {player} state is {isAlive}");
        Debug.LogError($"has key: {playerDatas.ContainsKey(player)}");
        playerDatas[player].isAlive = isAlive;              
        spectatorView.UpdateAlivePlayerList(
            playerDatas
            .Where(p => p.Value.isAlive)
            .Select(p => PhotonView.Find(p.Value.inGamePlayerViewId).GetComponent<InGamePlayer>())
            .ToList()
        );
    }

    [PunRPC]
    private void SendGameStateToClientRpc(int gameState)
    {
        if (photonView.IsMine) GameRoutine().Forget();

        state = (GameState)gameState;

        switch (state)
        {
            case GameState.StartWait:
                Debug.LogError($"Revive Player: {PhotonNetwork.LocalPlayer}");
                player.Revive();
                break;
            case GameState.Load:
            case GameState.Process:
                photonView.RPC(nameof(SendGameDataRequestToMasterRpc), photonView.Owner, PhotonNetwork.LocalPlayer);
                break;
        }
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void SendGameDataRequestToMasterRpc(Player player)
    {
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), player, (int)SyncTarget.Item, itemManager.ItemDataJson);
    }

    [PunRPC]
    private void SendGameDataLoadToClientRpc(int syncTarget, string data)
    {
        switch ((SyncTarget)syncTarget)
        {
            case SyncTarget.Item:
                itemManager.SyncItem(data);
                break;
            case SyncTarget.TestTrap:
                trapDatas = JsonSerializer.JsonToArray<TestTrapData>(data);
                for (int i = 0; i < trapDatas.Length; i++)
                    NetworkObject.Sync(trapDatas[i].prefab, trapDatas[i].viewid, trapDatas[i].position, Quaternion.Euler(trapDatas[i].rotation));
                break;
            default:
                break;
        }
        photonView.RPC(nameof(SendLoadCompleteToMasterRpc), photonView.Owner, PhotonNetwork.LocalPlayer, syncTarget);
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void SendLoadCompleteToMasterRpc(Player player, int syncTarget)
    {
        playerDatas[player].sync[syncTarget] = true;
    }

    public void RequestStartGame()
    {
        if (state != GameState.StartWait) return;
        photonView.RPC(nameof(SendStartGameRpc), photonView.Owner);
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void SendStartGameRpc()
    {
        state = GameState.Load;
        gameStartSign = true;
    }

    public void RequestEndGame()
    {
        if (basement.CurState != Basement.State.Down) return;
        photonView.RPC(nameof(SendEndGameRpc), photonView.Owner);
    }

    [PunRPC]
    /// <summary>
    /// Must run in Master Client
    /// </summary>
    /// <returns></returns>
    private void SendEndGameRpc()
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
            state = GameState.StartWait;

            gameStartSign = false;
            await UniTask.WaitUntil(() => gameStartSign);

            state = GameState.Load;
            InitializeGameAndRequestLoad();

            //Wait until all player sync complete
            await UniTask.WaitUntil(() =>
            {
                foreach (var playerData in playerDatas)
                    foreach (var b in playerData.Value.sync)
                        if (!b) return false;
                return true;
            });

            state = GameState.Process;
            basement.MoveDown();
            gameEndSign = false;

            //Wait until game end sign is come or all player were die
            await UniTask.WaitUntil(() => gameEndSign || !HasAlivePlayer);
            state = GameState.End;

            basement.MoveUp();
            await UniTask.WaitUntil(() => basement.CurState == Basement.State.Up);

            state = GameState.DisplayResult;
            itemManager.DestoryItems(true);

            await UniTask.WaitForSeconds(5f);
        }
    }

    private void InitializeGameAndRequestLoad()
    {
        itemManager.SpawnItem(1, rooms.Select(r => r.bounds).ToArray());
        playerDatas[PhotonNetwork.LocalPlayer].sync[(int)SyncTarget.Item] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Item, itemManager.ItemDataJson);

        for (int i = 0; i < trapDatas.Length; i++)
            trapDatas[i].viewid = NetworkObject.Instantiate(trapDatas[i].prefab, trapDatas[i].position, Quaternion.Euler(trapDatas[i].rotation)).photonView.ViewID;
        playerDatas[PhotonNetwork.LocalPlayer].sync[(int)SyncTarget.TestTrap] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.TestTrap, trapDatas.ToJson());
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext((int)state);
            stream.SendNext(playerDatas.Count);
            foreach (var data in playerDatas)
            {
                stream.SendNext(data.Key);
                stream.SendNext(data.Value);
            }
        }
        else
        {
            state = (GameState)(int)stream.ReceiveNext();
            playerDatas.Clear();
            var count = (int)stream.ReceiveNext();
            for(int i = 0; i < count; i++)
                playerDatas.Add((Player)stream.ReceiveNext(), (PlayerData)stream.ReceiveNext());
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerDatas.Remove(otherPlayer);
    }
}
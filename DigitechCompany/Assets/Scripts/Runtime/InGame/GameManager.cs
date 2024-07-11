using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using MSLIMA.Serializer;
using Sherbert.Framework.Generic;
using Unity.AI.Navigation;

public enum SyncTarget
{
    Player,
    Map,
    Item,

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
    public class PlayerData : IEqualityComparer<PlayerData>
    {
        public int viewID;
        public bool isAlive;
        public bool[] sync = new bool[(int)SyncTarget.End];
        public string playerName = "플레이어";
        public float gainDamage;
        public float fearAmount;
        public bool isGainMaxDamage;
        public bool isMostParanoia;

        public bool IsAllDataSync => !sync.Where(sync => sync == false).Any();

        private PlayerData() { }
        public PlayerData(int viewid)
        {
            this.viewID = viewid;
        }

        public static byte[] Serialize(object customObject)
        {
            var data = (PlayerData)customObject;
            var bytes = new byte[0];
            Serializer.Serialize(data.viewID, ref bytes);
            Serializer.Serialize(data.isAlive, ref bytes);
            Serializer.Serialize(data.sync, ref bytes);
            Serializer.Serialize(data.playerName, ref bytes);
            return bytes;
        }
        public static object Deserialize(byte[] bytes)
        {
            var data = new PlayerData();
            int offset = 0;
            data.viewID = Serializer.DeserializeInt(bytes, ref offset);
            data.isAlive = Serializer.DeserializeBool(bytes, ref offset);
            data.sync = Serializer.DeserializeBoolArray(bytes, ref offset);
            data.playerName = Serializer.DeserializeString(bytes, ref offset);
            return data;
        }

        public bool Equals(PlayerData x, PlayerData y)
        {
            return x.viewID == y.viewID;
        }

        public int GetHashCode(PlayerData obj)
        {
            return obj.viewID.GetHashCode();
        }
    }

    //service
    private ItemManager _itemManager;
    private ItemManager itemManager => _itemManager ??= ServiceLocator.For(this).Get<ItemManager>();
    private TestSpawner _testSpawner;
    private TestSpawner testSpawner => _testSpawner ??= ServiceLocator.For(this).Get<TestSpawner>();
    private SpectatorView _spectatorView;
    private SpectatorView spectatorView => _spectatorView ??= ServiceLocator.For(this).Get<SpectatorView>();
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();
    private Basement _basement;
    private Basement basement => _basement ??= ServiceLocator.For(this).Get<Basement>();

    //inspector
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private int inGamePlayerViewId;
    private bool gameEndSign;
    private bool gameStartSign;
    private GameState state;
    [SerializeField] private bool[] joinSyncCompleted = new bool[(int)SyncTarget.End];
    [SerializeField] private SerializableDictionary<int, PlayerData> playerDatas = new();

    //property
    public GameState State => state;
    public SerializableDictionary<int, PlayerData> PlayerDatas => playerDatas;
    public bool HasAlivePlayer => playerDatas.Where(p => p.Value.isAlive).Any();

    public event Action OnLoadComplete;

    private void Awake()
    {
        ServiceLocator.For(this).Register(this);
        Serializer.RegisterCustomType<PlayerData>((byte)'C');
    }

    private void Start()
    {
        JoinLoadTask().Forget();
    }

    public async UniTask JoinLoadTask()
    {
        photonView.RPC(nameof(SendJoinLoadToOwnerRpc), photonView.Owner, PhotonNetwork.LocalPlayer);
        await UniTask.WaitUntil(() => !joinSyncCompleted.Where(synced => synced == false).Any());

        inGamePlayerViewId = NetworkObject.Instantiate("Prefabs/Player", basement.transform.position, Quaternion.identity).photonView.ViewID;
        photonView.RPC(nameof(SendPlayerJoinToAllRpc), RpcTarget.All, PhotonNetwork.LocalPlayer, inGamePlayerViewId);
        photonView.RPC(nameof(SendRequestGameStateToOwnerRpc), photonView.Owner, PhotonNetwork.LocalPlayer);
    }

    [PunRPC]
    private void SendPlayerJoinToAllRpc(Player newPlayer, int inGamePlayerViewId)
    {
        Debug.LogError($"Player: {newPlayer.ActorNumber} join");
        var newPlayerData = new PlayerData(inGamePlayerViewId);
        playerDatas.Add(newPlayer.ActorNumber, newPlayerData);
    }

    [PunRPC]
    private void SendRequestGameStateToOwnerRpc(Player player)
    {
        photonView.RPC(nameof(SendGameStateToClientRpc), player, (int)state);
    }

    [PunRPC]
    private void SendGameStateToClientRpc(int state)
    {
        this.state = (GameState)state;

        switch (this.state)
        {
            case GameState.StartWait:
                player.Revive();
                break;
        }

        if (photonView.IsMine) GameRoutine().Forget();
        OnLoadComplete?.Invoke();
    }

    [PunRPC]
    private void SendJoinLoadToOwnerRpc(Player player)
    {
        Debug.Log("Send To Client");
        var playerDatas = new Dictionary<int, PlayerData>();
        foreach (var data in this.playerDatas) playerDatas.Add(data.Key, data.Value);

        photonView.RPC(nameof(SendJoinLoadDataToClientRpc), player, (int)SyncTarget.Player, DictionaryJsonUtility.ToJson(playerDatas));
        photonView.RPC(nameof(SendJoinLoadDataToClientRpc), player, (int)SyncTarget.Map, null);
        photonView.RPC(nameof(SendJoinLoadDataToClientRpc), player, (int)SyncTarget.Item, itemManager.ItemDataJson);
    }

    [PunRPC]
    private void SendJoinLoadDataToClientRpc(int syncTarget, string data)
    {
        Debug.Log(data);
        switch ((SyncTarget)syncTarget)
        {
            case SyncTarget.Item:
                itemManager.SyncItem(data);
                break;
            case SyncTarget.Player:
                var players = DictionaryJsonUtility.FromJson<int, PlayerData>(data);
                foreach (var player in players)
                    playerDatas.Add(player.Key, player.Value);
                Debug.Log(playerDatas.Count);
                foreach (var kvp in playerDatas)
                {
                    if (kvp.Value.viewID != 0)
                        NetworkObject.Sync("Prefabs/Player", kvp.Value.viewID);
                }
                break;
            case SyncTarget.Map:
                surface.BuildNavMesh();
                break;
        }

        joinSyncCompleted[syncTarget] = true;
    }

    public void SendPlayerState(Player player, bool isAlive)
    {
        photonView.RPC(nameof(SendPlayerStateToAllRpc), RpcTarget.All, player, isAlive);
    }

    [PunRPC]
    private void SendPlayerStateToAllRpc(Player player, bool isAlive)
    {
        Debug.LogError($"Player: {player} state is {isAlive}");
        Debug.LogError($"has key: {playerDatas.ContainsKey(player.ActorNumber)}");
        playerDatas[player.ActorNumber].isAlive = isAlive;
        spectatorView.UpdateAlivePlayerList(
            playerDatas
            .Where(p => p.Value.isAlive)
            .Select(p =>
            {
                Debug.Log(p.Value.viewID);
                var pv = PhotonView.Find(p.Value.viewID);
                Debug.Log(pv.gameObject.name);
                return pv.GetComponent<InGamePlayer>();
            })
            .ToList()
        );
    }

    [PunRPC]
    private void SendLoadCompleteToOwnerRpc(Player player, int syncTarget)
    {
        playerDatas[player.ActorNumber].sync[syncTarget] = true;
    }

    public void RequestStartGame()
    {
        if (state != GameState.StartWait) return;
        photonView.RPC(nameof(SendStartGameRpc), photonView.Owner);
    }

    [PunRPC]
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
            foreach(var data in playerDatas)
            {
                var player = PhotonView.Find(data.Value.viewID).GetComponent<InGamePlayer>();
                if(!data.Value.isAlive)
                {
                    player.SetPositionAndRotation(basement.transform.position + Vector3.up, Quaternion.Euler(0, 0, 0));
                    player.Revive();
                }
            }

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

            //calcuate result
            playerDatas
                .Values
                .OrderBy(data => data.gainDamage)
                .First()
                .isGainMaxDamage = true;
            
            var cowards = playerDatas
                .Values
                .Where(data => data.fearAmount > 40)
                .ToArray();
            
            if(cowards.Length > 0)
                cowards[0].isMostParanoia = true;
            
            basement.MoveUp();
            await UniTask.WaitUntil(() => basement.CurState == Basement.State.Up);

            state = GameState.DisplayResult;
            itemManager.DestoryItems(true);

            await UniTask.WaitForSeconds(3f);
        }
    }

    private void InitializeGameAndRequestLoad()
    {
        // var map = PhotonNetwork.InstantiateRoomObject("Prefabs/Maps/Map1", new Vector3(0, -50, 0), Quaternion.identity);
        // var rooms = map.GetComponentsInChildren<MeshRenderer>().Where(m => m.CompareTag("Room")).Select(mesh => mesh.bounds).ToArray();
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Player] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Player, null);

        surface.BuildNavMesh();
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Map] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Map, null);

        Debug.Log(rooms.Length);
        itemManager.SpawnItem(1, rooms.Select(m => m.bounds).ToArray());
        testSpawner.SpawnMonsters();
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Item] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Item, itemManager.ItemDataJson);
    }

    [PunRPC]
    private void SendGameDataLoadToClientRpc(int syncTarget, string datas)
    {
        switch ((SyncTarget)syncTarget)
        {
            case SyncTarget.Item:
                itemManager.SyncItem(datas);
                break;
            case SyncTarget.Map:
                surface.BuildNavMesh();
                break;
        }
        photonView.RPC(nameof(SendLoadCompleteToOwnerRpc), photonView.Owner, PhotonNetwork.LocalPlayer, syncTarget);
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
                playerDatas.Add((int)stream.ReceiveNext(), (PlayerData)stream.ReceiveNext());
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerDatas.Remove(otherPlayer);
    }
}
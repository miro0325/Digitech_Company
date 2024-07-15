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
        public string playerName = "�÷��̾�";
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
        player.transform.SetParent(basement.transform);
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

        if (this.state == GameState.StartWait) player.Revive();
        if (photonView.IsMine) GameRoutine().Forget();

        Debug.LogError("OnLoadComplete");
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
                Debug.Log(players.Count);
                foreach (var kvp in playerDatas)
                {
                    if (kvp.Value.viewID != 0)
                        NetworkObject.Sync("Prefabs/Player", kvp.Value.viewID);
                }
                break;
            case SyncTarget.Map:
                // surface.BuildNavMesh();
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
            foreach (var data in playerDatas)
            {
                var player = PhotonView.Find(data.Value.viewID).GetComponent<InGamePlayer>();
                if (!data.Value.isAlive)
                {
                    player.SetPositionAndRotation(basement.transform.position + Vector3.up, Quaternion.Euler(0, 0, 0));
                    player.Revive();
                }
            }

            gameStartSign = false;
            await UniTask.WaitUntil(() => gameStartSign);

            state = GameState.Load;
            await InitializeGameAndRequestLoad();

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

            if (cowards.Length > 0)
                cowards[0].isMostParanoia = true;

            basement.MoveUp();
            await UniTask.WaitUntil(() => basement.CurState == Basement.State.Up);

            state = GameState.DisplayResult;
            itemManager.DestoryItems(true);

            await UniTask.WaitForSeconds(3f);
        }
    }

    private async UniTask InitializeGameAndRequestLoad()
    {
        //Player
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Player] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Player, null);

        //Map
        var inmap = Instantiate(Resources.Load<InMap>("Prefabs/Maps/In/Map1"), new Vector3(0, -50, 0), Quaternion.identity);
        var outmap = Instantiate(Resources.Load<OutMap>("Prefabs/Maps/Out/Map1"), new Vector3(0, 0, 0), Quaternion.identity);

        inmap.ToGround.position = outmap.EnterPoint.position;
        outmap.ToMap.position = inmap.EnterPoint.position;

        basement.transform.SetParent(outmap.ArrivePoint);

        inmap.Doors.For((_, door) => door.gameObject.SetActive(false));
        await UniTask.NextFrame();
        surface.BuildNavMesh();
        await UniTask.NextFrame();
        inmap.Doors.For((_, door) => door.gameObject.SetActive(true));

        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Map, null);
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Map] = true;

        //Item
        Debug.Log(rooms.Length);
        itemManager.SpawnItem(1, inmap.MapBounds);
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Item] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Item, itemManager.ItemDataJson);
        /* -------------------------- */

        //testSpawner.SpawnMonsters();
        // testSpawner.SpawnMonsters(1, rooms);
    }

    [PunRPC]
    private void SendGameDataLoadToClientRpc(int syncTarget, string datas)
    {
        GameDataLoadTask((SyncTarget)syncTarget, datas).Forget();

        async UniTask GameDataLoadTask(SyncTarget syncTarget, string datas)
        {
            switch (syncTarget)
            {
                case SyncTarget.Player:
                    break;
                case SyncTarget.Item:
                    itemManager.SyncItem(datas);
                    break;
                case SyncTarget.Map:
                    var inmap = PhotonNetwork.InstantiateRoomObject("Prefabs/Maps/InMap1", new Vector3(0, -50, 0), Quaternion.identity).GetComponent<InMap>();
                    var outmap = PhotonNetwork.InstantiateRoomObject("Prefabs/OutMaps/OutMap1", new Vector3(0, 0, 0), Quaternion.identity).GetComponent<OutMap>();

                    inmap.ToGround.position = outmap.EnterPoint.position;
                    outmap.ToMap.position = inmap.EnterPoint.position;

                    basement.transform.SetParent(outmap.ArrivePoint);

                    inmap.Doors.For((_, door) => door.gameObject.SetActive(false));
                    await UniTask.NextFrame();
                    surface.BuildNavMesh();
                    await UniTask.NextFrame();
                    inmap.Doors.For((_, door) => door.gameObject.SetActive(true));
                    break;
            }
            photonView.RPC(nameof(SendLoadCompleteToOwnerRpc), photonView.Owner, PhotonNetwork.LocalPlayer, syncTarget);
        }
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
            for (int i = 0; i < count; i++)
                playerDatas.Add((int)stream.ReceiveNext(), (PlayerData)stream.ReceiveNext());
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        playerDatas.Remove(otherPlayer);
    }
}
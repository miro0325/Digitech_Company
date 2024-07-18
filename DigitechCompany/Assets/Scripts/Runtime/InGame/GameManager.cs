using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Photon.Pun;
using UnityEngine;
using Photon.Realtime;
using MSLIMA.Serializer;
using Sherbert.Framework.Generic;
using Unity.AI.Navigation;
using UnityEngine.AI;

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
    [System.Serializable]
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

    public struct EarnData : IEqualityComparer<EarnData>
    {
        public float gameTime;
        public List<ItemBase> items;
        public float wholeEarn;

        public bool Equals(EarnData x, EarnData y)
        {
            return Mathf.Approximately(x.gameTime, y.gameTime);
        }

        public int GetHashCode(EarnData obj)
        {
            return obj.gameTime.GetHashCode();
        }
    }

    //service
    private ItemManager _itemManager;
    private ItemManager itemManager => _itemManager ??= ServiceLocator.For(this).Get<ItemManager>();
    private MonsterManager _testSpawner;
    private MonsterManager testSpawner => _testSpawner ??= ServiceLocator.For(this).Get<MonsterManager>();
    private SpectatorView _spectatorView;
    private SpectatorView spectatorView => _spectatorView ??= ServiceLocator.For(this).Get<SpectatorView>();
    private InGamePlayer _player;
    private InGamePlayer player => _player ??= ServiceLocator.For(this).Get<InGamePlayer>();
    private Basement _basement;
    private Basement basement => _basement ??= ServiceLocator.For(this).Get<Basement>();
    private Delivery _delivery;
    private Delivery delivery => _delivery ??= ServiceLocator.For(this).Get<Delivery>();
    private SkyProcessor _skyProcessor;
    private SkyProcessor skyProcessor => _skyProcessor ??= ServiceLocator.For(this).Get<SkyProcessor>();

    //inspector
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private MeshRenderer[] rooms;

    //field
    private float targetEarn;
    private float curEarn;
    private float curUsableMoney;
    private int remainDay;
    private float dateTime;
    private int inGamePlayerViewId;
    private bool gameEndSign;
    private bool gameStartSign;
    private string planet = "Digitech";
    private GameState state;
    private OutMap outMap;
    private InMap inMap;
    private EarnData earnedData;
    [SerializeField] private bool[] joinSyncCompleted = new bool[(int)SyncTarget.End];
    [SerializeField] private SerializableDictionary<int, PlayerData> playerDatas = new();

    //property
    public GameState State => state;
    public SerializableDictionary<int, PlayerData> PlayerDatas => playerDatas;
    public bool HasAlivePlayer => playerDatas.Where(p => p.Value.isAlive).Any();
    public EarnData EarnedData => earnedData;

    public event System.Action OnLoadComplete;

    public void Earn(string viewIdListJson)
    {
        photonView.RPC(nameof(EarnRpc), RpcTarget.All, viewIdListJson);
    }

    [PunRPC]
    private void EarnRpc(string viewIdListJson)
    {
        var viewidList = viewIdListJson.ToList<int>();
        if (viewidList.Count == 0) return;

        var items = viewidList
            .Select(viewid => PhotonView.Find(viewid).GetComponent<ItemBase>())
            .ToList();

        var sum = items.Select(item => item.SellPrice).Sum();
        curEarn += sum;
        curUsableMoney += sum;

        earnedData = new EarnData() { gameTime = Time.time, items = items, wholeEarn = sum };
    }

    public void ChangePlanet(string planet)
    {
        photonView.RPC(nameof(SendChangePlanetToAllRpc), RpcTarget.All, planet);
    }

    [PunRPC]
    private void SendChangePlanetToAllRpc(string planet)
    {
        this.planet = planet;
    }

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
        player.SetParent(basement.photonView.ViewID);
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
        photonView.RPC(nameof(SendJoinLoadDataToClientRpc), player, (int)SyncTarget.Item, itemManager.ItemDataJson);
        if (state == GameState.StartWait) photonView.RPC(nameof(SendJoinLoadDataToClientRpc), player, (int)SyncTarget.Map, null);
        else photonView.RPC(nameof(SendJoinLoadDataToClientRpc), player, (int)SyncTarget.Map, $"{planet}\'{outMap?.GetReAllocatedData()}\'{inMap?.GetReAllocatedData()}");
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
                    {
                        var player = NetworkObject.Sync("Prefabs/Player", kvp.Value.viewID) as InGamePlayer;
                        if (!kvp.Value.isAlive)
                        {
                            player.Animator.SetEnableRagDoll(true);
                        }
                        else
                        {
                            player.Animator.SetActiveArmModel(false);
                            player.Animator.SetEnableRagDoll(false);
                            player.Animator.SetActivePlayerModel(true);
                        }
                    }
                }
                break;
            case SyncTarget.Map:
                if (string.IsNullOrEmpty(data)) break;

                var split = data.Split('\'');
                outMap = Instantiate(Resources.Load<OutMap>($"Prefabs/Maps/Out/{split[0]}"), new Vector3(0, 0, 0), Quaternion.identity);
                outMap.ReBindPhotonViews(split[1]);

                if (split[0] != "Company")
                {
                    inMap = Instantiate(Resources.Load<InMap>("Prefabs/Maps/In/Map"), new Vector3(0, -50, 0), Quaternion.identity);
                    inMap.ReBindPhotonViews(split[2]);

                    inMap.SetActiveDoors(false);
                    surface.BuildNavMesh();
                    inMap.SetActiveDoors(true);
                }
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

            if (targetEarn == 0)
            {
                targetEarn = Random.Range(120, 180);
            }
            else
            {
                if (remainDay == 0)
                {
                    if (curEarn < targetEarn)
                    {
                        //failed
                    }
                    targetEarn += Random.Range(targetEarn, targetEarn * 2);
                    remainDay = 3;
                }
                else
                {
                    remainDay--;
                }
            }

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
            await UniTask.WaitUntil(() => gameStartSign, cancellationToken: this.GetCancellationTokenOnDestroy());

            state = GameState.Load;
            await UniTask.WaitForSeconds(0.25f, cancellationToken: this.GetCancellationTokenOnDestroy());
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
            await UniTask.WaitUntil(() => gameEndSign || !HasAlivePlayer, cancellationToken: this.GetCancellationTokenOnDestroy());
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
            await UniTask.WaitUntil(() => basement.CurState == Basement.State.Up, cancellationToken: this.GetCancellationTokenOnDestroy());

            state = GameState.DisplayResult;
            itemManager.DestoryItems(true);
            photonView.RPC(nameof(DestoryMaps), RpcTarget.All);

            await UniTask.WaitForSeconds(3f, cancellationToken: this.GetCancellationTokenOnDestroy());
        }
    }

    [PunRPC]
    private void DestoryMaps()
    {
        basement.transform.SetParent(null);
        if (inMap) Destroy(inMap.gameObject);
        if (outMap) Destroy(outMap.gameObject);
    }

    private async UniTask InitializeGameAndRequestLoad()
    {
        //==================Player==================//
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Player] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Player, null);

        //==================Map==================//
        outMap = Instantiate(Resources.Load<OutMap>($"Prefabs/Maps/Out/{planet}"), new Vector3(0, 0, 0), Quaternion.identity);
        outMap.ReAllocatePhotonViews();

        if (planet != "Company")
        {
            inMap = Instantiate(Resources.Load<InMap>("Prefabs/Maps/In/Map"), new Vector3(0, -50, 0), Quaternion.identity);
            inMap.ReAllocatePhotonViews();

            inMap.ToGround.position = outMap.EnterPoint.position;
            inMap.ToGround.OnMove += player => player.SetInMap(false);

            outMap.ToMap.position = inMap.EnterPoint.position;
            outMap.ToMap.OnMove += player => player.SetInMap(true);

            inMap.SetActiveDoors(false);
            surface.BuildNavMesh();
            await UniTask.NextFrame();
            inMap.SetActiveDoors(true);
        }

        basement.transform.SetParent(outMap.ArrivePoint);
        basement.transform.localEulerAngles = Vector3.zero;

        delivery.transform.SetParent(outMap.DeliveryPoint);
        delivery.transform.SetLocalPositionAndRotation(new Vector3(0, 100, 0), Quaternion.Euler(0, 0, 0));

        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Map, $"{planet}\'{outMap?.GetReAllocatedData()}\'{inMap?.GetReAllocatedData()}");
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Map] = true;

        //==================Item==================//
        Debug.Log(rooms.Length);

        if (planet != "Company")
        {
            testSpawner.SpawnMonsters(1, inMap.MapBounds, inMap.WayPoints);
            itemManager.SpawnItem(1, inMap.MapBounds);
            itemManager.SpawnItem(new Vector3(29, 0, 29), "Railgun");
            itemManager.SpawnItem(new Vector3(28, 0, 28), "Drill");
            itemManager.SpawnItem(new Vector3(27, 0, 27), "Flashlight");
            testSpawner.SpawnWalls(inMap.WallPoints);
        }
        else
        {
            for (int i = 0; i < 8; i++)
                itemManager.SpawnItem(Vector3.up, "Mop");
            itemManager.SpawnItem(Vector3.up, "Drill");
            itemManager.SpawnItem(Vector3.up, "Railgun");
            itemManager.SpawnItem(Vector3.up, "Flashlight");
            itemManager.BuildItemDataJson();
        }
        playerDatas[PhotonNetwork.LocalPlayer.ActorNumber].sync[(int)SyncTarget.Item] = true;
        photonView.RPC(nameof(SendGameDataLoadToClientRpc), RpcTarget.Others, (int)SyncTarget.Item, itemManager.ItemDataJson);

        //==================Monster==================//
        //testSpawner.SpawnMonsters();
        // testSpawner.SpawnMonsters(1, rooms);
    }

    [PunRPC]
    private void SendGameDataLoadToClientRpc(int syncTarget, string data)
    {
        GameDataLoadTask((SyncTarget)syncTarget, data).Forget();

        async UniTask GameDataLoadTask(SyncTarget syncTarget, string data)
        {
            switch (syncTarget)
            {
                case SyncTarget.Player:
                    break;
                case SyncTarget.Item:
                    itemManager.SyncItem(data);
                    break;
                case SyncTarget.Map:
                    var split = data.Split('\'');
                    outMap = Instantiate(Resources.Load<OutMap>($"Prefabs/Maps/Out/{split[0]}"), new Vector3(0, 0, 0), Quaternion.identity);
                    outMap.ReBindPhotonViews(split[1]);

                    if (split[0] != "Company")
                    {
                        inMap = Instantiate(Resources.Load<InMap>("Prefabs/Maps/In/Map"), new Vector3(0, -50, 0), Quaternion.identity);
                        inMap.ReBindPhotonViews(split[2]);

                        inMap.ToGround.position = outMap.EnterPoint.position;
                        inMap.ToGround.OnMove += player => player.SetInMap(false);

                        outMap.ToMap.position = inMap.EnterPoint.position;
                        outMap.ToMap.OnMove += player => player.SetInMap(true);

                        surface.BuildNavMesh();
                        await UniTask.NextFrame();
                    }

                    basement.transform.SetParent(outMap.ArrivePoint);
                    basement.transform.localEulerAngles = Vector3.zero;

                    delivery.transform.SetParent(outMap.DeliveryPoint);
                    delivery.transform.SetLocalPositionAndRotation(new Vector3(0, 100, 0), Quaternion.Euler(0, 0, 0));
                    break;
            }
            photonView.RPC(nameof(SendLoadCompleteToOwnerRpc), photonView.Owner, PhotonNetwork.LocalPlayer, syncTarget);
        }
    }

    private void Update()
    {
        if (state == GameState.Process)
        {
            dateTime += Time.deltaTime;

            if (player.IsInMap)
            {
                skyProcessor.SetFogValue(Color.black, 0.1f);
            }
            else
            {
                var midnightTime = 10;
                var endTime = 30;
                if (dateTime < midnightTime)
                {
                    var lerp = dateTime / midnightTime;
                    skyProcessor.LerpSky(outMap.EnvirSetting.morning, outMap.EnvirSetting.midnight, lerp);
                }
                else
                {
                    var lerp = (dateTime - midnightTime) / (endTime - midnightTime);
                    skyProcessor.LerpSky(outMap.EnvirSetting.midnight, outMap.EnvirSetting.night, lerp);
                }
            }
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
            stream.SendNext(targetEarn);
            stream.SendNext(curEarn);
        }
        else
        {
            state = (GameState)(int)stream.ReceiveNext();
            playerDatas.Clear();
            var count = (int)stream.ReceiveNext();
            for (int i = 0; i < count; i++)
                playerDatas.Add((int)stream.ReceiveNext(), (PlayerData)stream.ReceiveNext());
            targetEarn = (float)stream.ReceiveNext();
            curEarn = (float)stream.ReceiveNext();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogError($"Player: {otherPlayer} left room");
        playerDatas.Remove(otherPlayer);
    }
}
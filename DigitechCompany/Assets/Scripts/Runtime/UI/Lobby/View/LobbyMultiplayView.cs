using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMultiplayView : UIView, ILobbyCallbacks
{
    [SerializeField] private RectTransform roomSlotParent;
    [SerializeField] private LobbyRoomSlot roomSlotPrefab;
    [SerializeField] private Button testButton;

    private List<LobbyRoomSlot> roomSlots = new();

    private void Awake()
    {
        uiManager.RegisterView(this);
        gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        testButton.onClick.AddListener(() =>
        {
            Debug.Log("Create Room");
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = 4 });
        });
    }

    public void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Room Updated");
        foreach(var info in roomList)
        {
            Debug.LogError(info.Name);
        }
        // if(roomSlots.Count > roomList.Count)
        // {
        //     for(int i = 0; i < roomSlots.Count - roomList.Count; i++)
        //     {
        //         Destroy(roomSlots[^1]);
        //         roomSlots.RemoveAt(roomSlots.Count - 1);
        //     }
        // }
        // else
        // {
        //     for(int i = 0; i < roomList.Count - roomSlots.Count; i++)
        //         roomSlots.Add(Instantiate(roomSlotPrefab, roomSlotParent));
        // }

        // for(int i = 0; i < roomList.Count; i++)
        //     roomSlots[i].Initialize(, );
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    public override void Open()
    {
        gameObject.SetActive(true);
    }

    public void OnJoinedLobby() { }
    public void OnLeftLobby() { }
    public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics) { }
}

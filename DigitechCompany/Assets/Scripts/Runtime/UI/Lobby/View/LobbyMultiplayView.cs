using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMultiplayView : UIView
{
    private LobbyPunCallbackReceiver _callbackReceiver;
    private LobbyPunCallbackReceiver callbackReceiver => _callbackReceiver ??= ServiceLocator.For(this).Get<LobbyPunCallbackReceiver>();

    [SerializeField] private RectTransform roomSlotParent;
    [SerializeField] private LobbyRoomSlot roomSlotPrefab;
    [SerializeField] private Button createRoom;
    [SerializeField] private MultiplayCreateRoomPopup createRoomPopup;

    private List<LobbyRoomSlot> roomSlots = new();

    private void Awake()
    {
        uiManager.RegisterView(this);
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
        createRoom.onClick.AddListener(() => createRoomPopup.Open());

        callbackReceiver.onRoomListUpdate += roomList =>
        {
            if (roomSlots.Count > roomList.Count)
            {
                for (int i = 0; i < roomSlots.Count - roomList.Count; i++)
                {
                    Destroy(roomSlots[^1]);
                    roomSlots.RemoveAt(roomSlots.Count - 1);
                }
            }
            else
            {
                for (int i = 0; i < roomList.Count - roomSlots.Count; i++)
                    roomSlots.Add(Instantiate(roomSlotPrefab, roomSlotParent));
            }

            for (int i = 0; i < roomList.Count; i++)
                roomSlots[i].Initialize(roomList[i]);
        };
        
        gameObject.SetActive(false);
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    public override void Open()
    {
        gameObject.SetActive(true);
    }
}

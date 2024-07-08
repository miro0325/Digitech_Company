using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayCreateRoomPopup : MonoBehaviour
{
    private PopupUI _popupUI;
    private PopupUI popupUI => _popupUI ??= ServiceLocator.ForGlobal().Get<PopupUI>();
    private LobbyPunCallbackReceiver _callbackReceiver;
    private LobbyPunCallbackReceiver callbackReceiver => _callbackReceiver ??= ServiceLocator.For(this).Get<LobbyPunCallbackReceiver>();

    [SerializeField] private TMP_InputField roomName;
    [SerializeField] private Slider playerAmountSlider;
    [SerializeField] private TextMeshProUGUI playerAmountText;
    [SerializeField] private Button create;
    [SerializeField] private Button cancle;

    private void Start()
    {
        create.onClick.AddListener(() =>
        {
            var customRoomProperties = new Hashtable() { { "roomName", roomName.text } };
            var customRoomPropertiesForLobby = new string[] { "roomName " };
            var roomOptions = new RoomOptions()
            {
                MaxPlayers = (int)playerAmountSlider.value,
                CustomRoomProperties = customRoomProperties,
                CustomRoomPropertiesForLobby = customRoomPropertiesForLobby
            };
            PhotonNetwork.CreateRoom(null, roomOptions);
            popupUI.Open("방 생성중...");
        });

        cancle.onClick.AddListener(() => Close());

        callbackReceiver.onCreatedRoom += () =>
        {
            PhotonNetwork.LoadLevel("InGame");
            popupUI.Close();
        };

        gameObject.SetActive(false);
    }

    private void Update()
    {
        playerAmountText.text = playerAmountSlider.value.ToString();
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}

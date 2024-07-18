using Cysharp.Threading.Tasks;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyRoomSlot : MonoBehaviour
{
    private PopupUI _popupUI;
    private PopupUI popupUI => _popupUI ??= ServiceLocator.ForGlobal().Get<PopupUI>();
    private LobbyPunCallbackReceiver _callbackReceiver;
    private LobbyPunCallbackReceiver callbackReceiver => _callbackReceiver ??= ServiceLocator.For(this).Get<LobbyPunCallbackReceiver>();
    private LobbyFadeOutUI _fadeOutUI;
    private LobbyFadeOutUI fadeOutUI => _fadeOutUI ??= ServiceLocator.For(this).Get<LobbyFadeOutUI>();

    [SerializeField] private TextMeshProUGUI roomName;
    [SerializeField] private TextMeshProUGUI data;

    private RoomInfo info;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            PhotonNetwork.JoinRoom(info.Name);
            popupUI.Open("방 접속중...");
        });
    }

    private void OnEnable()
    {
        callbackReceiver.onJoinedRoom += OnJoinedRoom;
        callbackReceiver.onJoinRoomFailed += OnJoinedRoomFailed;
    }

    private void OnDisable()
    {
        callbackReceiver.onJoinedRoom -= OnJoinedRoom;
        callbackReceiver.onJoinRoomFailed -= OnJoinedRoomFailed;
    }

    public void OnJoinedRoom()
    {
            popupUI.Close();
        fadeOutUI.FadeOut(2f, () => PhotonNetwork.LoadLevel("InGame"));
    }

    public void OnJoinedRoomFailed(short returnCode, string message)
    {
        popupUI.Open($"방 접속 실패\n{message}", new PopupUI.ButtonData("확인"));
    }

    public void Initialize(RoomInfo info)
    {
        roomName.text = (string)info.CustomProperties["roomName"];
        data.text = $"플레이어: {info.PlayerCount}/{info.MaxPlayers}";
        this.info = info;
    }
}
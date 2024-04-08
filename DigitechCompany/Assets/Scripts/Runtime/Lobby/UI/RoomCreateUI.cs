using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using ExitGames.Client.Photon;

namespace Game.Lobby
{
    public class RoomCreateUI : MonoBehaviour
    {
        private LobbyManager lobbyManager => ServiceProvider.Get<LobbyManager>();

        [SerializeField] private Image bg;
        [SerializeField] private TMP_InputField roomName;
        [SerializeField] private TMP_InputField maxPlayer;
        [SerializeField] private TMP_InputField password;
        [SerializeField] private Button create;
        [SerializeField] private Button cancle;

        private void Awake()
        {
            create.onClick.AddListener(() =>
            {
                var hash = new Hashtable { { "password", password.text } };
                var roomOption = new RoomOptions() { MaxPlayers = int.Parse(maxPlayer.text), CustomRoomProperties = hash };
                PhotonNetwork.CreateRoom(roomName.text, roomOptions: roomOption);
                lobbyManager.ConnectToRoom();
                Debug.Log("create");
            });

            cancle.onClick.AddListener(() => bg.gameObject.SetActive(false));
        }

        public void Display()
        {
            bg.gameObject.SetActive(true);
        }
    }
}
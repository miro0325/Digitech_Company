using Photon.Realtime;
using TMPro;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class RoomInfoSlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roomName;
        [SerializeField] private TextMeshProUGUI playerCount;
        [SerializeField] private Image lockImage;

        private RoomInfo roomInfo;

        public void Set(RoomInfo room)
        {
            roomInfo = room;
            lockImage.enabled = !string.IsNullOrWhiteSpace((string)roomInfo.CustomProperties["password"]);
        }

        private void Update()
        {
            roomName.text = roomInfo?.Name;
            playerCount.text = $"{roomInfo?.PlayerCount}/{roomInfo?.MaxPlayers}";
        }
    }
}
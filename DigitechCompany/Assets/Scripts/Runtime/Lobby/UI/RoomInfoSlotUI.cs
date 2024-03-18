using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class RoomInfoSlotUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI roomName;
        [SerializeField] private TextMeshProUGUI playerCount;

        private RoomInfo roomInfo;

        public void Set(RoomInfo room)
        {
            roomInfo = room;
        }

        private void Update()
        {
            roomName.text = roomInfo?.Name;
            playerCount.text = $"{roomInfo?.PlayerCount}/{roomInfo?.MaxPlayers}";
        }
    }
}
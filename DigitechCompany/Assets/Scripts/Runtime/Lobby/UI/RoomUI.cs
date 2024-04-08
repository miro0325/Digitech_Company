using System.Collections.Generic;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class RoomUI : MonoBehaviour
    {
        private LobbyManager lobbyManager => ServiceProvider.Get<LobbyManager>();

        [SerializeField] private Image bg;
        [SerializeField] private RoomInfoSlotUI slotPrefab;
        [SerializeField] private RectTransform slotParent;
        [SerializeField] private Button roomCreateButton;
        [SerializeField] private RoomCreateUI roomCreateUI;

        private List<RoomInfoSlotUI> slots = new();

        private void Start()
        {
            roomCreateButton.onClick.AddListener(() => roomCreateUI.Display());

            lobbyManager
                .ObserveEveryValueChanged(l => l.ConnectingState)
                .Where(c => c == ConnectingState.InLobby)
                .Subscribe(_ => bg.gameObject.SetActive(true));

            lobbyManager
                .ObserveEveryValueChanged(l => l.ConnectingState)
                .Where(c => c == ConnectingState.None)
                .Subscribe(_ => bg.gameObject.SetActive(false));

            lobbyManager
                .ObserveEveryValueChanged(l => l.Rooms.Count)
                .Subscribe(x =>
                {
                    while (slots.Count != x)
                    {
                        if (slots.Count > x)
                        {
                            Destroy(slots[^1].gameObject);
                            slots.RemoveAt(slots.Count - 1);
                        }
                        else
                        {
                            slots.Add(Instantiate(slotPrefab, slotParent));
                        }
                    }

                    for (int i = 0; i < x; i++)
                        slots[i].Set(lobbyManager.Rooms[i]);
                });
        }

        public void Display()
        {
            bg.gameObject.SetActive(true);
        }

        public void Hide()
        {
            bg.gameObject.SetActive(false);
        }
    }
}
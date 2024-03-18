using System.Collections.Generic;
using Photon.Pun;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class RoomDisplayUI : UIStackWindow
    {
        [SerializeField] private Image bg;
        [SerializeField] private RoomInfoSlotUI slotPrefab;
        [SerializeField] private RectTransform slotParent;
        [SerializeField] private Button testButton;

        private List<RoomInfoSlotUI> slots = new();

        private void Start()
        {
            testButton.onClick.AddListener(() =>
            {
                PhotonNetwork.CreateRoom("Test");
            });

            LobbyManager.Instance
                .ObserveEveryValueChanged(l => l.Rooms.Count)
                .Subscribe(x =>
                {
                    while (slots.Count != x)
                    {
                        if(slots.Count > x)
                        {
                            Destroy(slots[^1].gameObject);
                            slots.RemoveAt(slots.Count - 1);
                        }
                        else
                        {
                            slots.Add(Instantiate(slotPrefab, slotParent));
                        }
                    }

                    for(int i = 0; i < x; i++)
                        slots[i].Set(LobbyManager.Instance.Rooms[i]);
                });
        }

        public override void Display()
        {
            bg.gameObject.SetActive(true);
        }

        public override void Hide()
        {
            bg.gameObject.SetActive(false);
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class GameModeSelectUI : MonoBehaviour
    {
        private LobbyManager lobbyManager => ServiceProvider.Get<LobbyManager>();

        [SerializeField] private Image bg;
        [SerializeField] private Button single;
        [SerializeField] private Button online;

        private void Start()
        {
            bg.gameObject.SetActive(true);
            single.onClick.AddListener(() => { });
            online.onClick.AddListener(() => lobbyManager.ConnectToOnlineServer());
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
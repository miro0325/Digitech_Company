using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class GameModeSelectUI : UIStackWindow
    {
        [SerializeField] private Image bg;
        [SerializeField] private Button single;
        [SerializeField] private Button online;

        private void Start()
        {
            UIStack.Instance.Open<GameModeSelectUI>();
            
            single.onClick.AddListener(() => { });
            online.onClick.AddListener(() => 
            {
                LobbyManager.Instance.ConnectToOnlineServer().onComplete = () =>
                {
                    // UIStack.Instance.Open<>
                };
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
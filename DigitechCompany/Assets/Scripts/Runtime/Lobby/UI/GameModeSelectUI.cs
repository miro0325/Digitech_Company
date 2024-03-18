using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class GameModeSelectUI : MonoBehaviour
    {
        [SerializeField] private Image bg;
        [SerializeField] private Button single;
        [SerializeField] private Button online;

        private void Start()
        {
            LobbyManager.Instance
                .ObserveEveryValueChanged(x => x.IsConnectedLobby)
                .Subscribe(b => bg.gameObject.SetActive(b));
        }
    }
}
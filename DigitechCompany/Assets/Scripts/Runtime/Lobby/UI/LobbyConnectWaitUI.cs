using UniRx;
using UnityEngine;

namespace Game.Lobby
{
    public class LobbyConnectWaitUI : MonoBehaviour
    {
        [SerializeField] private GameObject bg;

        private void Start()
        {
            LobbyManager.Instance
                .ObserveEveryValueChanged(x => x.IsConnectedLobby)
                .Subscribe(b => bg.SetActive(!b));
        }
    }
}
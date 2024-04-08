using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class ConnectWaitUI : MonoBehaviour
    {
        private LobbyManager lobbyManager => ServiceProvider.Get<LobbyManager>();

        [SerializeField] private Image bg;
        [SerializeField] private TextMeshProUGUI text;

        private void Start()
        {
            lobbyManager
                .ObserveEveryValueChanged(l => l.ConnectingState)
                .Subscribe(x =>
                {
                    Debug.Log(x);
                    switch (x)
                    {
                        case ConnectingState.None:
                        case ConnectingState.InLobby:
                        case ConnectingState.InRoom:
                            bg.gameObject.SetActive(false);
                            break;
                        case ConnectingState.TryMaster:
                            bg.gameObject.SetActive(true);
                            text.text = $"connecting server...";
                            break;
                        case ConnectingState.TryRoom:
                            bg.gameObject.SetActive(true);
                            text.text = $"connecting room...";
                            break;
                        case ConnectingState.TryLobby:
                            bg.gameObject.SetActive(true);
                            text.text = $"connecting lobby...";
                            break;
                    }
                });
        }
    }
}
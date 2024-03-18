using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Lobby
{
    public class ConnectWaitUI : MonoBehaviour
    {
        [SerializeField] private Image bg;
        [SerializeField] private TextMeshProUGUI text;

        private void Start()
        {
            LobbyManager.Instance
                .ObserveEveryValueChanged(l => l.ConnectingState)
                .Subscribe(x =>
                {
                    bg.gameObject.SetActive(x == ConnectingState.TryMaster || x == ConnectingState.TryLobby);
                    if(x == ConnectingState.TryMaster)
                        text.text = $"connecting server...";
                    if(x == ConnectingState.TryLobby)
                        text.text = $"connecting lobby...";
                });
        }
    }
}
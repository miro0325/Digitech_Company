using UniRx;
using UnityEngine;

namespace Game.Lobby
{
    public class MasterConnectWaitUI : MonoBehaviour
    {
        [SerializeField] private GameObject bg;

        private void Start()
        {
            LobbyManager.Instance
                .ObserveEveryValueChanged(x => x.IsConnectedMaster)
                .Subscribe(b => bg.SetActive(!b));
        }
    }
}
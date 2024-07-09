using UniRx;
using UnityEngine;

public class SpectateUI : MonoBehaviour
{
    [SerializeField] private GameObject view;

    private void Start()
    {
        ServiceLocator
            .For(this)
            .Get<InGamePlayer>()
            .ObserveEveryValueChanged(p => p.IsDie)
            .Subscribe(active => view.SetActive(active));
    }
}
using UniRx;
using UnityEngine;

public class PlayUI : MonoBehaviour
{
    [SerializeField] private GameObject view;

    private void Start()
    {
        view.SetActive(false);

        ServiceLocator
            .For(this)
            .Get<GameManager>()
            .OnLoadComplete += () =>
            {
                ServiceLocator
                    .For(this)
                    .Get<InGamePlayer>()
                    .ObserveEveryValueChanged(p => p.IsDie)
                    .Subscribe(active => view.SetActive(!active));
            };
    }
}
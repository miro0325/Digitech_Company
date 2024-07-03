using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    private UserInput _input;
    private UserInput input => _input ??= UserInput.input;

    [SerializeField] private GameObject view;
    [SerializeField] private Button confirmButton;

    private ReactiveProperty<bool> isActive = new();

    private void Start()
    {
        ServiceLocator
            .For(this)
            .Get<GameManager>()
            .ObserveEveryValueChanged(g => g.State)
            .Where(state => state == GameState.DisplayResult)
            .Subscribe(_ => isActive.Value = true);
        
        isActive
            .Subscribe(b => view.SetActive(isActive.Value));

        confirmButton
            .OnClickAsObservable()
            .Subscribe(_ =>
            {
                isActive.Value = false;
                input.Player.Enable();
            });
    }

    private void Update()
    {
        if(isActive.Value)
            input.Player.Disable();
    }
}
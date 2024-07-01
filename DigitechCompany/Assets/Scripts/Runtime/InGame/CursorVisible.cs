using UniRx;
using UnityEngine;

public class CursorVisible : MonoBehaviour, IService
{
    private UserInput _input;
    private UserInput input => _input ??= UserInput.input;
    
    private void Awake()
    {
        ServiceLocator.ForGlobal().Register(this);
    }

    private void Start()
    {
        input
            .ObserveEveryValueChanged(i => i.Player.enabled)
            .Subscribe(enabled => 
            {
                Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
                Cursor.visible = !enabled;
            });
    }
}
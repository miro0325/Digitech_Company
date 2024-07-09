using UnityEngine;

public abstract class UIView : MonoBehaviour
{
    protected UIManager _uiManager;
    protected UIManager uiManager => _uiManager ??= ServiceLocator.For(this).Get<UIManager>();

    public abstract void Open();
    public abstract void Close();
}
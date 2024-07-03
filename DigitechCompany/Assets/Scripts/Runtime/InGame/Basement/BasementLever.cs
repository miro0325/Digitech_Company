using UnityEngine;
using DG.Tweening;

public class BasementLever : MonoBehaviour, IInteractable
{
    //service
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();
    
    private Basement _basement;
    private Basement basement => _basement ??= ServiceLocator.For(this).Get<Basement>();

    [SerializeField] private Transform gearStick;

    public string GetInteractionExplain(UnitBase unit)
    {
        if(gameManager.State == GameState.Load) return "·Îµù Áß";
        
        switch (basement.CurState)
        {
            case Basement.State.TakingOff: return "ÀÌ·ú Áß";
            case Basement.State.Up: return "Âø·úÇÏ±â";
            case Basement.State.Landing: return "Âø·ú Áß";
            case Basement.State.Down: return "ÀÌ·úÇÏ±â";
        }
        return "";
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 1;
    }

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public bool IsInteractable(UnitBase unit)
    {
        if(gameManager.State == GameState.Load) return false;
        
        switch (basement.CurState)
        {
            //moving
            case Basement.State.TakingOff:
            case Basement.State.Landing: return false;

            //stopping
            case Basement.State.Up:
            case Basement.State.Down: return true;
        }
        return false;
    }

    public void OnInteract(UnitBase unit)
    {
        if(gameManager.State == GameState.Load) return;
        
        switch (basement.CurState)
        {
            case Basement.State.TakingOff: break;
            case Basement.State.Up: gameManager.RequestStartGame(); break;
            case Basement.State.Landing: break;
            case Basement.State.Down: gameManager.RequestEndGame(); break;
        }

        #region Temp
        if (basement.CurState == Basement.State.Up)
        {
            gearStick.DOLocalRotate(new Vector3(25,0,0), 0.25f);
        }
        else if(basement.CurState == Basement.State.Down)
        {
            gearStick.DOLocalRotate(Vector3.zero, 0.25f);
        }
        #endregion
    }
}
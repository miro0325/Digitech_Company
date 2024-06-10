using UnityEngine;

public class TestBasementLever : MonoBehaviour, IInteractable
{
    //service
    private GameManager _gameManager;
    private GameManager gameManager => _gameManager ??= ServiceLocator.For(this).Get<GameManager>();
    
    private TestBasement _testBasement;
    private TestBasement testBasement => _testBasement ??= ServiceLocator.For(this).Get<TestBasement>();

    public string GetInteractionExplain(UnitBase unit)
    {
        if(gameManager.State == GameState.Loading) return "·Îµù Áß";
        
        switch (testBasement.State)
        {
            case TestBasementState.TakingOff: return "ÀÌ·ú Áß";
            case TestBasementState.Up: return "Âø·úÇÏ±â";
            case TestBasementState.Landing: return "Âø·ú Áß";
            case TestBasementState.Down: return "ÀÌ·úÇÏ±â";
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
        if(gameManager.State == GameState.Loading) return false;
        
        switch (testBasement.State)
        {
            //moving
            case TestBasementState.TakingOff:
            case TestBasementState.Landing: return false;

            //stopping
            case TestBasementState.Up:
            case TestBasementState.Down: return true;
        }
        return false;
    }

    public void OnInteract(UnitBase unit)
    {
        if(gameManager.State == GameState.Loading) return;
        
        switch (testBasement.State)
        {
            case TestBasementState.TakingOff: break;
            case TestBasementState.Up: gameManager.RequestStartGame(); break;
            case TestBasementState.Landing: break;
            case TestBasementState.Down: gameManager.RequestEndGame(); break;
        }
    }
}
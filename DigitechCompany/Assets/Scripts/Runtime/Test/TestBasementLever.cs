using UnityEngine;

public class TestBasementLever : MonoBehaviour, IInteractable
{
    //service
    private GameManager gameManager;
    private GameManager GameManager
    {
        get
        {
            if(ReferenceEquals(gameManager, null))
                gameManager = ServiceLocator.For(this).Get<GameManager>();
            return gameManager;
        }
    }
    
    private TestBasement testBasement;
    private TestBasement TestBasement
    {
        get
        {
            if(ReferenceEquals(testBasement, null))
                testBasement = ServiceLocator.For(this).Get<TestBasement>();
            return testBasement;
        }
    }

    public string GetInteractionExplain(UnitBase unit)
    {
        if(GameManager.State == GameState.Loading) return "·Îµù Áß";
        
        switch (TestBasement.State)
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
        if(GameManager.State == GameState.Loading) return false;
        
        switch (TestBasement.State)
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
        if(GameManager.State == GameState.Loading) return;
        
        switch (TestBasement.State)
        {
            case TestBasementState.TakingOff: break;
            case TestBasementState.Up: GameManager.RequestStartGame(); break;
            case TestBasementState.Landing: break;
            case TestBasementState.Down: GameManager.RequestEndGame(); break;
        }
    }
}
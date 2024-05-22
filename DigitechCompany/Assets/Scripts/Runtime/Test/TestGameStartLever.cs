using UnityEngine;

public class TestGameStartLever : MonoBehaviour, IInteractable
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

    public string GetInteractionExplain(UnitBase unit)
    {
        return GameManager.GameState == GameState.Waiting ? "Ãâ¹ß" : "Âø·ú Áß";
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
        return GameManager.GameState == GameState.Waiting;
    }

    public void OnInteract(UnitBase unit)
    {
        GameManager.RequestStartGame();
    }
}
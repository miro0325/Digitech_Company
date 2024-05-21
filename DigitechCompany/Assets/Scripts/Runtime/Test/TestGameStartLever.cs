using UnityEngine;

public class TestGameStartLever : MonoBehaviour, IInteractable
{
    //service
    private GameManager gameManager;

    public string GetInteractionExplain(UnitBase unit)
    {
        return gameManager.GameState == GameState.Waiting ? "Ãâ¹ß" : "Âø·ú Áß";
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
        return gameManager.GameState == GameState.Waiting;
    }

    public void OnInteract(UnitBase unit)
    {
        gameManager.RequestStartGame();
    }

    private void Start()
    {
        gameManager = ServiceLocator.For(this).Get<GameManager>();
    }
}
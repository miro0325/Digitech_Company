using UnityEngine;

public class TestGameStartLever : MonoBehaviour, IInteractable
{
    //service
    private GameManager gameManager;

    public string GetInteractionExplain(UnitBase unit)
    {
        return "√‚πﬂ";
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
        return gameManager.IsGameWaiting;
    }

    public void OnInteract(UnitBase unit)
    {
        gameManager.StartGame();
    }

    private void Start()
    {
        gameManager = ServiceLocator.For(this).Get<GameManager>();
    }
}
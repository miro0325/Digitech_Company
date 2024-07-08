using UnityEngine;

public class CaptureTrapExitTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private CaptureTrap trap;

    public InteractID GetTargetInteractID(UnitBase unit)
    {
        return InteractID.ID1;
    }

    public float GetInteractRequireTime(UnitBase unit)
    {
        return 1;
    }

    public bool IsInteractable(UnitBase unit)
    {
        return unit.photonView.ViewID == trap.CapturedPlayerViewId;
    }

    public string GetInteractionExplain(UnitBase unit)
    {
        return unit.photonView.ViewID == trap.CapturedPlayerViewId ? "≈ª√‚ Ω√µµ" : "";
    }

    public void OnInteract(UnitBase unit)
    {
        trap.TryExit();
    }
}
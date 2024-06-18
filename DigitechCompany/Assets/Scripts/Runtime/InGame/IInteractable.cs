public interface IInteractable
{
    public InteractID GetTargetInteractID(UnitBase unit);
    public float GetInteractRequireTime(UnitBase unit);
    public bool IsInteractable(UnitBase unit);
    public string GetInteractionExplain(UnitBase unit);
    public void OnInteract(UnitBase unit);
}
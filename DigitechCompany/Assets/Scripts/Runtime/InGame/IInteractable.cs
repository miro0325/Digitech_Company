public interface IInteractable
{
    public InteractID TargetInteractID { get; }

    public bool IsInteractable(UnitBase unit);
    public string GetInteractionExplain(UnitBase unit);
    public void OnInteract(UnitBase unit);
}
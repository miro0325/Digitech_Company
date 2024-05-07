public interface IInteractable
{
    public bool IsInteractable { get; }
    public InteractID TargetInteractID { get; }

    public string GetInteractionExplain(UnitBase unit);
    public void OnInteract(UnitBase unit);
}
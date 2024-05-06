public interface IInteractable
{
    public string InteractionExplain { get; }

    public void OnInteract(UnitBase unit);
}
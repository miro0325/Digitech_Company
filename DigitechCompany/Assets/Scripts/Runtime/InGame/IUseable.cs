using UnityEngine;

public interface IUseable
{
    public bool IsUsable(InteractID id);
    public string GetUseExplain(InteractID id, UnitBase unit);
    public void OnUsePressed(InteractID id);
    public void OnUseReleased();
}
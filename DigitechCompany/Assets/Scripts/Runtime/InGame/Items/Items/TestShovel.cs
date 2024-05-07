using UnityEngine;

public class TestShovel : ItemBase, IInteractable
{
    //static
    private static int Animator_IdleHash = Animator.StringToHash("Idle");
    private static int Animator_UseHash = Animator.StringToHash("Use");

    //field
    private float delayTime;

    //function
    public override bool IsUseable(InteractID id)
    {
        if(id == InteractID.ID2) return delayTime <= 0;
        return false;
    }

    public override string GetUseExplain(InteractID id, UnitBase unit)
    {
        if(id == InteractID.ID2) return "때리기";
        return "";
    }
    
    private void Update()
    {
        if(delayTime > 0)
            delayTime -= Time.deltaTime;
    }

    public override void OnInteract(UnitBase unit)
    {
        base.OnInteract(unit);
        delayTime = 1f;
        animator.SetTrigger(Animator_IdleHash);
    }

    public override void OnUse(InteractID id)
    {
        animator.SetTrigger(Animator_UseHash);
    }
}
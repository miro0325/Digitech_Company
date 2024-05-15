using UnityEngine;

public class AttackableItem : ItemBase, IInteractable
{
    //static
    private static int Animator_IdleHash = Animator.StringToHash("Idle");
    private static int Animator_AttackHash = Animator.StringToHash("Attack");

    //field
    private float delayTime;

    //function
    public override bool IsUsable(InteractID id)
    {
        if(id == InteractID.ID2) return delayTime <= 0;
        return false;
    }

    public override string GetUseExplain(InteractID id, UnitBase unit)
    {
        if(id == InteractID.ID2) return "때리기";
        return "";
    }
    
    protected override void Update()
    {
        base.Update();
        if(delayTime > 0)
            delayTime -= Time.deltaTime;
    }

    public override void OnInteract(UnitBase unit)
    {
        base.OnInteract(unit);
        animator.SetTrigger(Animator_IdleHash);
    }

    public override void OnUse(InteractID id)
    {
        if(id != InteractID.ID2) return;
        delayTime = 0.75f;
        animator.SetTrigger(Animator_AttackHash);
    }
}
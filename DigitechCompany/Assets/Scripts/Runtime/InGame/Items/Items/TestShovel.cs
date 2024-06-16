using UnityEngine;

public class TestShovel : ItemBase, IInteractable
{
    //static
    private static int Animator_IdleHash = Animator.StringToHash("Idle");
    private static int Animator_UseHash = Animator.StringToHash("Use");

    //field
    private float delayTime;
    private Animator animator;

    //function
    public override void OnCreate()
    {
        base.OnCreate();
        animator = GetComponent<Animator>();
    }
    
    public override bool IsUsable(InteractID id)
    {
        if(id == InteractID.ID2) return delayTime <= 0;
        return false;
    }

    public override string GetUseExplain(InteractID id, UnitBase unit)
    {
        if(id == InteractID.ID2) return "?•Œë¦¬ê¸°";
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

    public override void OnUsePressed(InteractID id)
    {
        if(id != InteractID.ID2) return;
        delayTime = 0.75f;
        animator.SetTrigger(Animator_UseHash);
    }
}
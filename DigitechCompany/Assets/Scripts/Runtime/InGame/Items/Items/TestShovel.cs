using UnityEngine;

public class TestShovel : ItemBase, IInteractable
{
    //static
    private static int Animator_IdleHash = Animator.StringToHash("Idle");
    private static int Animator_UseHash = Animator.StringToHash("Use");

    public override void OnInteract(UnitBase unit)
    {
        base.OnInteract(unit);
        animator.SetTrigger(Animator_IdleHash);
    }

    private void Update()
    {
        if(!InHand) return;
        if(Input.GetMouseButtonDown(0))
        {
            OnUse();
        }
    }

    public override void OnUse()
    {
        animator.SetTrigger(Animator_UseHash);
    }
}
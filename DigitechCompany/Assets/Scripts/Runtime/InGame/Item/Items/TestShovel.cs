using UnityEngine;

public class TestShovel : ItemBase, IInteractable
{
    private static int Animator_InteractHash = Animator.StringToHash("Interact");

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public override void OnInteract(UnitBase unit)
    {
        
    }

    public override void OnUse()
    {
        animator.SetTrigger(Animator_InteractHash);
    }
}
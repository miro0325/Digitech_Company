using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;


public abstract class ItemBase : MonoBehaviour,IInteractable
{
    public ItemData Data => data;
    public bool IsInHand => isInHand;
    public Transform[] ikHandPoints;

    private ItemData data;

    protected bool isInHand = false;
    protected Rigidbody rb;
    protected Animator animator;

    public abstract void OnInteract(TempPlayer temp);

    public virtual void OnGet(TempPlayer temp)
    {
        rb.isKinematic = true;
        isInHand = true;
    }

    public virtual void OnDrop()
    {
        rb.isKinematic = false;
        isInHand = false;
    }

    public void Init(ItemData data)
    {
        this.data = data;
       
    }

    protected virtual void Awake()
    {
        if (TryGetComponent(out Rigidbody _rb))
        {
            rb = _rb;
        }
        else
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        if (TryGetComponent(out Animator _anim))
        {
            animator = _anim;
        }
        else
        {
            animator = gameObject.AddComponent<Animator>();
        }
    }

}

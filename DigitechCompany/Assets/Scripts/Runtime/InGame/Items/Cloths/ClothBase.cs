using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClothBase : ItemBase
{
    public enum WearPoint
    {
        Head,Chest,Legs,Feet
    }

    public WearPoint wearPoint;
    
    public override void OnDrop()
    {

    }

    public override void OnGet(TempPlayer temp)
    {

    }

    public override void OnInteract(TempPlayer temp)
    {

    }

    public virtual void Effect()
    {

    }

    private void Awake()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

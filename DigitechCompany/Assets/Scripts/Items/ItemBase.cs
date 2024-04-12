using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public abstract class ItemBase : MonoBehaviour,IInteractable
{
    public string Name => itemName;
    public int SellPrice => sellPrice;
    public int BuyPrice => buyPrice;
    public bool IsOnlySell => isOnlySell;
    
    [SerializeField] private string itemName;
    [SerializeField] private int sellPrice = 0;
    [SerializeField] private int buyPrice = 0;
    [SerializeField] private bool isOnlySell = false;

    public abstract void OnInteract();

    public abstract void OnGet();

    public abstract void OnDrop();
}

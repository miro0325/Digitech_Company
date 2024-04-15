using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;


public abstract class ItemBase : MonoBehaviour,IInteractable
{
    public int Id => id;
    public string Name => itemName;
    public float Weight => weight;
    public int SellPrice => sellPrice;
    public int BuyPrice => buyPrice;
    public bool IsOnlySell => isOnlySell;
    public bool IsInteractable => isInteractable;
    public bool IsBothHand => isBothHand;
    public ItemType Type => type;

    [SerializeField] protected int id;
    [SerializeField] protected string itemName;
    [SerializeField] protected float weight;
    [SerializeField] protected int[] sellPrices = new int[2];
    [SerializeField] protected int buyPrice = 0;
    [SerializeField] protected bool isOnlySell = false;
    [SerializeField] protected bool isInteractable;
    [SerializeField] protected bool isBothHand = false;
    [SerializeField] protected ItemType type;

    private int sellPrice;

    public abstract void OnInteract();

    public abstract void OnGet();

    public abstract void OnDrop();

    public void Init(ItemData data)
    {
        id = data.id;
        itemName = data.name;
        weight = data.weight;
        isInteractable = data.isInteractable;
        isBothHand = data.isBothHand;
        type = data.type;
        if(data.isOnlySell)
        {
            sellPrices[0] = data.prices[0];
            sellPrices[1] = data.prices[1];
            buyPrice = 0;
        } else
        {
            buyPrice = Random.Range(data.prices[0],data.prices[1]);
            sellPrice = 0;
        }
        
    }

}

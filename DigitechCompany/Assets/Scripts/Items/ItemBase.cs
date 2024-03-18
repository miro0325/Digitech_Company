using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    public string Name => itemName;
    public int SellPrice => sellPrice;
    public int BuyPrice => buyPrice;
    
    [SerializeField] private string itemName;
    [SerializeField] private int sellPrice = 0;
    [SerializeField] private int buyPrice = 0;
    
    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}

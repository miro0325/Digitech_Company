using Photon.Pun;
using UnityEngine;

public abstract class UnitBase : NetworkObject
{
    [Header("BaseUnit")]
    [SerializeField] protected Transform itemHolder;

    protected Stats maxStats = new();
    protected Stats curStats = new();
    protected Stats.Modifier modifier = new();
    protected ItemContainer itemContainer;
    
    public Transform ItemHolder => itemHolder;
    public Stats MaxStats => maxStats;
    public Stats CurStats => curStats;
    public Stats.Modifier Modifier => modifier;
    public ItemContainer ItemContainer => itemContainer;

    public abstract Stats BaseStats { get; }

    public override void OnCreate()
    {
        if(!photonView.IsMine) return;
        
        modifier.OnValueChange += key => modifier.Calculate(key, maxStats, BaseStats);
        maxStats.OnStatChanged += (key, old, cur) =>
        {
            var diff = cur - old;
            curStats.SetStat(key, x => x + diff);
        };
    }
}
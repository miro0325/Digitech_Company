using Photon.Pun;
using UnityEngine;

public abstract class UnitBase : NetworkObject
{
    protected Stats maxStats = new();
    protected Stats curStats = new();
    protected Stats.Modifier modifier = new();
    
    public Stats MaxStats => maxStats;
    public Stats CurStats => curStats;
    public Stats.Modifier Modifier => modifier;
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
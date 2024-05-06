using Photon.Pun;
using UnityEngine;

public abstract class UnitBase : NetworkObject
{
    public Stats maxStats = new();
    public Stats curStats = new();
    public Stats.Modifier modifier = new();
    
    public abstract Stats BaseStats { get; }

    public override void OnCreate()
    {
        if(!photonView.IsMine) return;
        
        modifier.OnValueChange += key => modifier.Calculate(key, maxStats, BaseStats);
        maxStats.OnStatChanged += (key, old, cur) =>
        {
            var diff = cur - old;
            curStats.ModifyStat(key, x => x + diff);
        };
    }
}
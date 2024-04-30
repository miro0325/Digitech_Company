using Photon.Pun;
using UnityEngine;

public abstract class UnitBase : MonoBehaviourPun
{
    public Stats maxStats = new();
    public Stats curStats = new();
    public Stats.Modifier modifier = new();
    
    public abstract Stats BaseStats { get; }

    protected virtual void Awake()
    {
        if(!photonView.IsMine) return;
        modifier.OnValueChange += key => modifier.Calculate(key, maxStats, BaseStats);
    }
}
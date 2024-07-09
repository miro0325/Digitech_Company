using Photon.Pun;
using UnityEngine;

public abstract class UnitBase : NetworkObject,IDamagable
{
    [Header("BaseUnit")]
    [SerializeField] protected Transform itemHolder;

    protected Stats maxStats = new();
    protected Stats curStats = new();
    protected Stats.Modifier modifier = new();
    protected Inventory inventory;
    
    public Transform ItemHolder => itemHolder;
    public Stats MaxStats => maxStats;
    public Stats CurStats => curStats;
    public Stats.Modifier Modifier => modifier;
    public Inventory Inventory => inventory;

    public abstract Stats BaseStats { get; }

    public GameObject OwnObject => this.gameObject;

    public override void OnCreate()
    {
        base.OnCreate();

        if(!photonView.IsMine) return;
        
        modifier.OnValueChange += key => modifier.Calculate(key, maxStats, BaseStats);
        maxStats.OnStatChanged += (key, old, cur) =>
        {
            var diff = cur - old;
            curStats.SetStat(key, x => x + diff);
        };
    }
    
    public virtual void Damage(float damage, UnitBase attacker)
    {
        photonView.RPC(nameof(SendDamageToOwnerRpc), photonView.Owner, damage);
    }

    [PunRPC]
    protected void SendDamageToOwnerRpc(float damage)
    {
        if (curStats.GetStat(Stats.Key.Hp) <= 0) return;
        curStats.SetStat(Stats.Key.Hp, x => x - damage);
        if (curStats.GetStat(Stats.Key.Hp) <= 0)
        {
            Death();
        }
    }

    protected virtual void Death()
    {

    } 
}
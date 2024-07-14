using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    private List<GameObject> hitPlayers = new();

    private InGamePlayer targetPlayer;
    private UnitBase ownUnit;
    private float damage;
    private bool isAttacking = false;

    public void Initialize(UnitBase ownUnit)
    {
        this.ownUnit = ownUnit;
    }

    public void StartAttack(float damage, InGamePlayer targetPlayer = null)
    {
        this.damage = damage;
        this.targetPlayer = targetPlayer;
        hitPlayers.Clear();
        GetComponent<Collider>().enabled = true;
        isAttacking = true;
    }

    public void EndAttack()
    {
        GetComponent<Collider>().enabled = true;
        isAttacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //if (isAttacking && other.gameObject.layer == LayerMask.GetMask("Player"))
        //{
        //    if(targetPlayer != null)
        //    {
        //        if(targetPlayer.gameObject == other.gameObject)
        //        {
        //            if (hitPlayers.Contains(other.gameObject)) return;
        //            targetPlayer.Damage(damage,ownUnit);
        //            hitPlayers.Add(targetPlayer.gameObject);
        //        }
        //    } else
        //    {
        //        if (hitPlayers.Contains(other.gameObject)) return;
        //        hitPlayers.Add(other.gameObject);
        //        var player = other.GetComponent<InGamePlayer>();
        //        player.Damage(damage,ownUnit);
        //    }
        //}
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Monster") || other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if(other.TryGetComponent(out UnitBase unit))
            {
                if(unit.IsDie)
                unit.Damage(999, unit);
            }
        }
    }
}

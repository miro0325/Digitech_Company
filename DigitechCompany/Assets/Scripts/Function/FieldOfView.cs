using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public List<UnitBase> TargetUnits => targetUnits;
    public List<ItemBase> TargetItems => targetItems;

    public float ViewRadius => viewRadius;
    public float ViewRange => viewRange;

    [SerializeField]
    private float viewRadius = 10f;
    [SerializeField]
    private float viewRange = 120f;
    [SerializeField]
    private float viewCooldown = 0.2f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    private List<UnitBase> targetUnits = new List<UnitBase>();
    private List<ItemBase> targetItems = new List<ItemBase>();

    private WaitForSeconds wait;

    private void Start()
    {
        wait = new WaitForSeconds(viewCooldown);
        StartCoroutine(LoopFindVisibleTargets());
    }

    private IEnumerator LoopFindVisibleTargets()
    {
        while(true)
        {
            yield return wait;
            FindVisibleTargets();
        }
    }

    private void FindVisibleTargets()
    {
        
        targetItems.Clear();
        targetUnits.Clear();
        Collider[] hits = Physics.OverlapSphere(transform.position,viewRadius,targetMask);
        for(int i = 0; i < hits.Length; i++)
        {
            Transform target = hits[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            
            if(Vector3.Angle(transform.forward, dirToTarget) < viewRange)
            {
                float distToTarget = Vector3.Distance(transform.position, target.position);
                if(!Physics.Raycast(transform.position,dirToTarget,distToTarget,obstacleMask))
                {
                    if(target.TryGetComponent(out UnitBase unit))
                    {
                        targetUnits.Add(unit);
                    }
                    if(target.TryGetComponent(out ItemBase item))
                    {
                        targetItems.Add(item);
                    }
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angle, bool isGlobal = false)
    {
        if(!isGlobal)
        {
            angle += transform.transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad));
    }

}

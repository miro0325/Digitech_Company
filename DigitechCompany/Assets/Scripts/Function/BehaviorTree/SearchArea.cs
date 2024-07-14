
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorTree;
using System.Linq;

public class SearchArea : Node
{
    private GameObject[] searchObjs; 

    private Vector3 position;
    private float range;
    private LayerMask layerMask;
    private GameObject targetObj;

    public SearchArea(Vector3 position, float range, LayerMask layerMask, List<Node> children) : base(children)
    {
        this.position = position;
        this.range = range;
        this.layerMask = layerMask;
    }

    public SearchArea(Vector3 position, float range, GameObject targetObj, List<Node> children) : base(children)
    {
        this.position = position;
        this.range = range;
        this.targetObj = targetObj;
    }

    public override NodeState Evaluate()
    {
        Collider[] hits;
        if(layerMask.value != 0)
        {
            hits = Physics.OverlapSphere(position, range, layerMask);
            searchObjs = hits.Select(x => x.gameObject).ToArray();
            SetData("Search_Objects", searchObjs);
            return NodeState.Succes;
        }
        else
        {
            if(targetObj != null)
            {
                hits = Physics.OverlapSphere(position, range);
                searchObjs = hits.Where(x => x.gameObject == targetObj).Select(x => x.gameObject).ToArray();
                SetData("Search_Objects",searchObjs);
                return NodeState.Succes;
            }
            
        }
        
        return NodeState.Failure;
    }
}

using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SawTrap : MonoBehaviour
{
    [SerializeField] private Vector3[] points;

    private int curPointIndex;

    private void Start()
    {
        
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        
    }
}

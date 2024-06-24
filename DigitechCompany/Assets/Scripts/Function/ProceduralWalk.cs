using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ProceduralWalk : MonoBehaviour
{
    [SerializeField]
    private Transform body;
    [SerializeField]
    private Transform target;
    [SerializeField]
    private float footSpacing = 1;

    [SerializeField]
    private float stepDistance = 1.5f;
    [SerializeField]
    private float stepHeight = 0.2f;
    [SerializeField]
    private float stepSpeed = 3f;

    [SerializeField]
    private LayerMask isGround;
    [SerializeField]
    private Color debugColor;

    private Vector3 oldPosition;
    private Vector3 curPosition;
    private Vector3 newPosition;
    private float lerp;

    
    private void Start()
    {
       
        oldPosition = transform.position;
        newPosition = transform.position;
        curPosition = transform.position;
        target.position += body.forward * footSpacing;
        lerp = 0;
    }

    private void Update()
    {
        UpdateFootPosition();
    }

    private void UpdateFootPosition()
    {

        transform.position = curPosition;
        Vector3 checkPos = body.position - transform.position;
        Ray ray = new Ray(target.position+body.up, -body.up);
        Debug.DrawRay(target.position+body.up, -body.up, Color.red);
        if (Physics.Raycast(ray, out RaycastHit hit, 2, isGround))
        {
            //Debug.Log(Vector3.Distance(newPosition, hit.point));
            if (Vector3.Distance(newPosition, hit.point) > stepDistance)
            {
                lerp = 0;
                newPosition = hit.point;
            }
        }
        if (lerp < 1)
        {
            Vector3 footPos = Vector3.Lerp(oldPosition, newPosition, lerp);
            footPos.y += Mathf.Sin(lerp * Mathf.PI) * stepHeight;

            curPosition = footPos;
            lerp += Time.deltaTime * stepSpeed;
        }
        else
        {
            oldPosition = newPosition;
        }

    }
    
}

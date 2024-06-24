using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FieldOfView))]
public class FieldOfViewGUI : Editor
{
    private void OnSceneGUI()
    {
        FieldOfView fov = (FieldOfView)target;
        Handles.color = Color.white;
        Handles.DrawWireArc(fov.transform.position, Vector3.up, Vector3.forward, 360, fov.ViewRadius);
        Vector3 viewAngleA = fov.DirFromAngle(-fov.ViewRange / 2);
        Vector3 viewAngleB = fov.DirFromAngle(fov.ViewRange / 2);

        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleA * fov.ViewRadius);
        Handles.DrawLine(fov.transform.position, fov.transform.position + viewAngleB * fov.ViewRadius);

        Handles.color = Color.red;
        foreach (var visible in fov.TargetUnits)
        {
            Handles.DrawLine(fov.transform.position, visible.transform.position);
        }
    }

    
}



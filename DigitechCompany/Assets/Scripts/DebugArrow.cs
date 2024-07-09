using UnityEngine;
using System.Collections;

public static class DebugArrow
{
    public static void DrawGizmos(Vector3 start, Vector3 end)
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(start, end);

        var dir = (end - start).normalized;
        var right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + 30, 0) * new Vector3(0, 0, 1);
        var left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - 30, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawLine(end, right);
        Gizmos.DrawLine(end, left);
    }

    public static void DrawGizmos(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawLine(start, end);

        var dir = (end - start).normalized;
        var right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 + 30, 0) * new Vector3(0, 0, 1);
        var left = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 180 - 30, 0) * new Vector3(0, 0, 1);
        var up = Quaternion.LookRotation(dir) * Quaternion.Euler(180 + 30, 0, 0) * new Vector3(0, 0, 1);
        var down = Quaternion.LookRotation(dir) * Quaternion.Euler(180 - 30, 0, 0) * new Vector3(0, 0, 1);
        Gizmos.DrawLine(end, end + right);
        Gizmos.DrawLine(end, end + left);
        Gizmos.DrawLine(end, end + up);
        Gizmos.DrawLine(end, end + down);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InstantiateDoorPrefab : EditorWindow
{
    [MenuItem("Tools/Open")]
    public static void Open()
    {
        EditorWindow.GetWindow<InstantiateDoorPrefab>();
    }

    private GameObject gameObject;

    private void OnGUI()
    {
        gameObject = (GameObject)EditorGUILayout.ObjectField("prefab", gameObject, typeof(GameObject), false);
        if(GUILayout.Button("Instantiate"))
        {
            var selections = Selection.gameObjects;
            foreach(var go in selections)
            {
                var prefab = (GameObject)PrefabUtility.InstantiatePrefab(gameObject);
                prefab.transform.position = go.transform.position;
                prefab.transform.rotation = go.transform.rotation;
                prefab.transform.localScale = go.transform.lossyScale;
            }
        }
    }
}

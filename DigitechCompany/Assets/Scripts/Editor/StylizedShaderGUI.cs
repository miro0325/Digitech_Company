using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Rendering.Universal.ShaderGUI;

public class StylizedShaderGUI : BaseShaderGUI
{
    //StylizedLitGUI.LitProperties litProperties = StylizedLitGUI.LitProperties.
    
    public override void DrawAdvancedOptions(Material material)
    {
        EditorGUILayout.Space();
        DrawStylizedInputs(material);
        EditorGUILayout.Space();

        EditorGUILayout.HelpBox("Other Option",MessageType.None);
        //if(litProperties)
        base.DrawAdvancedOptions(material);
    }

    public void DrawStylizedInputs(Material material)
    {
        //if (LitProperties)
        //}
    }
}

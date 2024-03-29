using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[ExecuteAlways]
public class ToonHandler : MonoBehaviour
{
    [SerializeField] Light customLight;
    [SerializeField] MeshRenderer[] renderers;

    private readonly string SHADER_NAME = "Unlit/ToonShader";

    private Shader shader;

    private Material[] materials;

    private void Init()
    {
        shader = Shader.Find(SHADER_NAME);
        Debug.Log(shader);
        int count = 0;
        materials = new Material[renderers.Length];
        foreach(var renderer in renderers)
        {
            materials[count] = renderer.material;
            count++;
        }
    }

    private void Start()
    {
        Init();
    }

    private void Update()
    {
        UpdateShaderVariables();
    }

    private void UpdateShaderVariables()
    {
       foreach(var mat in materials)
       {
            if (!mat.shader.Equals(shader)) continue;
            mat.SetVector("_LightPos",customLight.transform.position);
            mat.SetColor("_LightColor", customLight.color);
            mat.SetFloat("_LightStrength",customLight.intensity);
            mat.SetVector("_LightDir", customLight.transform.forward);
       }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        
        Debug.DrawRay(customLight.transform.position, customLight.transform.forward);
    }

}

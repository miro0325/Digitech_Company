using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laptop : ItemBase
{
    private readonly int controlAnim = Animator.StringToHash("ControlLaptop");
    private readonly int textureID = Shader.PropertyToID("_MainTex");

    [SerializeField]
    private MeshRenderer[] renderers;

    [SerializeField]
    private Texture2D[] textures;

    private bool isOpen = false;
    private bool isPlaying = false;
    
    protected override void Update()
    {
        
    }

    public override void OnUse(InteractID id)
    {
        if (isPlaying) return;
        base.OnUse(id);
        isOpen = !isOpen;
        isPlaying = true;
        animator.SetBool(controlAnim, isOpen);
    }

    public void ControlLight()
    {
        var tex = textures[(isOpen) ? 0 : 1];
        foreach(var renderer in renderers)
        {
            renderer.material.SetTexture(textureID, tex);
        }
    }

    public void EndAnimation()
    {
        isPlaying = false;
    }
}

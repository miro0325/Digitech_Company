using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Pixelate_RenderFeature : ScriptableRendererFeature
{
    private Pixelate_RenderPass _renderPass = null;

    public override void Create()
    {
        _renderPass = new Pixelate_RenderPass("PixelatePass", RenderPassEvent.AfterRenderingPostProcessing);
        
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        _renderPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(_renderPass);
    }
}

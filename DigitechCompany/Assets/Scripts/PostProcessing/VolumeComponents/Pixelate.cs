using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable, VolumeComponentMenuForRenderPipeline("Custom Post-processing/Pixelate", typeof(UniversalRenderPipeline))]
public class Pixelate : VolumeComponent, IPostProcessComponent
{
    
    //Shader 정보 작성
    private const string SHADER_NAME = "Custom/PixelateShader";
    // 쉐이더 파라미터
    private const string RESOLUTION = "_Pixels";
    private const string PIXEL_WIDTH = "_PixelWidth";
    private const string PIXEL_HEIGHT = "_PixelHeight";

    private Material _material;
    /// <summary>
    /// 활성화 여부
    /// </summary>
    public BoolParameter IsEnable = new BoolParameter(false);
    public ClampedIntParameter resolution = new ClampedIntParameter(512,0,4096);
    public ClampedFloatParameter pixelWidth = new ClampedFloatParameter(16,0,512);
    public ClampedFloatParameter pixelHeight = new ClampedFloatParameter(16,0,512);
    

    public bool IsActive()
    {
        if(IsEnable.value == false) return false;
        if (!active || !_material || IsOverValue()) return false;
        return true;

    }

    public bool IsTileCompatible() => false;
    
    private bool IsOverValue()
    {
        if (resolution.value == 0) return true;
        if(pixelHeight.value == 0 && pixelWidth.value == 0) return true;
        return false;
    }

    public void Setup()
    {
        if (!_material)
        {
            Shader shader = Shader.Find(SHADER_NAME);
            _material = CoreUtils.CreateEngineMaterial(shader);
            Debug.Log(_material.shader.ToString());
        }
    }

    public void Destroy()
    {
        if (_material)
        {
            CoreUtils.Destroy(_material);
            _material = null;
        }
        
    }

    public void Render(CommandBuffer commandBuffer, ref RenderingData renderingData, RenderTargetIdentifier source, RenderTargetIdentifier destination)
    {
        if (!_material) return;
        _material.SetInt(RESOLUTION, resolution.value);
        _material.SetFloat(PIXEL_WIDTH, pixelWidth.value);
        _material.SetFloat(PIXEL_HEIGHT, pixelHeight.value);
        //Debug.Log(_material.GetFloat(PIXEL_WIDTH));
        commandBuffer.Blit(source, destination, _material);
    }
}

Shader "Custom/PixelateShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Pixels ("Resolution",int) = 512
        _PixelWidth ("Pixel Width", float) = 64
        _PixelHeight ("Pixel Height", float) = 64
    }
    SubShader
    {
        
        Tags {
            
            "RenderPipeline" = "UniversalPipeline"
            
        }
        
        Cull Off 
        ZWrite Off ZTest Always
        LOD 100
        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            CBUFFER_START(UnityPerMatial)
				float4 _MainTex_ST;
            CBUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                
                
                return o;
            }

            //sampler2D _MainTex;
            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            //TEXTURE2D (_MainTex);
            //SAMPLER(sampler_MainTex);

            float _Pixels;
            float _PixelWidth;
            float _PixelHeight;
            float _dx;
            float _dy;
           
            half4 frag (v2f i) : SV_Target
            {
                _dx = _PixelWidth * (1/_Pixels);
                _dy = _PixelHeight * (1/_Pixels);
                float2 coord = float2(_dx * floor(i.uv.x / _dx), _dy * floor(i.uv.y / _dy));
                half4 col = _MainTex.Sample(sampler_MainTex, coord);
                //col.rgb = col.gbr;
                //half4 col = SAMPLER_TEXTURE2D(_MainTex, sampler_MainTex, coord);
                return col;
            }
            ENDHLSL
        }
    }
}

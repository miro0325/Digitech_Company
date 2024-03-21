Shader "Custom/CartoonShader"
{
    Properties
    {
        _MainTex("Albedo",2D) = "white" {}
        _BumpMap("Normal Map",2D) = "bump" {}
        _RampTex("Ramp Texture",2D) = "white" {}
        _EmissionMap("Emission Map",2D) = "white" {}
        _Outline_Bold ("Outline Bold", float) = 0
        _Cel ("Cel Shadow", float) = 3
        [Toggle] _EnableCustomLightDir("Enable Custom Light Dir",float) = 0
        _Light1Dir ("Light 1 Direction", Vector) = (0,0,0)
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "UniversalMaterialType" = "Lit"
            "Queue"="Geometry"
        }
        LOD 200
        Cull front
        Pass
        {
            Tags {"LightMode"="Outline"}
            HLSLPROGRAM
            #pragma vertex vert noshadow
            #pragma fragment frag noshadow

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            
            float _Outline_Bold;

            v2f vert (appdata v)
            {
                v2f o;
                float3 n = normalize(v.normal);
                float3 Outline_Pos = v.vertex + n * (_Outline_Bold * 0.1f);
                o.vertex = TransformObjectToHClip(Outline_Pos);
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return 0.0f;
            }
            ENDHLSL
        }
        LOD 200
        Cull back
        Pass 
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            //#pragma target 4.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata 
            {
                float4 position : POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_BumpMap : TEXCOORD0;
                float2 uv_RampTex : TEXCOORD0;
                float2 uv_EmissionMap : TEXCOORD0;
                float3 Normal : NORMAL;
            };

            struct v2f 
            {
                float4 position : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_BumpMap : TEXCOORD1;
                float2 uv_RampTex : TEXCOORD2;
                float2 uv_EmissionMap : TEXCOORD3;
                float3 Normal : NORMAL;

            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            float3 _Light1Dir;
            float _Cel;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                float4 _RampTex_ST;
                float4 _EmissionMap_ST;
            CBUFFER_END

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            v2f vert (appdata v) //여기가 버텍스 쉐이더. 매개변수 v를 갖고, o값을 반환
            {
                v2f o;
                o.position = TransformObjectToHClip(v.position.xyz);
                o.Normal = TransformObjectToWorld(v.Normal);
                o.uv_MainTex = v.uv_MainTex;
                o.uv_BumpMap = v.uv_BumpMap;
                o.uv_RampTex = v.uv_RampTex;
                o.uv_EmissionMap = v.uv_EmissionMap;
                // = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, TRANSFORM_TEX(v.uv_BumpMap, _BumpMap)));
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(i.uv_MainTex, _MainTex));
                //float3 Normal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, TRANSFORM_TEX(i.uv_BumpMap, _BumpMap)));
                float Ndotl = saturate(dot(i.Normal,normalize(_WorldSpaceCameraPos.xyz - TransformObjectToWorld(_Light1Dir.xyz)))) * 0.5 + 0.5;
                float Toon = floor(Ndotl * _Cel) * (1/_Cel);
                float4 Ramp = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(Ndotl,0.5));

                col *= Toon;
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Diffuse"
}

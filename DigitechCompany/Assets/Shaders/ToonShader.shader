Shader "Unlit/ToonShader"
{
    Properties
    {
        [Header(Textures),Space(3)]
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Main Tex Color", Color) = (1,1,1,1)
        [HDR]_EmissionColor("Emission Color",Color) = (1,1,1,1)
        [Toggle]_UseBumpMap("Enable Normal Map", Range(0,1)) = 0 
        [Toggle]_UseSpecular("Enable Specular", Range(0,1)) = 0 
        [Toggle]_UseMainLight("Enable MainLight", float) = 0
        _BumpMap("NormalMap", 2D) = "bump" {}
        _EmissionMap("Emission Map",2D) = "white" {}
        _LUT_Tex("LUT", 2D) = "white" {}
        _Outline_Bold ("Outline Bold", float) = 0
        _Cel ("Cel", Range(1,10)) = 1

        _LightPos ("Light Pos",Vector) = (0,0,0,0)
        _LightDir ("Light Dir",Vector) = (0,0,0,0)
        _LightColor ("Light Color",Color) = (1,1,1,1) 
        _LightStrength ("Light Strength",float) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"

            //"LightMode" = "UniversalForward"
        }
        LOD 100
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
        
        
        Cull back
        Pass 
        {
            Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float4 position : POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_LUT_Tex : TEXCOORD0;
                float2 uv_BumpMap : TEXCOORD0;
                float2 uv_Emission : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 worldPos : NORMAL0;
                float2 uv : TEXCOORD0;
                float2 uv_LUT_Tex : TEXCOORD1;
                float2 uv_BumpMap : TEXCOORD2;
                float2 uv_Emission : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                float3 normal : NORMAL1;
            };



            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D(_LUT_Tex);
            SAMPLER(sampler_LUT_Tex);

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            float4 _Color;
            float4 _EmissionColor;
            float4 _ShadowColor;

            float3 _LightPos;
            float3 _LightDir;
            float4 _LightColor;
            float _LightStrength;

            bool _UseBumpMap;
            bool _UseSpecular;
            bool _UseMainLight;
            //Texture2D _MainTex;
            //SamplerState sampler_MainTex;

            float _Cel;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                float4 _LUT_Tex_ST;
                float4 _EmissionMap_ST;
            CBUFFER_END



            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 temp = TransformObjectToWorld(IN.position.xyz);
                OUT.position = TransformObjectToHClip(IN.position.xyz);
                OUT.worldPos = temp;
                OUT.uv = IN.uv_MainTex;
                OUT.normal = TransformObjectToWorld(IN.normal);
                OUT.uv_BumpMap = IN.uv_BumpMap;
                OUT.uv_LUT_Tex = IN.uv_LUT_Tex;
                OUT.uv_Emission = IN.uv_Emission;
                OUT.viewDir = normalize(_WorldSpaceCameraPos.xyz - temp);
                    
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex));
                half4 BandLUT = SAMPLE_TEXTURE2D(_LUT_Tex, sampler_LUT_Tex, TRANSFORM_TEX(IN.uv_LUT_Tex, _LUT_Tex));
                half4 EmissionMap = SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, TRANSFORM_TEX(IN.uv_Emission, _EmissionMap));

                float3 Normal;

                //half3 WorldPos = IN.worldPos;
                //half3 WorldNormal = IN.normal;
                //half3 Normals;
                //half3 WorldView = _WorldSpaceCameraPos.xyz;
                //half3 diffuseColor;
                //AdditionalLights_half(WorldPos,WorldNormal,WorldView,diffuseColor, Normals);

                if(_UseBumpMap) 
                {
                    Normal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, TRANSFORM_TEX(IN.uv_BumpMap, _BumpMap)));
                }
                else 
                {
                    Normal = normalize(IN.normal); 
                }
                float3 lightDir;
                half3 lightColor;
                float attenuation;

                if(_UseMainLight) 
                {
                    Light light = GetMainLight();
                    lightDir = normalize(light.direction);  
                    lightColor = light.color;
                    attenuation = light.distanceAttenuation;
                } else 
                {
                    lightDir = normalize(_LightPos.xyz - IN.worldPos);
                    lightDir *= _LightDir;
                    lightColor = _LightColor;
                    float3 vertexToLightSource = TransformObjectToWorld(_LightPos.xyz) - TransformObjectToWorld(IN.worldPos);
                    float distance = length(vertexToLightSource);
                    //attenuation = (100 / distance) * _LightStrength;
                    attenuation =1;
                }

                half Ndotl = saturate(dot(Normal, lightDir));
                
                half halfLambert = Ndotl * 0.5 + 0.5;
                half Toon = floor(halfLambert * _Cel) * (1/_Cel);
                col *= Toon;

                
                float3 BandedDiffuse = SAMPLE_TEXTURE2D(_LUT_Tex, sampler_LUT_Tex, float2(Toon,0.5f)).rgb;
                float3 SpecularColor;
                float3 HalfVector = normalize(lightDir + IN.viewDir);
                float HDotN = saturate(dot(HalfVector, Normal));
                float PowedHDotN = pow(HDotN, 500.0f);
                
                float SpecularSmooth = smoothstep(0.005, 0.01f, PowedHDotN);
                SpecularColor = SpecularSmooth * 1.0f;
                

                half4 finalColor;
                if(_UseSpecular) 
                {
                    finalColor.rgb = ((col * (_Color + (_EmissionColor * EmissionMap.rgb))) + SpecularColor) * BandedDiffuse * (lightColor * attenuation);
                }
                else 
                {
                    finalColor.rgb = (col * (_Color + (_EmissionColor * EmissionMap.rgb))) * BandedDiffuse * (lightColor * attenuation);
                }
                
                //finalColor.rgb = diffuseColor;
                finalColor.a = 1;
                
                return finalColor;
            }
            ENDHLSL
            
        }
        
    }
}

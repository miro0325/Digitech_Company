Shader "Unlit/ToonShader"
{
    Properties
    {
        [Header(Textures),Space(3)]
        _Outline_Color("Outline Color",Color) = (0,0,0,1)
        _Color("Main Tex Color", Color) = (1,1,1,1)
        [HDR]_EmissionColor("Emission Color",Color) = (1,1,1,1)

        [Toggle(USE_BUMPMAP)]_UseBumpMap("Enable Normal Map", Range(0,1)) = 0 
        [Toggle(USE_SPECULAR)]_UseSpecular("Enable Specular", Range(0,1)) = 0 
        [Toggle(ONLY_MAINLIGHT)]_UseMainLight("Enable MainLight", float) = 1

        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap("NormalMap", 2D) = "bump" {}
        _EmissionMap("Emission Map",2D) = "white" {}
        _RampTex("Ramp Texture", 2D) = "white" {}

        _Outline_Bold ("Outline Bold", float) = 0.1
        _Cel ("Cel", Range(1,10)) = 3
        _RimThreshold("Rim Threshold",float) = 1
        _Smoothness("Smoothness",float) = 1

        _EdgeDiffuse ("Edge Diffuse", float) = 1
        _EdgeSpecular ("Edge Specular", float) = 1
        _EdgeSpecularOffset ("Edge Specular Offset", float) = 1
        _EdgeDistanceAttenuation ("Edge Distance Attenuation", float) = 1
        _EdgeShadowAttenuation ("Edge Shadow Attenuation", float) = 1
        _EdgeRim ("Edge Rim", float) = 1
        _EdgeRimOffset ("Edge Rim Offset", float) = 1
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque"
            "RenderPipeline" = "UniversalPipeline"
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
                float4 color : COLOR;
            };

            
            float _Outline_Bold;
            float4 _Outline_Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex);
                float3 normal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV,v.normal));
                float2 offset = mul((float2x2)UNITY_MATRIX_P, normal.xy);
                o.vertex.xy += offset * o.vertex.z * _Outline_Bold;
                o.color = _Outline_Color;
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                return i.color;
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

            #pragma shader_feature USE_BUMPMAP
            #pragma shader_feature USE_SPECULAR
            #pragma shader_feature ONLY_MAINLIGHT 

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes
            {
                float4 position : POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_RampTex : TEXCOORD0;
                float2 uv_BumpMap : TEXCOORD0;
                float2 uv_Emission : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 worldPos : NORMAL0;
                float3 localPos : NORMAL2;
                float2 uv : TEXCOORD0;
                float2 uv_RampTex : TEXCOORD1;
                float2 uv_BumpMap : TEXCOORD2;
                float2 uv_Emission : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                float3 worldNormal : NORMAL1;
            };

            struct LightVariables {
                float3 Normal : NORMAL;
                float3 ViewDir : TEXCOORD0;
                float Smoothness;
                float Shininess;
                float RimThreshold;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);

            TEXTURE2D(_EmissionMap);
            SAMPLER(sampler_EmissionMap);

            float4 _Color;
            float4 _EmissionColor;
            float4 _ShadowColor;

            float _EdgeDiffuse;
            float _EdgeSpecular;
            float _EdgeSpecularOffset;
            float _EdgeDistanceAttenuation;
            float _EdgeShadowAttenuation;
            float _EdgeRim;
            float _EdgeRimOffset;

            float _RimThreshold;
            float _Smoothness;
                
            bool _UseBumpMap;
            bool _UseSpecular;
            bool _UseMainLight;
            //Texture2D _MainTex;
            //SamplerState sampler_MainTex;

            float _Cel;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                float4 _RampTex_ST;
                float4 _EmissionMap_ST;
            CBUFFER_END

            half3 CalculateCelShading(Light l, LightVariables s) {
                float shadowAttenuationSmoothSpeed = smoothstep(0,_EdgeShadowAttenuation,l.shadowAttenuation);
                float distanceAttenuationSmoothSpeed = smoothstep(0,_EdgeDistanceAttenuation,l.distanceAttenuation);
                float attenuation = shadowAttenuationSmoothSpeed * distanceAttenuationSmoothSpeed;
                
                float diffuse = saturate(dot(s.Normal,l.direction));
                diffuse = ceil(diffuse * 0.5 + 0.5);
                diffuse *= attenuation;
                float3 h = SafeNormalize(l.direction + s.ViewDir);
                float specular = saturate(dot(s.Normal,h));
                specular = pow(specular,s.Shininess);
                specular = specular > 0.2 ? 1 : 0; 
                specular *= diffuse * s.Smoothness;

                float rim = 1 - dot(s.ViewDir,s.Normal);
                rim = rim > 0.75 ? 1 : 0;

                diffuse = smoothstep(0, _EdgeDiffuse,diffuse);
                //specular = s.Smoothness * smoothstep(1 - s.Smoothness) * _EdgeSpecular + _EdgeSpecularOffset,_EdgeSpecular + _EdgeSpecularOffset,specular);

                return l.color * (diffuse + max(specular,rim));
            }

            void Lighting_CelShading(float Smoothness,float RimThreshold, float3 Position, float3 Normal, float3 View, out float3 Color) {
                LightVariables s;

                s.Normal = Normal;
                s.ViewDir = SafeNormalize(View);
                s.Smoothness = Smoothness;
                s.Shininess = exp2(10 * Smoothness + 1);
                s.RimThreshold = RimThreshold;

                float4 clipPos = TransformObjectToHClip(Position);
                float4 shadowCoord = ComputeScreenPos(clipPos);

                //float4 shadowCoord = TransformWorldToShadowCoord(Position);

                Light light = GetMainLight();
                Color = CalculateCelShading(light,s);
                #ifdef ONLY_MAINLIGHT
                #else
                int pixelLightCount = GetAdditionalLightsCount();
                for(int i = 0; i < pixelLightCount; i++) {
                    light = GetAdditionalLight(i,Position);
                    Color += CalculateCelShading(light,s);
                }
                #endif
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                float3 temp = TransformObjectToWorld(IN.position.xyz);
                OUT.localPos = IN.position;
                OUT.position = TransformObjectToHClip(IN.position.xyz);
                OUT.worldPos = temp;
                OUT.uv = IN.uv_MainTex;
                OUT.worldNormal = TransformObjectToWorld(IN.normal);
                OUT.uv_BumpMap = IN.uv_BumpMap;
                OUT.uv_RampTex = IN.uv_RampTex;
                OUT.uv_Emission = IN.uv_Emission;
                OUT.viewDir = normalize(_WorldSpaceCameraPos.xyz - temp);
                    
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex));
                half4 EmissionMap = SAMPLE_TEXTURE2D_LOD(_EmissionMap, sampler_EmissionMap, TRANSFORM_TEX(IN.uv_Emission, _EmissionMap),0);

                float3 Normal;


                #ifdef USE_BUMPMAP
                    Normal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, TRANSFORM_TEX(IN.uv_BumpMap, _BumpMap)));
                #else
                    Normal = normalize(IN.worldNormal); 
                #endif
                float3 lightDir;
                half3 lightColor;
                float attenuation;

                    Light light = GetMainLight();
                    lightDir = normalize(light.direction);  
                    lightColor = light.color;
                    attenuation = light.distanceAttenuation;
                    

                half rim = 1 - max(saturate(dot(Normal,normalize(IN.viewDir))),0);
                

                half Ndotl = saturate(max(0,dot(Normal, lightDir)));
                
                half halfLambert = Ndotl * 0.5 + 0.5;
                half Toon = floor(halfLambert * _Cel) * (1/_Cel);
                half2 rh = Toon; 
                half3 Ramp = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(Toon,0.5)).rgb;
                //col *= Toon;

                
                float3 BandedDiffuse = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(Toon,0.5f)).rgb;
                float3 SpecularColor;
                float3 HalfVector = normalize(lightDir + IN.viewDir);
                float HDotN = saturate(dot(HalfVector, Normal));
                float PowedHDotN = pow(HDotN, 500.0f);
                
                float SpecularSmooth = smoothstep(0.005, 0.01f, PowedHDotN);
                SpecularColor = SpecularSmooth * 1.0f;
                

                half4 finalColor;
                Lighting_CelShading(_Smoothness, _RimThreshold,IN.worldPos,Normal,IN.viewDir,finalColor.rgb);
                #ifdef USE_SPECULAR
                    finalColor.rgb *= (col * Ramp * (_Color  + (_EmissionColor * EmissionMap.rgb)));
                    //finalColor.rgb = ((col *  Ramp *(_Color  + (_EmissionColor * EmissionMap.rgb))) + SpecularColor) * (lightColor * attenuation);
                    //finalColor.rgb = (col * Ramp * (_Color + (_EmissionColor * EmissionMap.rgb)))  * (lightColor * attenuation);
                    //finalColor.rgb *= rim2;
                #else
                    finalColor.rgb *= (col * Ramp * (_Color  + (_EmissionColor * EmissionMap.rgb)));
                    //finalColor.rgb = (col * Ramp * ((_Color + (_EmissionColor * EmissionMap.rgb)))) * (lightColor * attenuation);
                    //finalColor.rgb *= rim2;
                #endif
                //finalColor.rgb = diffuseColor;
                finalColor.a = 1;
                
                return finalColor;
            }
            ENDHLSL
            
        }
        
    }
}

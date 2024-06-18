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
        [Toggle(SHADOWS_SCREEN)]_UseShadowScreen("Enable Shadow Screen", float) = 1

        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap("NormalMap", 2D) = "bump" {}
        _EmissionMap("Emission Map",2D) = "white" {}
        _RampTex("Ramp Texture", 2D) = "white" {}

        _Outline_Bold ("Outline Bold", float) = 0.1
        _Cel ("Cel", Range(1,10)) = 3
        _RimThreshold("Rim Threshold",float) = 1
        _Smoothness("Smoothness",float) = 1
        _NormalStrength("Normal Strength",Range(0,10)) = 1

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
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_RampTex : TEXCOORD0;
                float2 uv_BumpMap : TEXCOORD0;
                float2 uv_Emission : TEXCOORD0;
                float4 position : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 worldPos : NORMAL0;
                float3 worldNormal : NORMAL1;
                float3 localPos : NORMAL2;
                float2 uv : TEXCOORD0;
                float2 uv_RampTex : TEXCOORD1;
                float2 uv_BumpMap : TEXCOORD2;
                float2 uv_Emission : TEXCOORD3;
                float3 viewDir : TEXCOORD4;
                //#if USE_BUMPMAP
                float3 T : TEXCOORD5;
                float3 B : TEXCOORD6;
                float3 N : TEXCOORD7;
                //#endif
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
            float _NormalStrength;
                
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
                float shadowAttenuationSmoothStepped = smoothstep(0,_EdgeShadowAttenuation,l.shadowAttenuation);
                float distanceAttenuationSmoothStepped = smoothstep(0,_EdgeDistanceAttenuation,l.distanceAttenuation);
                float attenuation = shadowAttenuationSmoothStepped * distanceAttenuationSmoothStepped;

                float diffuse = saturate(dot(s.Normal,l.direction)) * 0.5 + 0.5;
                diffuse *= attenuation;
                //half Toon = floor(diffuse * _Cel) * (1/_Cel);
                half Toon = ceil(diffuse * _Cel) / _Cel;
                //Toon = Toon > 0 ? 1 : 0;
                
                float3 HalfVector = SafeNormalize(l.direction + s.ViewDir);
                float specular = saturate(dot(s.Normal,HalfVector));
                specular = pow(specular,s.Shininess);
                specular *= Toon * s.Smoothness;
                specular = specular > 0.2 ? 1 : 0; 

                float rim = 1 - dot(s.ViewDir,s.Normal);
                rim *= pow(Toon,s.RimThreshold);
                rim = rim > 0.75 ? 1 : 0;
                
                Toon = smoothstep(0, _EdgeDiffuse,Toon);
                specular = s.Smoothness * smoothstep((1 - s.Smoothness) * _EdgeSpecular + _EdgeSpecularOffset,_EdgeSpecular + _EdgeSpecularOffset,specular); 
                rim = s.Smoothness * smoothstep(_EdgeRim - 0.5f * _EdgeRimOffset,_EdgeRim + 0.5f * _EdgeRimOffset,rim);

                half3 Ramp = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(1 - Toon,0.5)).rgb;
                return l.color * (Ramp + max(specular,rim));
            }   

            void Lighting_ToonShading(float Smoothness,float RimThreshold, float3 Position, float3 Normal, float3 View, float3 Ramp, out float3 Color) {
                LightVariables s;

                s.Normal = Normal;
                s.ViewDir = SafeNormalize(View);
                s.Smoothness = Smoothness;
                s.Shininess = exp2(10 * Smoothness + 1);
                s.RimThreshold = RimThreshold;
                #if SHADOWS_SCREEN
                    float4 clipPos = TransformObjectToHClip(Position);
                    float4 shadowCoord = ComputeScreenPos(clipPos);
                #else
                    float4 shadowCoord = TransformWorldToShadowCoord(Position);
                #endif

                //float4 shadowCoord = TransformWorldToShadowCoord(Position);

                Light light = GetMainLight(shadowCoord);
                Color = CalculateCelShading(light,s);
                #ifdef ONLY_MAINLIGHT
                #else
                int pixelLightCount = GetAdditionalLightsCount();
                for(int i = 0; i < pixelLightCount; i++) {
                    light = GetAdditionalLight(i,Position,1);
                    Color += CalculateCelShading(light,s);
                }
                #endif
            }

            void LocalNormalToTBN(half3 localNormal, float4 tangent, inout half3 T, inout half3 B, inout half3 N) 
            {
                half tangentSign = tangent.w * unity_WorldTransformParams.w;    
                N = normalize(TransformObjectToWorld(localNormal));
                T = normalize(mul(unity_ObjectToWorld, float4(tangent.x, tangent.y, tangent.z, 0)).xyz);
                B = normalize(cross(N,T) * tangentSign);
            }

            half3 TangentNormalToWorldNormal(half3 tangentNormal, half3 T, half3 B, half3 N) 
            {
                float3x3 TBN = float3x3(T,B,N);
                TBN = transpose(TBN);
                return mul(TBN,tangentNormal);
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
                //OUT.worldNormal = IN.normal;
                OUT.uv_BumpMap = IN.uv_BumpMap;
                OUT.uv_RampTex = IN.uv_RampTex;
                OUT.uv_Emission = IN.uv_Emission;
                OUT.viewDir = normalize(_WorldSpaceCameraPos.xyz - IN.position);
                //OUT.viewDir = WorldSpaceViewDir(IN.position);    
                #if USE_BUMPMAP
                LocalNormalToTBN(IN.normal,IN.tangent,OUT.T,OUT.B,OUT.N);
                #endif
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, TRANSFORM_TEX(IN.uv, _MainTex));
                half4 EmissionMap = SAMPLE_TEXTURE2D_LOD(_EmissionMap, sampler_EmissionMap, TRANSFORM_TEX(IN.uv_Emission, _EmissionMap),0);

                float3 Normal;

                #ifdef USE_BUMPMAP
                    float2 uv = TRANSFORM_TEX(IN.uv,_MainTex);
                    half3 tangentNormal = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv * _BumpMap_ST.rg));
                    tangentNormal.xy *= _NormalStrength;
                    Normal = (TangentNormalToWorldNormal(tangentNormal,IN.T,IN.B,IN.N));
                #else
                    Normal = normalize(IN.worldNormal); 
                #endif
                half3 lightDir;
                //half3 lightColor;
                //float attenuation;

                Light light = GetMainLight();
                lightDir = normalize(light.direction);  
                //lightColor = light.color;
                //attenuation = light.distanceAttenuation;

                //half rim = 1 - max(saturate(dot(Normal,normalize(IN.viewDir))),0);
                
                half Ndotl = saturate(max(0,dot(Normal, lightDir)));
                
                half halfLambert = Ndotl * 0.5 + 0.5;
                half Toon = floor(halfLambert * _Cel) * (1/_Cel);
                //half2 rh = Toon; 
                half3 Ramp = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(Toon,0.5)).rgb;
                ////col *= Toon;
                
                //float3 BandedDiffuse = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, float2(Toon,0.5f)).rgb;
                //float3 SpecularColor;
                //float3 HalfVector = normalize(lightDir + IN.viewDir);
                //float HDotN = saturate(dot(HalfVector, Normal));
                //float PowedHDotN = pow(HDotN, 500.0f);
                
                //float SpecularSmooth = smoothstep(0.005, 0.01f, PowedHDotN);
                //SpecularColor = SpecularSmooth * 1.0f;
                
                half4 finalColor;
                Lighting_ToonShading(_Smoothness, _RimThreshold,IN.worldPos,Normal,IN.viewDir,Ramp,finalColor.rgb);
                #ifdef USE_SPECULAR
                    finalColor.rgb *= (col *_Color);
                    finalColor.rgb += (EmissionMap.rgb * _EmissionColor);
                    //finalColor.rgb = ((col *  Ramp * (_Color  + (_EmissionColor * EmissionMap.rgb))) + SpecularColor) * (lightColor * attenuation);
                    //finalColor.rgb = (col * Ramp * (_Color + (_EmissionColor * EmissionMap.rgb)))  * (lightColor * attenuation);
                    //finalColor.rgb *= rim2;
                #else
                    //finalColor.rgb *= (col * Ramp * (_Color  + (_EmissionColor * EmissionMap.rgb)));
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

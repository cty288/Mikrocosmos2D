﻿Shader "Test/2D/Sprite-Lit-Entity"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        _MaskTex("Mask", 2D) = "white" {}
        _NormalMap("Normal Map", 2D) = "bump" {}
        _AlphaValue("Alpha Value", Float) = 1
        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
       _Color("Text Color", Color) = (1,1,1,1)
        _Tint("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
        _OutlineWidth("OutlineWidth", Range(0,100)) = 0
        [Toggle(FOR_TEXT)] _ForText("For Text", Float) = 0

    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
    ENDHLSL

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        // Outline pass
		
        Pass
        {
            Tags { "LightMode" = "Universal2D" }
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma shader_feature FOR_TEXT
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2  uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                float4  color       : COLOR;
                float2	uv          : TEXCOORD0;
                float2	lightingUV  : TEXCOORD1;
                float2  worldPos    : TEXCOORD2;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);float4 _MainTex_TexelSize;
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            half4 _MainTex_ST;
            half4 _NormalMap_ST;
            float _AlphaValue;
            uniform float4 _Color;
            half4 _Tint;
            float _OutlineWidth;
            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                float4 clipVertex = o.positionCS / o.positionCS.w;
                o.lightingUV = ComputeScreenPos(clipVertex).xy;
                o.color = v.color;
                o.worldPos = mul (unity_ObjectToWorld, v.positionOS);
                return o;
            }

            #include "Include/ShapeLightVisionEntity.hlsl"

            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {
                half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                #ifdef FOR_TEXT
                if(main.a >0){
                    main += _Color;
                }
              
				
               
                #endif
                
				
                half4 result = CombinedShapeLightShared(main, mask, i.lightingUV, i.worldPos) * _Tint ;
                //_AlphaValue = CombinedShapeLightShared(main,mask, half2(0.5,0.5), mul(unity_ObjectToWorld, half2(0.5,0.5)));
                 
                return result; //+ _Color;
            }
            ENDHLSL
        }
        
        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment
            uniform float4 _Color;
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color		: COLOR;
                float2 uv			: TEXCOORD0;
                float4 tangent      : TANGENT;
            };

            struct Varyings
            {
                float4  positionCS		: SV_POSITION;
                float4  color			: COLOR;
                float2	uv				: TEXCOORD0;
                float3  normalWS		: TEXCOORD1;
                float3  tangentWS		: TEXCOORD2;
                float3  bitangentWS		: TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            float4 _NormalMap_ST;  // Is this the right way to do this?

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap);
                o.uv = attributes.uv;
                o.color = attributes.color;
                o.normalWS = TransformObjectToWorldDir(float3(0, 0, -1));
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            float4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));
               

                float4 result = NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
                 

                   return result;

            }
            ENDHLSL
        }
        

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment
            uniform float4 _Color;
            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color		: COLOR;
                float2 uv			: TEXCOORD0;
            };

            struct Varyings
            {
                float4  positionCS		: SV_POSITION;
                float4  color			: COLOR;
                float2	uv				: TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                o.uv = attributes.uv;
                o.color = attributes.color;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {
                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                
               
                return mainTex ;
            }
            ENDHLSL
        }

   
    }

    Fallback "Sprites/Default"
}

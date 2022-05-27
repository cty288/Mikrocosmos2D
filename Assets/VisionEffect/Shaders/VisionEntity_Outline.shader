Shader "Test/2D/VisionEntity_Outline"
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

        [Toggle(FOR_TEXT)] _ForText("For Text", Float) = 0


        _Color("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
     
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0

        [MaterialToggle] _IsOutlineEnabled("Enable Outline", float) = 0
        [HDR] _OutlineColor("Outline Color", Color) = (1,1,1,1)
        _OutlineSize("Outline Size", Range(1, 10)) = 1
        _AlphaThreshold("Alpha Threshold", Range(0, 1)) = 0.01
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
            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex ComputeVertex
            #pragma fragment ComputeFragment
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile _ SPRITE_OUTLINE_OUTSIDE

            #ifndef SAMPLE_DEPTH_LIMIT
            #define SAMPLE_DEPTH_LIMIT 10
            #endif

            #ifdef UNITY_INSTANCING_ENABLED

            UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, unity_SpriteRendererColorArray)
            UNITY_DEFINE_INSTANCED_PROP(fixed2, unity_SpriteFlipArray)
            UNITY_INSTANCING_BUFFER_END(PerDrawSprite)
            #define _RendererColor UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
            #define _Flip UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)

            UNITY_INSTANCING_BUFFER_START(PerDrawSpriteOutline)
            UNITY_DEFINE_INSTANCED_PROP(float,  _IsOutlineEnabledArray)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _OutlineColorArray)
            UNITY_DEFINE_INSTANCED_PROP(float,  _OutlineSizeArray)
            UNITY_DEFINE_INSTANCED_PROP(float,  _AlphaThresholdArray)
            UNITY_INSTANCING_BUFFER_END(PerDrawSpriteOutline)
            #define _IsOutlineEnabled UNITY_ACCESS_INSTANCED_PROP(PerDrawSpriteOutline, _IsOutlineEnabledArray)
            #define _OutlineColor UNITY_ACCESS_INSTANCED_PROP(PerDrawSpriteOutline, _OutlineColorArray)
            #define _OutlineSize UNITY_ACCESS_INSTANCED_PROP(PerDrawSpriteOutline, _OutlineSizeArray)
            #define _AlphaThreshold UNITY_ACCESS_INSTANCED_PROP(PerDrawSpriteOutline, _AlphaThresholdArray)

            #endif 

            CBUFFER_START(UnityPerDrawSprite)
            #ifndef UNITY_INSTANCING_ENABLED
            fixed4 _RendererColor;
            fixed2 _Flip;
            #endif
            float _EnableExternalAlpha;
            CBUFFER_END

            CBUFFER_START(UnityPerDrawSpriteOutline)
            #ifndef UNITY_INSTANCING_ENABLED
            fixed4 _OutlineColor;
            float _IsOutlineEnabled, _OutlineSize, _AlphaThreshold;
            #endif
            CBUFFER_END

            sampler2D _MainTex, _AlphaTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;

            struct VertexInput
            {
                float4 Vertex : POSITION;
                float4 Color : COLOR;
                float2 TexCoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct VertexOutput
            {
                float4 Vertex : SV_POSITION;
                fixed4 Color : COLOR;
                float2 TexCoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            inline float4 UnityFlipSprite(in float3 pos, in fixed2 flip)
            {
                return float4(pos.xy * flip, pos.z, 1.0);
            }

            VertexOutput ComputeVertex(VertexInput vertexInput)
            {
                VertexOutput vertexOutput;

                UNITY_SETUP_INSTANCE_ID(vertexInput);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(vertexOutput);

                vertexOutput.Vertex = UnityFlipSprite(vertexInput.Vertex, _Flip);
                vertexOutput.Vertex = UnityObjectToClipPos(vertexInput.Vertex);
                vertexOutput.TexCoord = vertexInput.TexCoord;
                vertexOutput.Color = vertexInput.Color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                vertexOutput.Vertex = UnityPixelSnap(vertexOutput.Vertex);
                #endif

                return vertexOutput;
            }

            // Determines whether _OutlineColor should replace sampledColor at the given texCoord when drawing inside the sprite borders.
            // Will return 1 when the test is positive (should draw outline), 0 otherwise.
            int ShouldDrawOutlineInside (fixed4 sampledColor, float2 texCoord, int isOutlineEnabled, int outlineSize, float alphaThreshold)
            {
                // Won't draw if effect is disabled, outline size is zero or sampled fragment is tranpsarent.
                if (isOutlineEnabled * outlineSize * sampledColor.a == 0) return 0;

                float2 texDdx = ddx(texCoord);
                float2 texDdy = ddy(texCoord);

                // Looking for a transparent pixel (sprite border from inside) around computed fragment with given depth (_OutlineSize).
                // Also checking if sampled fragment is out of the texture space (UV is out of 0-1 range); considering such fragment as sprite border.
                for (int i = 1; i <= SAMPLE_DEPTH_LIMIT; i++)
                {
                    float2 pixelUpTexCoord = texCoord + float2(0, i * _MainTex_TexelSize.y);
                    fixed pixelUpAlpha = pixelUpTexCoord.y > 1.0 ? 0.0 : tex2Dgrad(_MainTex, pixelUpTexCoord, texDdx, texDdy).a;
                    if (pixelUpAlpha <= alphaThreshold) return 1;

                    float2 pixelDownTexCoord = texCoord - float2(0, i * _MainTex_TexelSize.y);
                    fixed pixelDownAlpha = pixelDownTexCoord.y < 0.0 ? 0.0 : tex2Dgrad(_MainTex, pixelDownTexCoord, texDdx, texDdy).a;
                    if (pixelDownAlpha <= alphaThreshold) return 1;

                    float2 pixelRightTexCoord = texCoord + float2(i * _MainTex_TexelSize.x, 0);
                    fixed pixelRightAlpha = pixelRightTexCoord.x > 1.0 ? 0.0 : tex2Dgrad(_MainTex, pixelRightTexCoord, texDdx, texDdy).a;
                    if (pixelRightAlpha <= alphaThreshold) return 1;

                    float2 pixelLeftTexCoord = texCoord - float2(i * _MainTex_TexelSize.x, 0);
                    fixed pixelLeftAlpha = pixelLeftTexCoord.x < 0.0 ? 0.0 : tex2Dgrad(_MainTex, pixelLeftTexCoord, texDdx, texDdy).a;
                    if (pixelLeftAlpha <= alphaThreshold) return 1;

                    if (i > outlineSize) break;
                }

                return 0;
            }

            // Determines whether _OutlineColor should replace sampledColor at the given texCoord when drawing outside the sprite borders.
            // Will return 1 when the test is positive (should draw outline), 0 otherwise.
            int ShouldDrawOutlineOutside (fixed4 sampledColor, float2 texCoord, int isOutlineEnabled, int outlineSize, float alphaThreshold)
            {
                // Won't draw if effect is disabled, outline size is zero or sampled fragment is above alpha threshold.
                if (isOutlineEnabled * outlineSize == 0) return 0;
                if (sampledColor.a > alphaThreshold) return 0;

                float2 texDdx = ddx(texCoord);
                float2 texDdy = ddy(texCoord);

                // Looking for an opaque pixel (sprite border from outise) around computed fragment with given depth (_OutlineSize).
                for (int i = 1; i <= SAMPLE_DEPTH_LIMIT; i++)
                {
                    float2 pixelUpTexCoord = texCoord + float2(0, i * _MainTex_TexelSize.y);
                    fixed pixelUpAlpha = tex2Dgrad(_MainTex, pixelUpTexCoord, texDdx, texDdy).a;
                    if (pixelUpAlpha > alphaThreshold) return 1;

                    float2 pixelDownTexCoord = texCoord - float2(0, i * _MainTex_TexelSize.y);
                    fixed pixelDownAlpha = tex2Dgrad(_MainTex, pixelDownTexCoord, texDdx, texDdy).a;
                    if (pixelDownAlpha > alphaThreshold) return 1;

                    float2 pixelRightTexCoord = texCoord + float2(i * _MainTex_TexelSize.x, 0);
                    fixed pixelRightAlpha = tex2Dgrad(_MainTex, pixelRightTexCoord, texDdx, texDdy).a;
                    if (pixelRightAlpha > alphaThreshold) return 1;

                    float2 pixelLeftTexCoord = texCoord - float2(i * _MainTex_TexelSize.x, 0);
                    fixed pixelLeftAlpha = tex2Dgrad(_MainTex, pixelLeftTexCoord, texDdx, texDdy).a;
                    if (pixelLeftAlpha > alphaThreshold) return 1;

                    if (i > outlineSize) break;
                }

                return 0;
            }

            fixed4 SampleSpriteTexture(float2 uv)
            {
                fixed4 color = tex2D(_MainTex, uv);

                #if ETC1_EXTERNAL_ALPHA
                fixed4 alpha = tex2D(_AlphaTex, uv);
                color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
                #endif

                return color;
            }

            fixed4 ComputeFragment(VertexOutput vertexOutput) : SV_Target
            {
                fixed4 color = SampleSpriteTexture(vertexOutput.TexCoord) * vertexOutput.Color;
                color.rgb *= color.a;

                #ifdef SPRITE_OUTLINE_OUTSIDE
                int shouldDrawOutline = ShouldDrawOutlineOutside(color, vertexOutput.TexCoord, _IsOutlineEnabled, _OutlineSize, _AlphaThreshold);
                #else
                int shouldDrawOutline = ShouldDrawOutlineInside(color, vertexOutput.TexCoord, _IsOutlineEnabled, _OutlineSize, _AlphaThreshold);
                #endif

                color.rgb = lerp(color.rgb, _OutlineColor.rgb * _OutlineColor.a, shouldDrawOutline);

                return color;
            }

            ENDCG
        }

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

            TEXTURE2D(_MainTex);
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

Shader "examples/week 10/VHS"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTexture("Noise texture", 2D) = "white" {}
        _DitherTexture("Dither texture", 2D) = "white" {} 
        _LensDistortion("Lens distortion", float) = 1.2 
        _Scale("Distortion scale", float) = 0.5
        //_Iterations("Iterations", float) = 0
        _ChromaticAberration("Chromatic aberration", float) = 0
        _ChromaticAberrationIntensity("Chromatic aberration intensity", float) = 0
        _LineAmount("Line amount", float) = 1
        _LineDisplacement("Lines displacement", float) = 0
        _LineSpeed("Lines speed", float) = 0
        _WaveAmount("Sine lines amount", float) = 1
        _WaveSpeed("Sine lines speed", float) = 0
        _WaveDisplacement("Sine lines displacement", float) = 0
    }
    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
             
            #define MAX_OFFSET 0.15

            sampler2D _MainTex; float4 _MainTex_TexelSize;
            sampler2D _NoiseTexture; float4 _NoiseTexture_ST;
            sampler2D _DitherTexture; float4 _DitherTexture_TexelSize;
            float _LensDistortion;
            float _Scale;
            //float _Iterations;
            float _ChromaticAberration;
            float _ChromaticAberrationIntensity;
            float _LineAmount;
            float _LineDisplacement;
            float _LineSpeed;
            float _WaveAmount;
            float _WaveDisplacement;
            float _WaveSpeed;
            
            float rand (float2 uv) {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                float2 uv = i.uv;

                uv -= 0.5;
                float radius = pow(length(uv), 2);
                float distort = 1 + radius * _LensDistortion;
                uv = uv * distort * _Scale + 0.5;

                //uv.y = frac(uv.y + lerp(0.0, 1.0, frac(_Time.y * 2.0)));
                uv.y = frac(uv.y + lerp(0.0, 0.75, frac(_Time.z) * step(0.95, rand(floor(_Time.z * 2.5)))));
                
                float modifier = length(uv  * 2 - 1);
                float offset = MAX_OFFSET * modifier * _ChromaticAberrationIntensity;

                float lines = step(0.5, frac(uv.y * _LineAmount + _Time.y * _LineSpeed));
                float lineDisplacement = lines * _LineDisplacement;

                float waves = sin(uv.y * _WaveAmount + _Time.y * _WaveSpeed) * 0.5 + 0.5;
                float waveDisplacement = waves  * _WaveDisplacement;

                float r = tex2D(_MainTex, uv + float2(offset + _ChromaticAberration + lineDisplacement + waveDisplacement, 0) * _MainTex_TexelSize.xy).r;
                float g = tex2D(_MainTex, uv + float2(offset - lineDisplacement + waveDisplacement, 0) * _MainTex_TexelSize.xy).g;
                float b = tex2D(_MainTex, uv - float2(offset + _ChromaticAberration - lineDisplacement + waveDisplacement, 0) * _MainTex_TexelSize.xy).b;
                
                float3 color = float3(r, g, b);

                color *= max(0.7, rand(uv + _Time.x));
               
                float noise = tex2D(_NoiseTexture, uv * _NoiseTexture_ST.xy).x;

                float lines2 = step(0.5, frac(uv.y * _LineAmount + _Time.y * _LineSpeed * 5));
                color += step(0.75, 1.0 - lines2) * step(waves, noise) * 0.10;

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}

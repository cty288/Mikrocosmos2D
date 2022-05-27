Shader "mk/Background"
{
    Properties
    {
        _color1 ("color 1", Color) = (0.1, 0.3, 0.0, 1)
        _color2 ("color 2", Color) = (0.8, 0.1, 0.0, 1)
        _color3 ("color 3", Color) = (0.6 ,0.3, 0.1, 1)
        _color4 ("color 4", Color) = (0.1, 0.1, 0.4, 1)
        _tex ("texture", 2D) = "white" {}
        _scale ("noise scale", Range(2, 800)) = 5
        _speed ("speed", Range(0, 10)) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _tex;

            float4 _color1;
            float4 _color2;
            float4 _color3;
            float4 _color4;

            float _scale;
            float _speed;
            float _intensity;

            float rand (float2 uv) {
                return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.5453123);
            }

            float noise (float2 uv) {
                float2 ipos = floor(uv);
                float2 fpos = frac(uv); 
                
                float o  = rand(ipos);
                float x  = rand(ipos + float2(1, 0));
                float y  = rand(ipos + float2(0, 1));
                float xy = rand(ipos + float2(1, 1));

                float2 smooth = smoothstep(0, 1, fpos);
                return lerp( lerp(o,  x, smooth.x), lerp(y, xy, smooth.x), smooth.y);
            }

            float fractal_noise (float2 uv) {
                float n = 0;

                n  = (1 / 2.0)  * noise( uv * 1 + _Time.y * _speed);
                n += (1 / 4.0)  * noise( uv * 2 + _Time.y * _speed); 
                n += (1 / 8.0)  * noise( uv * 4 + _Time.y * _speed); 
                n += (1 / 16.0) * noise( uv * 8 + _Time.y * _speed);
                
                return n;
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

            float4 frag (Interpolators i) : SV_Target
            {   
                float2 uv = i.uv;
                uv *= _scale;

                float3 color = _color1;

                //newUV.x = fractal_noise(newUV);
                //newUV.y = fractal_noise(newUV + translate);
                //newUV = float2(fractal_noise(newUV), fractal_noise(newUV + translate));
                //uv += 5 * newUV;
                float fn1 = fractal_noise(uv + float2(1, 1));
                float fn2 = fractal_noise(uv + float2(1, 0) + fn1 * 2); //+ sin(_Time.y) * 0.1);
                float fn3 = fractal_noise(uv + float2(0, 1) + fn2 * 3); //+ sin(_Time.y) * 0.1);
                    
                color = lerp(color, _color2, fn1 * 3);
                color = lerp(color, _color3, fn2 * 2);
                color = lerp(color, _color4, fn3 * 1);

                //uv = fractal_noise(uv);
                //color = tex2D(_tex, uv);

                return float4(color, 1.0);
            }
            ENDCG
        }
    }
}

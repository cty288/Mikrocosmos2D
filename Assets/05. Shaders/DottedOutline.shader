Shader "mk/DottedOutline"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_OutlineExtrusion("Outline Extrusion", float) = 0
		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineDot("Outline Dot", float) = 0.25
		_OutlineDot2("Outline Dot Distance", float) = 0.5
		_OutlineSpeed("Outline Dot Speed", Range(0,0.2)) = 0
		_SourcePos("Source Position", vector) = (0, 0, 0, 0)
		_OutlineWidth("OutlineWidth", Range(0,100)) = 0
		_Transparency("Transparacy", Range(0,1)) = 0.5
		_OutlineTransparencyThreshold("Outline Transparacy Threshold", Range(0,0.5)) = 0.5

		[Toggle(FLIP_V)] _FlipVerticalDirection("Flip Vertical Direction", Float) = 0
		[Toggle(FLIP_H)] _FlipHorizontalDirection("Flip Horizontal Direction", Float) = 0
	}

	SubShader
	{
		Tags{
			"Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" 
		}
		Blend SrcAlpha OneMinusSrcAlpha
	

		// Outline pass
		Pass
		{
			// Won't draw where it sees ref value 4
			Cull OFF
			ZWrite OFF
			ZTest ON
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#pragma shader_feature FLIP_V
			#pragma shader_feature FLIP_H
			// Properties
			float4 _OutlineColor;
			float  _OutlineSize;
			float  _OutlineExtrusion;
			float  _OutlineDot;
			float  _OutlineDot2;
			float  _OutlineSpeed;
			float4 _SourcePos;
			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float _OutlineWidth;
			float _Transparency;
			float _OutlineTransparencyThreshold;
			struct vertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				 float2 uv : TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 screenCoord : TEXCOORD0;
				 float2 uv : TEXCOORD1;
			};

			vertexOutput vert(vertexInput input)
			{
				vertexOutput output;

				float4 newPos = input.vertex;

				// normal extrusion technique
				float3 normal = normalize(input.normal);
				newPos += float4(normal, 0.0) * _OutlineExtrusion;

				// convert to world space
				output.pos = UnityObjectToClipPos(newPos);

				// get screen coordinates
				output.screenCoord = ComputeScreenPos(output.pos);
				output.uv = input.uv;
				return output;
			}

			float4 frag(vertexOutput input) : COLOR
			{
				fixed4 col = tex2D(_MainTex, input.uv);
				col.a *= _Transparency;
				float2 pos;

				#ifdef FLIP_H
				     pos = input.uv.xy ;
				#else
				    pos = input.uv.yx ;
				#endif
                
				#ifdef FLIP_V
				     pos *= -1;
				#else
				  
				#endif
				pos += _Time * _OutlineSpeed;
                float skip = sin(_OutlineDot*abs(distance(_SourcePos.xy, pos * 1000))) + _OutlineDot2;
				

                
				

                float2 up_uv = input.uv + float2(0,1) * _OutlineWidth * _MainTex_TexelSize.xy;
                float2 down_uv =input.uv + float2(0,-1) * _OutlineWidth * _MainTex_TexelSize.xy;
                float2 left_uv = input.uv + float2(-1,0) * _OutlineWidth * _MainTex_TexelSize.xy;
                float2 right_uv =input.uv + float2(1,0) * _OutlineWidth * _MainTex_TexelSize.xy;
               
			    float up = tex2D(_MainTex,up_uv).a;
				float down = tex2D(_MainTex,down_uv).a;
				float left = tex2D(_MainTex,left_uv).a;
				float right = tex2D(_MainTex,right_uv).a;
                float w =  up* down * left * right;

			
                if(up!= 0 || down!=0 || left!=0 || right!=0){
				     
					  if(w<=_OutlineTransparencyThreshold){
					  col = lerp(_OutlineColor,col,w);
				         clip(skip); // stops rendering a pixel if 'skip' is negative
					     col.a = 1;
			       	}
				}else{
				    col.a = 0;
				}
               

			  
                return col;
			}

			ENDCG
		}


	}
}
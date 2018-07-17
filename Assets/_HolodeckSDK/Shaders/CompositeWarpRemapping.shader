Shader "Vulcan/WarpRemappingComposite"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WarpTex ("WarpTexture", 2D) = "white" {}
		_BlendTex ("BlendTexture", 2D) = "white" {}
		_MainBkgdTex ("TextureBkgd", 2D) = "white" {}
		_AlphaThresh ("AlphaThreshold", Range(0.0,1.0)) = 0.01	
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Cull Off ZWrite Off ZTest Off Lighting Off

		// Video Pass
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_nicest
			#pragma target 5.0

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			uniform sampler2D _MainBkgdTex;
			uniform float4 _MainBkgdTex_ST;
			uniform float4 _MainBkgdTex_TexelSize;

			uniform sampler2D_float _WarpTex;
			uniform sampler2D_float _BlendTex;
			
			uniform float4 _WarpTex_ST;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _WarpTex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				i.uv.x = 1-i.uv.x;
				
				float4 remap = tex2D(_WarpTex, i.uv);
				remap.y = lerp(remap.y, 1 - remap.y, _MainBkgdTex_ST.y > 0);

				float4 col = tex2D(_MainBkgdTex, frac(remap.xy));
				i.uv.y = 1 - i.uv.y;
				
				col *= tex2D(_BlendTex, i.uv).r;
				return col;
			}
			ENDCG
		}

		// Geometry Pass
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_nicest
			#pragma target 5.0
			
			#include "UnityCG.cginc"

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

			uniform sampler2D_float _MainTex;
			uniform sampler2D_float _WarpTex;
			uniform sampler2D_float _BlendTex;
			
			uniform float4 _MainTex_ST;
			uniform float4 _WarpTex_ST;
			uniform float4 _MainTex_TexelSize;

			uniform float _AlphaThresh;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _WarpTex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// world to texture correction
				i.uv.x = 1 - i.uv.x;

				float4 remap = tex2D(_WarpTex, i.uv);
				remap.y = lerp(remap.y, 1 - remap.y, _MainTex_ST.y < 1);

				float4 col = tex2D(_MainTex, frac(remap.xy));

				i.uv.y = 1 - i.uv.y;
				col *= tex2D(_BlendTex, i.uv).r;
				if (col.a < _AlphaThresh)
				{
					discard;
				}
				return col;
			}
			ENDCG
		}
	}
}
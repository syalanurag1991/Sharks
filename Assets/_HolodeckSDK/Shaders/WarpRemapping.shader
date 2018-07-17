Shader "Vulcan/WarpRemapping"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_WarpTex ("WarpTexture", 2D) = "white" {}
		_BlendTex ("BlendTexture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		ZTest Off Cull Off Blend Off Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_nicest
			#pragma target 5.0
			
			#include "UnityCG.cginc"
			#pragma multi_compile_fog

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			uniform sampler2D_float _MainTex;
			uniform sampler2D_float _WarpTex;
			uniform sampler2D_float _BlendTex;
			
			uniform float4 _MainTex_ST;
			uniform float4 _WarpTex_ST;
			uniform float4 _MainTex_TexelSize;
			
			v2f vert (appdata v)
			{
				v2f o;
				
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _WarpTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				i.uv.x = 1-i.uv.x;

				float4 remap = tex2D(_WarpTex, i.uv);
				remap.y = lerp(remap.y, 1 - remap.y, _MainTex_ST.y > 0);

				float4 col = tex2D(_MainTex, frac(remap.xy));
				i.uv.y = 1 - i.uv.y;
				
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col * tex2D(_BlendTex, i.uv).r;
			}
			ENDCG
		}
	}
}
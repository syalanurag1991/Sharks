//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

Shader "Vulcan/SimulatorUnlitGrid"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_GridColor("Grid Color", Color) = (1,1,1,1)
		_GridStep("Grid Step", Float) = 10
		_GridWidth("Grid Width", Float) = 1
		_GridStrength("Grid Strength", Range(0,1)) = 0.5
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		Cull Off Blend Off Lighting Off

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

			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			uniform float4 _GridColor;
			uniform float _GridStep;
			uniform float _GridWidth;
			uniform float _GridStrength;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// Checker debug width
				//o.uv *= 30;
				
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);

				float gsr = 1 / _GridStep;

				float2 pos = i.uv.xy / gsr;
				float2 f = abs(frac(pos) - 0.5);
				float2 df = fwidth(pos) * _GridWidth;
				float2 g = smoothstep(-df, df, f);
				float grid = 1.0 - saturate(g.x * g.y);

				col.rgb = lerp(col.rgb, _GridColor.rgb, grid * _GridStrength);

				/*float2 c = i.uv;
				c = floor(c) / 2;
				float checker = frac(c.x + c.y) * 2;
				return checker;*/

				return col;
			}
			ENDCG
		}
	}
}

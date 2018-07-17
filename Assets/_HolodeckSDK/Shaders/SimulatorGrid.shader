//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

Shader "Vulcan/SimulatorGrid" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0

		_GridStep("Grid Step", Float) = 10
		_GridWidth("Grid Width", Float) = 1
		_GridStrength("Grid Strength", Range(0,1)) = 0.5
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		float _GridStep;
		float _GridWidth;
		float _GridStrength;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex);

			float gsr = 1/_GridStep;

			float2 pos = IN.uv_MainTex.xy / gsr;
			float2 f = abs(frac(pos) - 0.5);
			float2 df = fwidth(pos) * _GridWidth;
			float2 g = smoothstep(-df, df, f);
			float grid = 1.0 - saturate(g.x * g.y);
			c.rgb = lerp(c.rgb, _Color.rgb, grid * _GridStrength);

			o.Albedo = c.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

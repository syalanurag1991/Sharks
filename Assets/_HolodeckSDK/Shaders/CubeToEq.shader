//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

Shader "Vulcan/CubemapToEquirectangular" {
	Properties{
		_MainTex_0("Cubemap (RGB)", CUBE) = "" {}
		_MainTex_1("Cubemap (RGB)", CUBE) = "" {}
		_MainTex_2("Cubemap (RGB)", CUBE) = "" {}
		_MainTex_3("Cubemap (RGB)", CUBE) = "" {}
		//_MainTexCubes("Cubemap Array", CUBEArray) = "" {}
		_NumCubesInUse("Number of Rigs", Range(1.0, 4.0)) = 1

		_edgeColor("Edge Color", Color) = (1,0,0,1)
		_edgeWidth("Edge Width", Range(0.0, 1.0)) = 0.5

		_VerticalMask("Vertical Mask", Range(-1.0,1.0)) = 1.0
	}

	Subshader {
		Pass {
			ZTest Always Cull Off ZWrite Off
			Fog{ Mode off }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"

			#define PI    3.141592653589793
			#define TWOPI 6.283185307179587

			struct v2f {
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
			};

			const float4 CLEAR_COLOR = float4(0, 0, 0, 0);

			uniform int _NumCubesInUse;

			uniform samplerCUBE _MainTex_0;
			uniform samplerCUBE _MainTex_1;
			uniform samplerCUBE _MainTex_2;
			uniform samplerCUBE _MainTex_3;
			uniform float4 _MainTex_0_ST;
			uniform float4 _MainTex_1_ST;
			uniform float4 _MainTex_2_ST;
			uniform float4 _MainTex_3_ST;

			uniform float4 _edgeColor;
			uniform float _edgeWidth;

			uniform float _VerticalMask;

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy * float2(TWOPI, PI);
				o.uv2 = TRANSFORM_TEX(v.texcoord, _MainTex_0);
				return o;
			}

			//UNITY_DECLARE_TEX2DARRAY(_MainTexCubes);

			fixed4 frag(v2f i) : COLOR
			{
				float theta = i.uv.y;
				float phi = i.uv.x;
				float3 unit = float3(0,0,0);

				unit.x = sin(phi) * sin(theta) * -1;
				unit.y = cos(theta) * -1;
				unit.z = cos(phi) * sin(theta) * -1;

				half4 c = CLEAR_COLOR;

				half4 c0 = texCUBE(_MainTex_0, unit);
				half4 c1 = texCUBE(_MainTex_1, unit);
				half4 c2 = texCUBE(_MainTex_2, unit);
				half4 c3 = texCUBE(_MainTex_3, unit);

				float edgePercOne = 1.0 / _NumCubesInUse;
				float edgePercTwo = edgePercOne * 2.0;
				float edgePercThree = edgePercOne * 3.0;

				half w = lerp(0, _edgeWidth * 0.005, step(1.1, _NumCubesInUse));

				half minEdgeOne = edgePercOne - w;
				half maxEdgeOne = edgePercOne + w;
				half minEdgeTwo = edgePercTwo - w;
				half maxEdgeTwo = edgePercTwo + w;
				half minEdgeThree = edgePercThree - w;
				half maxEdgeThree = edgePercThree + w;

				half screenMin = w;
				half screenMax = 1.0 - w;

				c += lerp(CLEAR_COLOR, _edgeColor, step(minEdgeOne, i.uv2.x) - step(maxEdgeOne, i.uv2.x));
				c += lerp(CLEAR_COLOR, _edgeColor, step(minEdgeTwo, i.uv2.x) - step(maxEdgeTwo, i.uv2.x));
				c += lerp(CLEAR_COLOR, _edgeColor, step(minEdgeThree, i.uv2.x) - step(maxEdgeThree, i.uv2.x));
				c += lerp(CLEAR_COLOR, _edgeColor, step(screenMax, i.uv2.x));
				c += lerp(CLEAR_COLOR, _edgeColor, step(i.uv2.x, screenMin));
				c += lerp(CLEAR_COLOR, c0, step(screenMin, i.uv2.x) - step(minEdgeOne, i.uv2.x));
				c += lerp(CLEAR_COLOR, c1, step(maxEdgeOne, i.uv2.x) - step(minEdgeTwo, i.uv2.x));
				c += lerp(CLEAR_COLOR, c2, step(maxEdgeTwo, i.uv2.x) - step(minEdgeThree, i.uv2.x));
				c += lerp(CLEAR_COLOR, c3, step(maxEdgeThree, i.uv2.x) - step(screenMax, i.uv2.x));

				c = lerp(c, CLEAR_COLOR, step(_VerticalMask, unit.y));

				return c;
			}
			ENDCG
		}
	}
	Fallback Off
}
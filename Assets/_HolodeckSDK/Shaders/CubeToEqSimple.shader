//  
// Copyright (c) Vulcan Inc. All rights reserved.  
//

Shader "Vulcan/CubemapToEquirectangular Simple" {
	Properties{
		_MainTex_0("Cubemap (RGB)", CUBE) = "" {}
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

			uniform samplerCUBE _MainTex_0;
			uniform float4 _MainTex_0_ST;

			uniform float _VerticalMask;

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord.xy * float2(TWOPI, PI);
				o.uv2 = TRANSFORM_TEX(v.texcoord, _MainTex_0);
				return o;
			}

			fixed4 frag(v2f i) : COLOR
			{
				float theta = i.uv.y;
				float phi = i.uv.x;
				float3 unit = float3(0,0,0);

				unit.x = sin(phi) * sin(theta) * -1;
				unit.y = cos(theta) * -1;
				unit.z = cos(phi) * sin(theta) * -1;

				half4 c = texCUBE(_MainTex_0, unit);
				c = lerp(c, CLEAR_COLOR, step(_VerticalMask, unit.y));

				return c;
			}
			ENDCG
		}
	}
		Fallback Off
}
Shader "Custom/testBasic"
{
	Properties{
		
	}

	SubShader{

		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Lambert vertex:vert addshadow
		#pragma target 3.0
		#pragma glsl
		#pragma multi_compile BRUSH_MARKER_OFF BRUSH_MARKER_ON

		#include "TerrainVolumeUtilities.cginc"

		float4x4 _World2Volume;

		struct Input
		{
			float4 color : COLOR;
			float3 worldPos : POSITION;
			float3 volumeNormal;
			float4 volumePos;
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);

			v.normal = normalize(v.normal);
			o.volumeNormal = v.normal;

			float4 worldPos = mul(_Object2World, v.vertex);
			o.volumePos = mul(_World2Volume, worldPos);
		}

		void surf(Input IN, inout SurfaceOutput o)
		{
			IN.volumeNormal = normalize(IN.volumeNormal);

			half4 materialStrengths = IN.color;
			half materialStrengthsSum = materialStrengths.x + materialStrengths.y + materialStrengths.z + materialStrengths.w;
			materialStrengths /= materialStrengths.w;

			half4 diffuse = materialStrengths;
			o.Albedo = diffuse.rgb;
			o.Alpha = 1.0;
		}

		ENDCG
	}
}

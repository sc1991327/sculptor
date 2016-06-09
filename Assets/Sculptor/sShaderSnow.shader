Shader "BasicSnow2"
{
	Properties{
		_MainTex		("Diffuse (RGB)", 2D)	= "white" {}
		_GlitterTex		("Specular (RGB)", 2D)	= "black" {}
		_BumpTex		("Normal (RGB)", 2D)	= "bump" {}
		_DepthTex		("Depth (R)", 2D)		= "white" {}

		_TexScale				("Texture Scale", Range(0.1, 100.0))			= 1.0
		_ColorScale				("Color Scale", Range(0.0, 1.0))				= 1.0


		_TexWeight				("All Weight", Range(0.0, 1.0))					= 1.0
		_MainTexWeight			("Diffuse Weight", Range(0.0, 100.0))				= 1.0
		_GlitterTexWeight		("Specular Weight", Range(0.0, 100.0))			= 1.0
		_BumpTexWeight			("Normal Weight", Range(0.0, 100.0))				= 1.0
		_DepthTexWeight			("Depth Weight", Range(0.0, 100.0))				= 1.0

		_RampPower("Shade Intensity", Range(0.0, 1.0)) = 1.0
		_Specular("Specular Intensity", Range(0.0, 5.0)) = 1.0
		_Shininess("Shininess", Range(0.01, 1.0)) = 0.08
		_Aniso("Anisotropic Mask", Range(0.0, 1.0)) = 0.0
		_Glitter("Anisotropic Intensity", Range(0.0, 15.0)) = 0.5
		_Depth("Translucency", Range(-2.0, 1.0)) = 1.0
	}

	SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Snow vertex:vert addshadow
		#pragma target 3.0
		#pragma glsl
		#pragma multi_compile BRUSH_MARKER_OFF BRUSH_MARKER_ON

		#include "TerrainVolumeUtilities.cginc"
		#include "SnowCore.cginc"

		sampler2D _MainTex;
		sampler2D _GlitterTex;
		sampler2D _BumpTex;
		sampler2D _DepthTex;

		float4 _MainTex_ST;
		float4 _GlitterTex_ST;
		float4 _BumpTex_ST;
		float4 _DepthTex_ST;

		half _TexScale;
		half _ColorScale;

		half _TexWeight;
		half _MainTexWeight;
		half _GlitterTexWeight;
		half _BumpTexWeight;
		half _DepthTexWeight;

		half 		_RampPower;
		half		_Glitter;
		half		_Aniso;
		half		_Shininess;
		half		_Specular;
		float		_Depth;

		half4		_SpecularColor;
		half		_Speed;
		half		_Density;
		half		_DensityStatic;
		half		_Power;

		//float3 _TexOffset0;
		//float3 _TexOffset1;
		//float3 _TexOffset2;
		//float3 _TexOffset3;

		float4x4 _World2Volume;

		struct Input
		{
			float4 color : COLOR;
			float3 worldPos : POSITION;
			float3 volumeNormal;
			float4 volumePos;
		};

		struct SnowOutput
		{
			half3 	Albedo;
			half3	Normal;
			half3 	Emission;
			half3	Specular;
			half 	Alpha;
			half	Depth;
		};

		inline half4 LightingSnow(SnowOutput s, half3 lightDir, half3 viewDir, half atten)
		{
			half3 H = normalize(lightDir + viewDir);
			half NdotH = max(0, dot(s.Normal, H));
			half NdotL = dot(s.Normal, lightDir);
			half NdotV = dot(s.Normal, viewDir);

			float3 shadow = atten * _LightColor0.rgb;

			half3 albedo = s.Albedo * (_LightColor0.rgb * 2.0);
			float y = NdotL * shadow;
			
			half2 uv_Ramp = half2(_RampPower * NdotV, y);
			half3 ramp = tex2D(_MainTex, uv_Ramp.xy);

			half ssatten = 1.0;

			if (0.0 != _WorldSpaceLightPos0.w) {
				half depth = clamp(s.Depth + _Depth, -1, 1);
				half ssdepth = lerp(NdotL, 1, depth + saturate(dot(s.Normal, -NdotL)));
				ssatten = atten * ssdepth;
				ramp = ramp * ssatten;
			}

			half3 view = mul((float3x3)UNITY_MATRIX_V, s.Normal);
			half3 glitter = frac(0.7 * s.Normal + 9 * s.Specular + _Speed * viewDir * lightDir * view);
			glitter *= (_Density - glitter);
			glitter = saturate(1 - _DensityStatic * (glitter.x + glitter.y + glitter.z));
			glitter = (glitter * _SpecularColor.rgb) * _SpecularColor.a + half3(Overlay(glitter, s.Specular.rgb * _Power)) * (1 - _SpecularColor.a);

			half3 specular = saturate(pow(NdotH, _Shininess * 128.0) * _Specular * glitter);

			half3 anisotropic = max(0, sin(radians((NdotH + _Aniso) * 180))) * ssatten;
			anisotropic = saturate(glitter * anisotropic * _Glitter);

			half4 c;
			c.rgb = ramp * albedo + (anisotropic + specular) * shadow;

			return c;
		}

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);

			// It seems that 'v.normal' (not 'o.volumeNormal') get used 
			// for the lighting so we need to make sure both are normalised.
			v.normal = normalize(v.normal);
			o.volumeNormal = v.normal;

			// Volume-space positions and normals are used for triplanar texturing
			float4 worldPos = mul(_Object2World, v.vertex);
			o.volumePos = mul(_World2Volume, worldPos);

		}

		void surf(Input IN, inout SnowOutput o)
		{
			// adjust tex Scale
			half invTexScale = 1.0 / _TexScale;

			// Interpolation can cause the normal vector to become denomalised.
			IN.volumeNormal = normalize(IN.volumeNormal);

			// Vertex colors coming out of Cubiquity don't actually sum to one
			// (roughly 0.5 as that's where the isosurface is). Make them sum
			// to one, though Cubiquity should probably be changed to do this.

			half4 materialStrengths = IN.color;
			half materialStrengthsSum =
				materialStrengths.x + materialStrengths.y + materialStrengths.z + materialStrengths.w;
			materialStrengths /= materialStrengthsSum;

			float3 texCoords = IN.volumePos.xyz * invTexScale;
			float3 dx = ddx(texCoords);
			float3 dy = ddy(texCoords);

			// Squaring a normalized vector makes the components sum to one. It also seems
			// to give nicer transitions than simply dividing each component by the sum.
			float3 triplanarBlendWeights = IN.volumeNormal * IN.volumeNormal;

			// Sample each of the four textures using triplanar texturing, and
			// additively blend the results using the factors in materialStrengths.

			half4 diffuse = texTriplanar(_MainTex, texCoords, _MainTex_ST, dx, dy, _MainTexWeight * _TexWeight);
			
			half4 specular = texTriplanar(_GlitterTex, texCoords, _GlitterTex_ST, dx, dy, _GlitterTexWeight * _TexWeight);
			half4 normal = texTriplanar(_BumpTex, texCoords, _BumpTex_ST, dx, dy, _BumpTexWeight * _TexWeight);
			half4 depth = texTriplanar(_DepthTex, texCoords, _DepthTex_ST, dx, dy, _DepthTexWeight * _TexWeight);

			diffuse = diffuse * (1 - _ColorScale) + materialStrengths * _ColorScale;

			o.Albedo = diffuse.rgb;
			o.Normal = normal.rgb;
			o.Specular = specular.rgb;
			o.Depth = depth.r;
			o.Alpha = 1.0;
		}
		ENDCG
	}
}

// Blackfire Studio
// Matthieu Ostertag

#ifndef SNOW_LIGHTING_CGINC
#define SNOW_LIGHTING_CGINC
	
	// forward rendering
	inline half4 LightingSnow (SnowOutput s, half3 lightDir, half3 viewDir, half atten)
	{
		half3 H	= normalize(lightDir + viewDir);
		half NdotH = max(0, dot(s.Normal, H));
		half NdotL = dot(s.Normal, lightDir);
		half NdotV = dot(s.Normal, viewDir);
		
			float3 shadow = atten * _LightColor0.rgb;
		
		half3 albedo = s.Albedo * (_LightColor0.rgb * 2.0);
		#ifdef SHADER_API_D3D11
			// Avoid DX11 warning...
			float y = NdotL * shadow;
			half2 uv_Ramp = half2(_RampPower * NdotV, y);
		#else
			half2 uv_Ramp = half2(_RampPower * NdotV, NdotL * shadow);
		#endif

		half3 ramp = tex2D(_Ramp, uv_Ramp.xy);
		
		half ssatten = 1.0;
		
		if (0.0 != _WorldSpaceLightPos0.w) {
			half depth		= clamp(s.Depth + _Depth, -1, 1);
			half ssdepth	= lerp(NdotL, 1, depth + saturate(dot(s.Normal, -NdotL)));
			#if defined(SNOW_BLEND_ADVANCED) || defined(SNOW_BLEND_TEXTURE) || defined(SNOW_BLEND_HEIGHT)
				ssatten = atten * ssdepth * s.Alpha;
			#else
				ssatten = atten * ssdepth;
			#endif
			ramp = ramp * ssatten;
		}
		
		half3 view			= mul((float3x3)UNITY_MATRIX_V, s.Normal);
		half3 glitter		= frac(0.7 * s.Normal + 9 * s.Specular + _Speed * viewDir * lightDir * view);
		glitter 			*= (_Density - glitter);
		glitter 			= saturate(1 - _DensityStatic * (glitter.x + glitter.y + glitter.z));
		glitter				= (glitter * _SpecularColor.rgb) * _SpecularColor.a + half3(Overlay(glitter, s.Specular.rgb * _Power)) * (1 - _SpecularColor.a);
			
		half3 specular		= saturate(pow(NdotH, _Shininess * 128.0) * _Specular * glitter);

		half3 anisotropic	= max(0, sin(radians((NdotH + _Aniso) * 180))) * ssatten;
		anisotropic			= saturate(glitter * anisotropic * _Glitter);
		
		half4 c;
		c.rgb	= ramp * albedo + (anisotropic + specular) * shadow;
		
		return c;
	}

#endif
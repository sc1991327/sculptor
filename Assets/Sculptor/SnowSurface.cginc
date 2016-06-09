// Blackfire Studio
// Matthieu Ostertag

#ifndef SNOW_SURFACE_CGINC
#define SNOW_SURFACE_CGINC

	void vert(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);

		v.normal = normalize(v.normal);
		o.volumeNormal = v.normal;

		float4 worldPos = mul(_Object2World, v.vertex);
		o.volumePos = mul(_World2Volume, worldPos);
	}

	void SnowSurface(Input IN, inout SnowOutput o)
	{
		half3 normal	= UnpackNormal(tex2D(_BumpTex, IN.uv_BumpTex));	// Base Normal map
		half4 albedo	= tex2D(_MainTex, IN.uv_MainTex);
		half3 depth		= tex2D(_DepthTex, IN.uv_MainTex);

		o.Albedo	= albedo.rgb;
		o.Normal	= normal;
		o.Specular	= tex2D(_GlitterTex, IN.uv_GlitterTex);
		o.Depth		= depth.r;
	}

#endif
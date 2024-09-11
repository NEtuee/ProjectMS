Shader "Custom/BackgroundDecalCombine"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
	}

	SubShader
	{
		// No culling or depth
        Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

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
			
			v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

			sampler2D _MainTex;
			sampler2D _DecalTex;
			sampler2D _PerspectiveDepthTexture;
			sampler2D _CharacterTexture;

			fixed4 _ShadowColor;
			float4 _ScreenSize;

			float _SunAngle;
			float _ShadowDistance;
			float _ShadowDistanceRatio;
			float _ShadowDistanceOffset;

			fixed4 sampleToShadow(sampler2D targetTexture, float shadowSample, float2 texcoord)
			{
				float sunAngle = _SunAngle * 0.0174532925 + 3.141592;

				const float near = 0.3f;
				const float far = 1000.0f;

				float clipDistance = far - near;
				float shadowDistance = (shadowSample * clipDistance);
				float additionalShadowDistance = _ShadowDistance * ((shadowDistance) * _ShadowDistanceRatio);
				float2 toUV = (1.0 / _ScreenSize.xy);

				float2 shadowDirection = float2(cos(sunAngle), sin(sunAngle));

				float2 shadowSampleTarget = toUV * (shadowDirection * (_ShadowDistance + additionalShadowDistance * _ShadowDistance));
				float2 shadowOffset = toUV * shadowDirection * _ShadowDistanceOffset;

				float2 uv = texcoord + shadowOffset + shadowSampleTarget;
				if (uv.x < 0.0f || uv.x > 1.0f || uv.y < 0.0f || uv.y > 1.0f)
					return 0.0f;

				fixed4 shadowReSample = tex2D(targetTexture, texcoord + shadowOffset + shadowSampleTarget);
					
				return shadowReSample * _ShadowColor;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D (_MainTex, IN.uv);
				fixed4 decal = tex2D (_DecalTex, IN.uv);
				fixed4 shadowMap = tex2D(_PerspectiveDepthTexture, IN.uv);

				const float near = 0.3f;
				const float far = 1000.0f;

				float clipDistance = far - near;
				float shadowDistance = ((1.0 - shadowMap.r) * clipDistance);

				c.rgb = lerp(c.rgb, decal.rgb, decal.a * (1.0 - step(25.0, shadowDistance)));

				fixed4 characterShadow = sampleToShadow(_CharacterTexture, 1.0 - shadowMap, IN.uv);
				fixed4 shadowdedBackground = lerp(c, characterShadow, characterShadow.a);

				return shadowdedBackground;
			}
		ENDCG
		}
	}
}
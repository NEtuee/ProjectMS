
Shader "Custom/SpriteShadowScreenShader"
{
	Properties
	{
		_CharacterTexture ("Character Texture", 2D) = "white" {}
		_MainTex ("Background Texture", 2D) = "white" {}
        _ShadowMapTexture ("ShadowMap Texture", 2D) = "white" {}

	    _SunAngle ("Sun Angle Degree", Range(0.0, 360.0)) = 0.0
		_ShadowDistance ("Shadow Distance", Range(0.1, 3.0)) = 0.1
		_ShadowDistanceRatio ("Shadow Distance Ratio", Range(0.0, 10.0)) = 0.0

		_ScreenSize("Screen Size", Vector) = (1024, 1024, 0, 0)
		_ShadowDistanceOffset("Shadow Distance Offset", Range(0.0, 10.0)) = 0.0

		_ShadowColor ("ShadowColor", Color) = (0,0,0,1)

		[Space][Space][Space]
		_Brightness ("Brightness", Range(0.0, 5.0)) = 1.0
		_Saturation ("Saturation", Range(0.0, 1.0)) = 1.0
		_ColorTint ("Color Tint", Color) = (1,1,1,1)

		[Space][Space][Space]
		_BlurSize ("Blur Size", Range(0.0, 2.0)) = 0.0
		_MultiSampleDistance ("Multi Sample Distance", Range(0.0, 5.0)) = 0.0
		_MultiSampleColorTintRight ("Multi Sample Color Tint Right", Color) = (1,1,1,1)
		_MultiSampleColorTintLeft ("Multi Sample Color Tint Left", Color) = (1,1,1,1)

		[Space][Space][Space]
		_FogRate ("Fog Rate", Range(0.0, 1.0)) = 0.0
		_FogStrength ("Fog Strength", Range(0.0, 1.0)) = 1.0
		_FogColor ("Fog Color", Color) = (1,1,1,1)

		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				float2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;
			fixed4 _ShadowColor;

			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = UnityObjectToClipPos(IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _CharacterTexture;
			sampler2D _MainTex;
			sampler2D _ShadowMapTexture;

			sampler2D _AlphaTex;

			float _SunAngle;
			float _ShadowDistance;
			float _ShadowDistanceRatio;
			float _AlphaSplitEnabled;
			float _ShadowDistanceOffset;
			float4 _ScreenSize;

			float _Brightness;
			float _Saturation;
			fixed4 _ColorTint;

			float _BlurSize;
			float _MultiSampleDistance;
			fixed4 _MultiSampleColorTintRight;
			fixed4 _MultiSampleColorTintLeft;


			float _FogRate;
			float _FogStrength;
			fixed4 _FogColor;

			half4 _MainTex_TexelSize;

			fixed4 SampleSpriteTexture (sampler2D sampleTexture, float2 uv)
			{
				fixed4 color = tex2D (sampleTexture, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 sampleBackground(float2 texcoord)
			{
				fixed4 backgroundSample = SampleSpriteTexture(_MainTex, texcoord);
				fixed4 shadowMap = SampleSpriteTexture(_ShadowMapTexture, texcoord);

				//sun angle to radian
				float sunAngle = _SunAngle * 0.0174532925 + 3.141592;
				float additionalShadowDistance = _ShadowDistance * ((1.0 - shadowMap.r) * _ShadowDistanceRatio);
				float2 toUV = (1.0 / _ScreenSize.xy);

				float2 shadowDirection = float2(cos(sunAngle),sin(sunAngle));

				float2 shadowSampleTarget = toUV * (shadowDirection * (_ShadowDistance + additionalShadowDistance));
				float2 shadowOffset = toUV * shadowDirection * _ShadowDistanceOffset;

				fixed4 characterShadowSample = _ShadowColor * SampleSpriteTexture(_CharacterTexture, texcoord + shadowOffset + shadowSampleTarget).a;

				backgroundSample.rgb *= backgroundSample.a;
				backgroundSample = (backgroundSample * (1.0 - characterShadowSample.a)) + characterShadowSample;

				return backgroundSample;
			}

			fixed4 bluredBackgroundSample(float2 texcoord)
			{
				fixed4 backgroundSample = fixed4(0, 0, 0, 0);
                float2 offset = _MainTex_TexelSize.xy * _BlurSize;
				const float _preComputeKernel[9] = 
				{
                	0.000229,
                	0.005977,
                	0.060598,
                	0.241732,
                	0.382928,
                	0.241732,
                	0.060598,
                	0.005977,
                	0.000229
            	};

				for (int k = 0; k < 9; k++) 
				{
    			    float2 uvOffset = float2((k - 4) * offset.x, (k - 4) * offset.y);
    			    backgroundSample += sampleBackground(texcoord + uvOffset) * _preComputeKernel[k];
    			}

				return backgroundSample;
			}

			fixed4 allTogether(float2 texcoord)
			{
				float2 offset = _MainTex_TexelSize.xy * _MultiSampleDistance;
				offset.y = 0.0;

				fixed4 backgroundSample = bluredBackgroundSample(texcoord);

				backgroundSample += (bluredBackgroundSample(texcoord - offset) * _MultiSampleColorTintLeft) * _MultiSampleColorTintLeft.a;
				backgroundSample += (bluredBackgroundSample(texcoord + offset) * _MultiSampleColorTintRight) * _MultiSampleColorTintRight.a;

				backgroundSample /= 3.0;

				fixed4 characterColor = SampleSpriteTexture (_CharacterTexture, texcoord);
				if(characterColor.a != 0.0)
				{
					characterColor.rgb *= characterColor.a;
					backgroundSample.rgb *= backgroundSample.a;
					return (backgroundSample * (1.0 - characterColor.a)) + characterColor;
				}


				return backgroundSample;
			}

			fixed4 onlyShadow(v2f IN)
			{
				fixed4 characterColor = SampleSpriteTexture (_CharacterTexture, IN.texcoord);
				fixed4 backgroundSample = SampleSpriteTexture(_MainTex, IN.texcoord);
				fixed4 shadowMap = SampleSpriteTexture(_ShadowMapTexture, IN.texcoord);

				if(characterColor.a != 0.0 || shadowMap.r == 0.0)
					return fixed4(0.0, 0.0, 0.0 ,0.0);

				float sunAngle = _SunAngle * 0.0174532925 + 3.141592;
				float additionalShadowDistance = _ShadowDistance * ((1.0 - shadowMap.r) * _ShadowDistanceRatio);
				float2 toUV = (1.0 / _ScreenSize.xy);

				float2 shadowDirection = float2(cos(sunAngle),sin(sunAngle));

				float2 shadowSampleTarget = toUV * (shadowDirection * (_ShadowDistance + additionalShadowDistance));
				float2 shadowOffset = toUV * shadowDirection * _ShadowDistanceOffset;

				float characterShadowSample = SampleSpriteTexture(_CharacterTexture, IN.texcoord + shadowOffset + shadowSampleTarget).a;

				return _ShadowColor * characterShadowSample;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 resultColor = allTogether(IN.texcoord);

				//brigtness
				resultColor.rgb *= _Brightness;

				//grayscale
				float gray = dot(resultColor.rgb, fixed3(0.21f, 0.72f, 0.07f));
				resultColor.rgb = lerp(resultColor.rgb, fixed3(gray,gray,gray), 1.0 - _Saturation);

				//color tint
				resultColor.rgb *= _ColorTint;

				//fog
				float fogRate = smoothstep(0.0, 1.0, (IN.texcoord.y - _FogStrength) * (1.0 / ((1.0 - _FogStrength) * _FogRate)));
				resultColor.rgb *= (fogRate + ((1.0 - fogRate) * (1.0 - _FogColor.a) ) );
				resultColor.rgb += _FogColor.rgb * (1.0 - fogRate) * _FogColor.a;

				return resultColor;
			}
		ENDCG
		}
	}
}
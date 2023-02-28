
Shader "Custom/SpriteShadowScreenShader"
{
	Properties
	{
		_CharacterTexture ("Character Texture", 2D) = "white" {}
		_BackgroundTexture ("Background Texture", 2D) = "white" {}
        _ShadowMapTexture ("ShadowMap Texture", 2D) = "white" {}

	    _SunAngle ("Sun Angle Degree", Range(0.0, 360.0)) = 0.0
		_ShadowDistance ("Shadow Distance", Range(0.1, 3.0)) = 0.1
		_ShadowDistanceRatio ("Shadow Distance Ratio", Range(0.0, 10.0)) = 0.0

		_ScreenSize("Screen Size", Vector) = (1024, 1024, 0, 0)

		_ShadowColor ("ShadowColor", Color) = (0,0,0,1)
		_Color ("Tint", Color) = (1,1,1,1)

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
			sampler2D _BackgroundTexture;
			sampler2D _ShadowMapTexture;

			sampler2D _AlphaTex;

			float _SunAngle;
			float _ShadowDistance;
			float _ShadowDistanceRatio;

			float _AlphaSplitEnabled;

			float4 _ScreenSize;

			float _toRadian = 0.0174532925;

			fixed4 SampleSpriteTexture (sampler2D sampleTexture, float2 uv)
			{
				fixed4 color = tex2D (sampleTexture, uv);

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled)
					color.a = tex2D (_AlphaTex, uv).r;
#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

				return color;
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				//IN.color;
				fixed4 characterColor = SampleSpriteTexture (_CharacterTexture, IN.texcoord);
				fixed4 backgroundSample = SampleSpriteTexture(_BackgroundTexture, IN.texcoord);
				fixed4 shadowMap = SampleSpriteTexture(_ShadowMapTexture, IN.texcoord);

				if(characterColor.a != 0.0 || shadowMap.r == 0.0)
				{
					characterColor.rgb *= characterColor.a;
					backgroundSample.rgb *= backgroundSample.a;
					return (backgroundSample * (1.0 - characterColor.a)) + characterColor;
				}

				float sunAngle = _SunAngle * 0.0174532925 + 3.141592;
				float additionalShadowDistance = _ShadowDistance * ((1.0 - shadowMap.r) * _ShadowDistanceRatio);
				float2 toUV = (1.0 / _ScreenSize.xy);
				float2 shadowOffset = toUV * ((float2(cos(sunAngle),sin(sunAngle)) * (_ShadowDistance + additionalShadowDistance)));

				float characterShadowSample = SampleSpriteTexture(_CharacterTexture, IN.texcoord + shadowOffset).a;
				float shadowAlpha = _ShadowColor.a;
				fixed4 shadowColorInverse = -_ShadowColor;
				shadowColorInverse.a = shadowAlpha;

				// float backgroundShadowSample = backgroundSample.r - SampleSpriteTexture(_BackgroundTexture, IN.texcoord + shadowOffset).r;
				// backgroundShadowSample = clamp(backgroundShadowSample,0.0,1.0);
				// if(backgroundShadowSample > 0.0)
				// 	characterShadowSample = 1.0;

				backgroundSample.rgb *= backgroundSample.a;
				backgroundSample *= fixed4(1.0,1.0,1.0,1.0) + shadowColorInverse * (characterShadowSample);

				return backgroundSample;
				// c.rgb *= c.a;
				// return c;
			}
		ENDCG
		}
	}
}
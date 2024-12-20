// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


Shader "Custom/SpriteShadowScreenShader"
{
	Properties
	{
		_CharacterTexture("Character Texture", 2D) = "white" {}
		_MainTex("Background Texture", 2D) = "white" {}

		_PerspectiveDepthTexture("Perspective Depth Texture", 2D) = "white" {}
		_CrossFillFactor("Cross Fill Factor Test",Range(0.0, 2.0)) = 0.0
		_CrossWidth("Cross Width", Range(0.0, 2.0)) = 0.15
		_CrossHeight("Cross Height", Range(0.0, 2.0)) = 0.15
		_CrossTileSize("Cross Tile Size", Range(0.0, 1.0)) = 0.025
		_CenterUV("Center UV", Vector) = (0.5, 0.5, 0, 0)
		
		_SunAngle("Sun Angle Degree", Range(0.0, 360.0)) = 0.0
		_ShadowDistance("Shadow Distance", Range(0.1, 3.0)) = 1.07
		_ShadowDistanceRatio("Shadow Distance Ratio", Range(0.0, 10.0)) = 2.58

		_ScreenSize("Screen Size", Vector) = (1024, 1024, 0, 0)
		_ShadowDistanceOffset("Shadow Distance Offset", Range(-100.0, 100.0)) = -33.2

		_ShadowColor("ShadowColor", Color) = (0,0,0,1)

		[Space][Space][Space]
		_ImpactFrame("Impact Frame", Range(0.0, 1.0)) = 1.0
		_Brightness("Brightness", Range(0.0, 5.0)) = 1.0
		_Saturation("Saturation", Range(0.0, 1.0)) = 1.0
		_Bloom("Bloom", Range(0.0, 2.0)) = 0.0
		_Vignette("Vignette", Range(0.0, 1.0)) = 0.0
		_Contrast("Background Contrast", Range(0.0, 1.0)) = 1.0
		_ContrastTarget("Background Contrast Target", Range(0.0, 1.0)) = 0.5
		_ColorTint("Character Color Tint", Color) = (1,1,1,1)
		_BackgroundColorTint("BackgroundColor", Color) = (1,1,1,1)
		_ForwardScreenColorTint("ForwardScreenColor", Color) = (1,1,1,1)

		[Space][Space][Space]
		_ShadowBlurExponential("Shadow Blur Exp", Range(0.0, 1.0)) = 0.0
		_BlurSize("Blur Size", Range(0.0, 32.0)) = 0.0
		_MultiSampleDistance("Multi Sample Distance", Range(0.0, 5.0)) = 0.0
		_MultiSampleColorTintRight("Multi Sample Color Tint Right", Color) = (1,1,1,1)
		_MultiSampleColorTintLeft("Multi Sample Color Tint Left", Color) = (1,1,1,1)

		[Space][Space][Space]
		_FogRate("Fog Rate", Range(0.0, 1.0)) = 0.0
		_FogStrength("Fog Strength", Range(0.0, 1.0)) = 1.0
		_FogColor("Fog Color", Color) = (1,1,1,1)

		[Space][Space][Space]
		_Curvature ("Curvature", Range(0, 2)) = 0.0
		_Scanline ("Scanline", Range(0, 1)) = 0.0

		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
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
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
					float3 worldPosition : TEXCOORD1;
				};

				fixed4 _Color;
				fixed4 _ShadowColor;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					OUT.worldPosition = mul(unity_ObjectToWorld, IN.vertex).xyz;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _CharacterTexture;
				sampler2D _MainTex;
				sampler2D _AlphaTex;
				sampler2D _PerspectiveDepthTexture;
				sampler2D _ForwardScreenTexture;

				float _CrossFillFactor;
				float _CrossWidth;
				float _CrossHeight;
				float _CrossTileSize;
				float4 _CenterUV;

				float _SunAngle;
				float _ShadowDistance;
				float _ShadowDistanceRatio;
				float _AlphaSplitEnabled;
				float _ShadowDistanceOffset;
				float4 _ScreenSize;

				float _ImpactFrame;
				float _Brightness;
				float _Saturation;
				float _Bloom;
				float _Vignette;
				float _Contrast;
				float _ContrastTarget;
				fixed4 _ColorTint;
				fixed4 _BackgroundColorTint;
				fixed4 _ForwardScreenColorTint;

				float _BlurSize;
				float _ShadowBlurExponential;
				float _MultiSampleDistance;
				fixed4 _MultiSampleColorTintRight;
				fixed4 _MultiSampleColorTintLeft;

				float4 _RealCameraSize;

				float _FogRate;
				float _FogStrength;
				fixed4 _FogColor;

				float _Distortion;
				float _Curvature;
				float _Scanline;

				half4 _MainTex_TexelSize;


				fixed4 SampleSpriteTexture(sampler2D sampleTexture, float2 uv)
				{
					fixed4 color = tex2D(sampleTexture, uv);

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				static const float2 resolution = float2(1024, 1024);

				float easeOutCubic(float start, float end, float value)
				{
					end -= start;
					return start + (end * (1.0 - pow(1.0 - value, 3.0)));
				}

				float easeOutQuint(float start, float end, float value)
				{
					end -= start;
					return start + (end * (1 - pow(1 - value, 5)));
				}

				/*
				1. �׸��ڴ� ������ ������ �ִ�.
				2. � ���� ���ø� ���� ���� �������� �����Ѵ�.
				3. ���� ���� �������� �׸��ڰ� �ȳ��;��Ѵ�.
				*/
				fixed4 drawCharacter(float2 texcoord)
				{
					fixed4 characterSample = SampleSpriteTexture(_CharacterTexture, texcoord) * _ColorTint;

					return fixed4(characterSample.xyz * (1.0 - _ImpactFrame), characterSample.w);
				}



				fixed4 sampleOtherBackground(float2 texcoord, float3 worldPosition, bool ignoreOtherBackground)
				{
					if(ignoreOtherBackground)
						return fixed4(0.0,0.0,0.0,0.0);

					fixed4 resultColor = fixed4(0.4,0.4,0.5,1.0);

					{
						float squareSize = 0.1f;
						float gapSize = 2.0;

						float3 worldPos = worldPosition;
                		worldPos = abs(fmod(worldPos, squareSize + gapSize));
                		bool inSquare = (worldPos.x < squareSize) && (worldPos.y < squareSize);
						if(inSquare)
							resultColor = fixed4(0.7,0.7,0.7,1.0);
					}

					{
						float squareSize = 0.07f;
						float gapSize = 2.5f;

						float3 worldPos = worldPosition + _WorldSpaceCameraPos * -0.3f;
                		worldPos = abs(fmod(worldPos, squareSize + gapSize));
                		bool inSquare = (worldPos.x < squareSize) && (worldPos.y < squareSize);
						if(inSquare)
							resultColor = fixed4(0.5,0.5,0.5,1.0);
					}

					float2 offset = float2(5.0f, 5.0f);
					float2 vignetteRatio = 1.0 - ((_RealCameraSize.xy + offset) / resolution);
					float2 uv = texcoord.xy - vignetteRatio * 0.5f;
					uv /= 1.0f - vignetteRatio;
    				uv *=  1.0 - uv.yx;
    				float vig = uv.x*uv.y * 7.0;
    				vig = pow(vig, 0.27);

    				resultColor.xyz *= vig;

					return resultColor;
				}

				


				fixed4 bluredBackgroundSample(sampler2D backgroundTexture, float2 texcoord, float3 worldPosition, bool firstBackground)
				{
					float Pi = 6.28318530718; // Pi*2
					float3 backgroundTint = (firstBackground == false ? _ForwardScreenColorTint : _BackgroundColorTint).rgb;

					// GAUSSIAN BLUR SETTINGS {{{
					float Directions = 6.0; // BLUR DIRECTIONS (Default 16.0 - More is better but slower)
					float Quality = 4.0; // BLUR QUALITY (Default 4.0 - More is better but slower)
					float Size = _BlurSize; // BLUR SIZE (Radius)
					// GAUSSIAN BLUR SETTINGS }}}

					float2 Radius = Size / resolution;

					float4 Color = float4(0,0,0,0);


					float2 directionVector = texcoord - _CenterUV.xy;
					float crossLength = _CrossFillFactor * 2.5;
					float cosAngle, sinAngle;// = cos(_CrossFillFactor * 0.3);
    				//float sinAngle = sin(_CrossFillFactor * 0.3);
					sincos(_CrossFillFactor * 0.3, sinAngle, cosAngle);

    				float2 rotatedPos = float2(cosAngle * directionVector.x - sinAngle * directionVector.y, sinAngle * directionVector.x + cosAngle * directionVector.y) * 2.5;

					float distanceFromCenterX = abs(rotatedPos.x);
					float distanceFromCenterY = abs(rotatedPos.y);
					float widthX = max(0.01 - distanceFromCenterY / crossLength * 0.01, 0);
					float widthY = max(0.01 - distanceFromCenterX / crossLength * 0.01, 0);

					bool ignoreOtherBackground = firstBackground == false;
					bool isInCross = (distanceFromCenterX < widthX && distanceFromCenterY < crossLength) || (distanceFromCenterY < widthY && distanceFromCenterX < crossLength);
					if(isInCross)
					{
						Color = sampleOtherBackground(texcoord, worldPosition,ignoreOtherBackground); 
					}
					else
					{
						float2 offset = -_CenterUV.xy + float2(_CrossTileSize,_CrossTileSize) * 0.5 + float2(0.5, 0.5);
						float2 tileIndex = floor((texcoord + offset ) / _CrossTileSize);
						float2 tileCenter = (tileIndex * _CrossTileSize);

						float distance = length(tileCenter - float2(0.5, 0.5));
						float2 tilePos = fmod(abs(texcoord + offset), _CrossTileSize) - 0.5 * _CrossTileSize;
	
						tilePos.x /= _CrossWidth;
    					tilePos.y /= _CrossHeight;

						float fillFactor = _CrossFillFactor - distance * 2;
    					float value = pow(abs(tilePos.x), fillFactor) + pow(abs(tilePos.y), fillFactor);

    					if (value <= 1.0)
						{
							Color = sampleOtherBackground(texcoord, worldPosition,ignoreOtherBackground);  
						}
    					else
						{
							fixed4 backgroundSample = SampleSpriteTexture(backgroundTexture, texcoord);
							Color = backgroundSample * (ignoreOtherBackground ? _ForwardScreenColorTint : _BackgroundColorTint);

							//Blur calculations
							float3 blur = float3(0, 0, 0);
							for (float d = 0.0; d < Pi; d += Pi / Directions)
							{
								float cosineLoop, sineLoop;// = cos(d);
								//float sineLoop = sin(d);
								sincos(d, sineLoop, cosineLoop);
								for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
								{
									blur += SampleSpriteTexture(backgroundTexture, texcoord + float2(cosineLoop, sineLoop) * Radius * i).rgb;
								}
							}
							blur /= Quality * Directions;
							blur *= backgroundTint;

							Color.rgb += blur;
							Color.rgb *= 0.5;
		
							float brightness = dot(Color.xyz, float3(0.2126f, 0.7152f, 0.0722));
							if (brightness > 0.5)
							{
								float3 bloom = float3(0.0, 0.0, 0.0);
								float bloomBlurRadius = (brightness * 8.0f * _Bloom) / resolution;
								for (float d = 0.0; d < Pi; d += Pi / Directions)
								{
									float cosineLoop, sineLoop;// = cos(d);
									//float sineLoop = sin(d);
									sincos(d, sineLoop, cosineLoop);
									for (float i = 1.0 / Quality; i <= 1.0; i += 1.0 / Quality)
									{
										bloom += SampleSpriteTexture(backgroundTexture, texcoord + float2(cosineLoop, sineLoop) * bloomBlurRadius * i).rgb * backgroundTint;
									}
								}
								bloom /= Quality * Directions;
		
								Color.rgb += bloom;
								Color.rgb *= 0.5;
							}

							// contrast
							Color.xyz = ((Color.xyz - _ContrastTarget) * max(_Contrast, 0.0)) + _ContrastTarget;
						}
					}

					
					//impact frame
					Color += fixed4(_ImpactFrame, _ImpactFrame, _ImpactFrame, _ImpactFrame);
					Color.a *= firstBackground ? _ImpactFrame : 1.0f - _ImpactFrame;

					return Color;
				}

				fixed4 allTogether(float2 texcoord, float3 worldPosition)
				{
					fixed4 backgroundSample = bluredBackgroundSample(_MainTex, texcoord, worldPosition, true);
					fixed4 characterSample = drawCharacter(texcoord);

					fixed4 forwardScreenSample = bluredBackgroundSample(_ForwardScreenTexture, texcoord, worldPosition, false);
					fixed4 mixed2 = lerp(backgroundSample, characterSample, characterSample.a);
					fixed4 mixed3 = lerp(mixed2, forwardScreenSample, forwardScreenSample.a);
					return mixed3;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					float2 center = float2(0.5, 0.5);
            		float2 coord = IN.texcoord - center;
            		coord *= 1.0 + _Curvature * dot(coord, coord);
            		coord += center;

					fixed4 resultColor = allTogether(coord,IN.worldPosition);

					// Scanline
					float time = _Time.y * 0.01f;
            		resultColor.rgb *= 1.0 + _Scanline * sin((coord.y + time) * 1200.0);

					//brigtness
					resultColor.rgb *= _Brightness; 

					//grayscale
					float gray = dot(resultColor.rgb, fixed3(0.21f, 0.72f, 0.07f));
					resultColor.rgb = lerp(resultColor.rgb, fixed3(gray,gray,gray), 1.0 - _Saturation);

					//fog
					float fogRate = smoothstep(0.0, 1.0, (coord.y - _FogStrength) * (1.0 / ((1.0 - _FogStrength) * _FogRate)));
					resultColor.rgb *= (fogRate + ((1.0 - fogRate) * (1.0 - _FogColor.a)));
					resultColor.rgb += _FogColor.rgb * (1.0 - fogRate) * _FogColor.a;

					//vignette
					float2 offset = float2(5.0f, 5.0f);
					float2 vignetteRatio = 1.0 - ((_RealCameraSize.xy + offset) / resolution);
					float2 uv = coord.xy - vignetteRatio * 0.5f;
					uv /= 1.0f - vignetteRatio;
    				uv *=  1.0 - uv.yx;
    				float vig = uv.x*uv.y * 8.0;
    				vig = pow(vig, _Vignette);
    				resultColor.xyz *= vig;

					return resultColor;
				}
				ENDCG
			}

	}
}
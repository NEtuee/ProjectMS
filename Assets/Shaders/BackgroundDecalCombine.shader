Shader "Custom/BackgroundDecalCombine"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_OutlineColor ("OutlineColor", Color) = (1,1,1,1)
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

			float4x4 _ClipToView;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 viewSpaceDir : TEXCOORD2;
            };
			
			v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
				o.viewSpaceDir = mul(_ClipToView, o.vertex).xyz;
                return o;
            }

			sampler2D _MainTex;
			sampler2D _DecalTex;
			sampler2D _NormalTexture;
			sampler2D _PerspectiveDepthTexture;
			sampler2D _CharacterTexture;

			fixed4 _ShadowColor;
			fixed4 _OutlineColor;
			float4 _ScreenSize;
			
			
			half4 _MainTex_TexelSize;

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

			float4 alphaBlend(float4 top, float4 bottom)
			{
				float3 color = (top.rgb * top.a) + (bottom.rgb * (1 - top.a));
				float alpha = top.a + bottom.a * (1 - top.a);

				return float4(color, alpha);
			}
			
			// https://roystan.net/articles/outline-shader/
			fixed4 sampleBackground(sampler2D backgroundTexture, sampler2D depthTexture, sampler2D normalTexture, float2 texcoord, float3 viewSpaceDir)
			{
				float _Scale = 1.0;
				float _DepthThreshold = 1.5;
				float _NormalThreshold = 0.3;
				float _DepthNormalThreshold = 1.5;
				float _DepthNormalThresholdScale = 6.1;	

				float halfScaleFloor = floor(_Scale * 0.5);
				float halfScaleCeil = ceil(_Scale * 0.5);

				float2 bottomLeftUV = texcoord - float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleFloor;
				float2 topRightUV = texcoord + float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y) * halfScaleCeil;  
				float2 bottomRightUV = texcoord + float2(_MainTex_TexelSize.x * halfScaleCeil, -_MainTex_TexelSize.y * halfScaleFloor);
				float2 topLeftUV = texcoord + float2(-_MainTex_TexelSize.x * halfScaleFloor, _MainTex_TexelSize.y * halfScaleCeil);

				float depth0 = tex2D(depthTexture, bottomLeftUV).r;
				float depth1 = tex2D(depthTexture, topRightUV).r;
				float depth2 = tex2D(depthTexture, bottomRightUV).r;
				float depth3 = tex2D(depthTexture, topLeftUV).r;

				float normal0 = tex2D(normalTexture, bottomLeftUV).r;
				float normal1 = tex2D(normalTexture, topRightUV).r;
				float normal2 = tex2D(normalTexture, bottomRightUV).r;
				float normal3 = tex2D(normalTexture, topLeftUV).r;

				float3 viewNormal = normal0 * 2 - 1;
				float NdotV = 1 - dot(viewNormal, -viewSpaceDir);

				float normalThreshold01 = saturate((NdotV - _DepthNormalThreshold) / (1 - _DepthNormalThreshold));
				float normalThreshold = normalThreshold01 * _DepthNormalThresholdScale + 1;

				float depthThreshold = _DepthThreshold * depth0 * normalThreshold;

				float depthFiniteDifference0 = depth1 - depth0;
				float depthFiniteDifference1 = depth3 - depth2;

				float3 normalFiniteDifference0 = normal1 - normal0;
				float3 normalFiniteDifference1 = normal3 - normal2;

				float edgeDepth = sqrt(pow(depthFiniteDifference0, 2) + pow(depthFiniteDifference1, 2)) * 20000;
				edgeDepth = edgeDepth > depthThreshold ? 1 : 0;	

				float edgeNormal = sqrt(dot(normalFiniteDifference0, normalFiniteDifference0) + dot(normalFiniteDifference1, normalFiniteDifference1));
				edgeNormal = edgeNormal > normalThreshold ? 1 : 0;

				float finalEdgeDetection = max(edgeDepth, edgeNormal);

				float4 edgeColor = float4(_OutlineColor.rgb,_OutlineColor.a * finalEdgeDetection);
				float4 color = tex2D(backgroundTexture, texcoord);

				return alphaBlend(edgeColor, color);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = sampleBackground (_MainTex, _PerspectiveDepthTexture, _NormalTexture, IN.uv, IN.viewSpaceDir);
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
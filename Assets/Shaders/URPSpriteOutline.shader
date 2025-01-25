Shader "Custom/PixelOutlineShader"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Name "SpriteLitOutline"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float4 _MainTex_TexelSize; 
            float4 _Color;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            
                float2 perPixelUV = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
            
                // Multiple offsets for thicker outlines
                float2 offsets[4] = {
                    float2(-1,  0),  // Left
                    float2( 1,  0),  // Right
                    float2( 0, -1),  // Down
                    float2( 0,  1),  // Up
                };
            
                float alphaThreshold = 0.001;
                float outline = 0.0;
            
                // Sample around the pixel for thicker outlines
                int thickness = 2;
                for(int i = 1; i <= thickness; ++i)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        float2 offset = offsets[j] * perPixelUV * i;
                        float4 neighbor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv + offset);
                        outline += step(alphaThreshold, neighbor.a);

                        if (outline > 0.0)
                            break;
                    }
                }
            
                // Flashing effect
                float blink = (sin(_Time.y * 12.28) * 0.5 + 0.5); // Oscillates between 0 and 1
                float4 blinkColor = lerp(float4(1, 1, 1, 1), _Color, blink); // Interpolate between white and _Color
            
                // Outline condition
                if (outline > 0.0 && original.a < alphaThreshold)
                    return blinkColor;
            
                return float4(blinkColor.rgb, original.a);
            }

            ENDHLSL
        }
    }
}
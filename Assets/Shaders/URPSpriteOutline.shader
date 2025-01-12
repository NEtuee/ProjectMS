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

            float  _OutlineThickness;
            float4 _MainTex_TexelSize; 
            float4 _Color;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (Varyings i) : SV_Target
            {
                float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);

                float2 perPixelUV = float2(_MainTex_TexelSize.x, _MainTex_TexelSize.y);
                float2 offset     = perPixelUV;

                float4 left  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(-offset.x,  0));
                float4 right = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(+offset.x,  0));
                float4 down  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, -offset.y));
                float4 up    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv + float2(0, +offset.y));

                float alphaThreshold = 0.001;

                float outline = 0.0;
                outline += step(alphaThreshold, left.a);
                outline += step(alphaThreshold, right.a);
                outline += step(alphaThreshold, down.a);
                outline += step(alphaThreshold, up.a);

                float hue = frac(_Time.y * 2.0f);

                if(outline > 0.0 && original.a < alphaThreshold)
                    return _Color;

                return float4(_Color.rgb, original.a);
            }
            ENDHLSL
        }
    }
}
Shader "Unlit/Toon"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        [HDR] _AmbientColor("Ambient Color", Color) = (0.4, 0.4, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"
        "LightMode" = "ForwardBase"}
        LOD 100
        

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldNormal : NORMAL;
                SHADOW_COORDS(2)
                
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            float4 _Color;
            float4 _AmbientColor;
            

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float NdotL = dot(_WorldSpaceLightPos0, normal);
                float lightIntensity = NdotL > 0 ? 1 : 0;
                float4 light = lightIntensity * _LightColor0;
                
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);


                return col * _Color * (_AmbientColor + light);
            }
            ENDCG
        }
        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
        
    }
}

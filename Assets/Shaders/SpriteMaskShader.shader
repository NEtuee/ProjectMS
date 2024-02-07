 
Shader "Custom/SpriteMaskShader"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)          
        _Cutoff("Cutoff", float) = 0.5
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
 
            Stencil
            {
                Ref 1
                Comp Greater
                Pass Replace
                ReadMask 255
                WriteMask 255
            }
 
            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
 
            Pass
            {
                Name "Default"
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0
 
                #include "UnityCG.cginc"
                #include "UnityUI.cginc"
 
                #pragma multi_compile __ UNITY_UI_CLIP_RECT
                #pragma multi_compile __ UNITY_UI_ALPHACLIP
 
                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
 
                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    UNITY_VERTEX_OUTPUT_STEREO
                };
 
                fixed4 _Color;
                fixed4 _TextureSampleAdd;
                float4 _ClipRect;
 
                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.vertex = UnityObjectToClipPos(v.vertex);
 
                    OUT.texcoord = v.texcoord;
 
                    OUT.color = v.color * _Color;
                    return OUT;
                }
 
                sampler2D _MainTex;
                float _Cutoff;
 
                fixed4 frag(v2f IN) : SV_Target
                {
                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                    clip(color.a - _Cutoff);
 
                    return color;
                }
            ENDCG
            }
        }
}
 
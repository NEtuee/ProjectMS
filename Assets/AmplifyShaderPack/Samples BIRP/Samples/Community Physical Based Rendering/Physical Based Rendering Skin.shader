// Made with Amplify Shader Editor v1.9.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AmplifyShaderPack/Community/Physical Based Rendering Skin"
{
	Properties
	{
		[Enum(Front,2,Back,1,Both,0)] _Cull( "Render Face", Int ) = 2
		[Header(ALPHA)][ToggleUI] _GlancingClipMode( "Enable Clip Glancing Angle", Float ) = 0
		_AlphaCutoffBias( "Alpha Cutoff Bias", Range( 0, 1 ) ) = 1
		[Header(COLOR)] _BaseColor( "Base Color", Color ) = ( 1, 1, 1 )
		_Saturation( "Saturation", Range( 0, 1 ) ) = 0
		_Brightness( "Brightness", Range( 0, 2 ) ) = 1
		[Header(SURFACE INPUTS)][SingleLineTexture] _MainTex( "BaseColor Map", 2D ) = "white" {}
		_MainUVs( "Main UVs", Vector ) = ( 1, 1, 0, 0 )
		[Enum(MSO,0,MRO,1)] _MainMaskType( "Main Mask Type", Float ) = 0
		[SingleLineTexture] _MainMaskMap( "Main Mask Map", 2D ) = "white" {}
		_MetallicStrength( "Metallic Strength", Range( 0, 1 ) ) = 0.15
		_SmoothnessStrength( "Smoothness Strength", Range( 0, 1 ) ) = 0.5
		_OcclusionStrengthAO( "Occlusion Strength", Range( 0, 1 ) ) = 0
		[SingleLineTexture] _SpecularMap( "Specular Map", 2D ) = "white" {}
		[HideInInspector] _MaskClipValue2( "Mask Clip Value", Float ) = 0.5
		_SpecularColor( "Specular Color", Color ) = ( 0, 0, 0, 1 )
		[Normal][SingleLineTexture][Space(10)] _BumpMap( "Normal Map", 2D ) = "bump" {}
		_NormalStrength( "Normal Strength", Float ) = 1
		[Header(EMISSION)][ToggleUI] _EmissionEnable( "ENABLE EMISSION", Float ) = 0
		[Toggle] _AlbedoAffectEmissive( "BaseColor Affect Emissive", Float ) = 0
		[HDR][SingleLineTexture] _EmissionMap( "Emissive Color Map", 2D ) = "black" {}
		[HDR] _EmissionColor( "Emissive Color", Color ) = ( 0, 0, 0, 0 )
		_EmissiveIntensity( "Emissive Intensity", Float ) = 1
		[Header(GEOMETRIC SHADOWING)] _ShadowStrength( "Shadow Strength", Range( 0, 1 ) ) = 0.1
		_ShadowOffset( "Shadow Offset", Range( -1, 1 ) ) = -0.05
		_ShadowFalloff( "Shadow Falloff", Range( 1, 10 ) ) = 1
		[Header(SHADOW COLOR)][ToggleUI][Space(5)] _ShadowColorEnable( "Enable Shadow Color", Float ) = 0
		[HDR] _ShadowColor( "Shadow Color", Color ) = ( 0.3113208, 0.3113208, 0.3113208, 0 )
		[HDR][Header(INDIRECT LIGHTING)] _IndirectSpecColor( "Indirect Specular Color", Color ) = ( 0.01, 0.01, 0.01 )
		_IndirectSpecular( "Indirect Specular ", Range( 0, 1 ) ) = 0.85
		_IndirectSpecularSmoothness( "Indirect Specular Smoothness", Range( 0, 1 ) ) = 1
		_IndirectDiffuse( "Indirect Diffuse", Range( 0, 1 ) ) = 0.5
		[Header(TRANSMISSION)][ToggleUI] _TransmissionMapEnable( "ENABLE TRANSMISSION", Float ) = 0
		[SingleLineTexture] _TransmissionMap( "Transmission Map", 2D ) = "white" {}
		[Toggle] _TransmissionMapInverted( "Transmission Map Inverted", Float ) = 0
		[HDR] _TransmissionColor( "Transmission Color", Color ) = ( 0.5, 0.5, 0.5, 1 )
		_TransmissionStrength( "Transmission Strength", Range( 0, 1 ) ) = 0.15
		_TransmissionFeather( "Transmission Feather", Range( 0, 2 ) ) = 1
		[Header(TRANSLUCENCY)][ToggleUI] _TranslucencyMapEnable( "ENABLE TRANSLUCENCY", Float ) = 0
		[SingleLineTexture] _TranslucencyMap( "Translucency Map", 2D ) = "white" {}
		[Toggle] _TranslucencyMapInverted( "Translucency Map Inverted", Float ) = 0
		[HDR] _TranslucencyColor( "Translucency Color", Color ) = ( 0.35, 0.35, 0.35, 1 )
		_TranslucencyStrength( "Translucency Strength", Range( 0, 50 ) ) = 0.5
		_TranslucencyFeather( "Translucency Feather", Range( 0, 2 ) ) = 1
		_TranslucencyNormalDistortion( "Translucency Normal Distortion", Range( 0, 1 ) ) = 0.3
		_TranslucencyScattering( "Translucency Scatterring", Range( 1, 50 ) ) = 1
		_TranslucencyDirect( "Translucency Direct", Range( 0, 1 ) ) = 0.45
		_TranslucencyAmbient( "Translucency Ambient", Range( 0, 1 ) ) = 0.65
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" }
		Cull [_Cull]
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardBRDF.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#pragma target 3.5
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#define ASE_VERSION 19902
		#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
		#pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
		#pragma multi_compile _ _FORWARD_PLUS
		#pragma multi_compile _ _CLUSTER_LIGHT_LOOP
		#pragma multi_compile _ _LIGHT_LAYERS
		#pragma multi_compile _ LIGHTMAP_ON
		#pragma multi_compile _ DYNAMICLIGHTMAP_ON
		#pragma multi_compile _ DIRLIGHTMAP_COMBINED
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 vertexToFrag795_g62248;
			float3 ase_positionRWS;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 vertexColor : COLOR;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform int _Cull;
		uniform sampler2D _MainTex;
		uniform float4 _MainUVs;
		uniform half _AlphaCutoffBias;
		uniform half _GlancingClipMode;
		uniform sampler2D _BumpMap;
		uniform half _NormalStrength;
		uniform half _IndirectSpecularSmoothness;
		uniform sampler2D _MainMaskMap;
		uniform float _MainMaskType;
		uniform half _SmoothnessStrength;
		uniform half _OcclusionStrengthAO;
		uniform float3 _IndirectSpecColor;
		uniform half _IndirectSpecular;
		uniform float _MetallicStrength;
		uniform half3 _BaseColor;
		uniform float _Saturation;
		uniform half _Brightness;
		uniform float4 _ShadowColor;
		uniform half _ShadowStrength;
		uniform half _ShadowOffset;
		uniform float _ShadowFalloff;
		uniform float _ShadowColorEnable;
		uniform float4 _SpecularColor;
		uniform sampler2D _SpecularMap;
		uniform float _IndirectDiffuse;
		uniform float4 _EmissionColor;
		uniform sampler2D _EmissionMap;
		uniform half _EmissiveIntensity;
		uniform float _AlbedoAffectEmissive;
		uniform float _EmissionEnable;
		uniform float _TranslucencyNormalDistortion;
		uniform float _TranslucencyScattering;
		uniform float _TranslucencyDirect;
		uniform float _TranslucencyAmbient;
		uniform sampler2D _TranslucencyMap;
		uniform float _TranslucencyFeather;
		uniform float _TranslucencyMapInverted;
		uniform float4 _TranslucencyColor;
		uniform float _TranslucencyStrength;
		uniform half _TranslucencyMapEnable;
		uniform sampler2D _TransmissionMap;
		uniform float _TransmissionFeather;
		uniform float _TransmissionMapInverted;
		uniform float4 _TransmissionColor;
		uniform half _TransmissionStrength;
		uniform half _TransmissionMapEnable;
		uniform float _MaskClipValue2;


		float4x4 InverseProjectionMatrix()
		{
			float4x4 m = UNITY_MATRIX_P;
			float n11 = m[ 0 ][ 0 ];
			float n22 = m[ 1 ][ 1 ];
			float n33 = m[ 2 ][ 2 ];
			float n34 = m[ 3 ][ 2 ];
			float n43 = m[ 2 ][ 3 ];
			float t11 = -n22 * n34 * n43;
			float det = n11 * t11;
			float idet = 1.0f / det;
			m[ 0 ][ 0 ] = +t11* idet;
			m[ 1 ][ 1 ] = -n11* n34 * n43* idet;
			m[ 2 ][ 2 ] = 0;
			m[ 2 ][ 3 ] = -n11* n22 * n43* idet;
			m[ 3 ][ 2 ] = -n11* n22 * n34* idet;
			m[ 3 ][ 3 ] = +n11* n22 * n33* idet;
			return m;
		}


		float3 ASESafeNormalize(float3 inVec)
		{
			float dp3 = max(1.175494351e-38, dot(inVec, inVec));
			return inVec* rsqrt(dp3);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.vertexToFrag795_g62248 = ( ( v.texcoord.xy * (_MainUVs).xy ) + (_MainUVs).zw );
			float4 ase_positionOS4f = v.vertex;
			float4x4 ase_matrixInvP = InverseProjectionMatrix();
			float4 ase_positionCS = UnityObjectToClipPos( ase_positionOS4f );
			float4 ase_hpositionVS = mul( ase_matrixInvP, ase_positionCS );
			float3 ase_positionRWS = mul( ( float3x3 )UNITY_MATRIX_I_V, ase_hpositionVS.xyz / ase_hpositionVS.w );
			o.ase_positionRWS = ase_positionRWS;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float4 tex2DNode35_g62248 = tex2D( _MainTex, i.vertexToFrag795_g62248 );
			float Alpha79_g62248 = tex2DNode35_g62248.a;
			clip( Alpha79_g62248 - ( 1.0 - _AlphaCutoffBias ));
			float temp_output_186_0_g62248 = saturate( ( ( Alpha79_g62248 / max( fwidth( Alpha79_g62248 ) , 0.0001 ) ) + 0.5 ) );
			float3 ase_positionRWS = i.ase_positionRWS;
			float3 temp_output_102_0_g62254 = ( cross( ddx( ase_positionRWS ) , ddy( ase_positionRWS ) ) * _ProjectionParams.x );
			float3 normalizeResult79_g62254 = normalize( temp_output_102_0_g62254 );
			float3 ase_positionWS = i.worldPos;
			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_positionWS );
			float3 ase_viewDirSafeWS = Unity_SafeNormalize( ase_viewVectorWS );
			float3 ViewDir_WS1115_g62248 = ase_viewDirSafeWS;
			float dotResult149_g62248 = dot( normalizeResult79_g62254 , ViewDir_WS1115_g62248 );
			float temp_output_147_0_g62248 = ( 1.0 - abs( dotResult149_g62248 ) );
			#ifdef UNITY_PASS_SHADOWCASTER
				float staticSwitch143_g62248 = 1.0;
			#else
				float staticSwitch143_g62248 = ( 1.0 - ( temp_output_147_0_g62248 * temp_output_147_0_g62248 ) );
			#endif
			float lerpResult142_g62248 = lerp( 1.0 , staticSwitch143_g62248 , _GlancingClipMode);
			float3 Normal_WS1160_g62248 = normalize( (WorldNormalVector( i , UnpackScaleNormal( tex2D( _BumpMap, i.vertexToFrag795_g62248 ), _NormalStrength ) )) );
			float3 indirectNormal647_g62248 = Normal_WS1160_g62248;
			float4 tex2DNode473_g62248 = tex2D( _MainMaskMap, i.vertexToFrag795_g62248 );
			float lerpResult750_g62248 = lerp( tex2DNode473_g62248.g , ( 1.0 - tex2DNode473_g62248.g ) , _MainMaskType);
			float temp_output_414_0_g62248 = ( lerpResult750_g62248 * _SmoothnessStrength );
			float Smoothness_417_g62248 = temp_output_414_0_g62248;
			float Occlusion435_g62248 = saturate( (min( tex2DNode473_g62248.b , i.vertexColor.a )*_OcclusionStrengthAO + ( 1.0 - _OcclusionStrengthAO )) );
			Unity_GlossyEnvironmentData g647_g62248 = UnityGlossyEnvironmentSetup( (_IndirectSpecularSmoothness*( 1.0 - Smoothness_417_g62248 ) + Smoothness_417_g62248), data.worldViewDir, indirectNormal647_g62248, float3(0,0,0));
			float3 indirectSpecular647_g62248 = UnityGI_IndirectSpecular( data, Occlusion435_g62248, indirectNormal647_g62248, g647_g62248 );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 MainLightColor1902_g62248 = ase_lightColor.rgb;
			float MainLightIntensity2109_g62248 = ase_lightColor.a;
			float Metallic403_g62248 = ( _MetallicStrength * tex2DNode473_g62248.r );
			float temp_output_1366_0_g62248 = (_IndirectSpecular*( 1.0 - Metallic403_g62248 ) + Metallic403_g62248);
			float3 temp_output_1368_0_g62248 = (( indirectSpecular647_g62248 * ( _IndirectSpecColor * ( MainLightColor1902_g62248 * max( MainLightIntensity2109_g62248 , 0.0 ) ) ) )*temp_output_1366_0_g62248 + ( 1.0 - temp_output_1366_0_g62248 ));
			float3 Indirect_Specular600_g62248 = temp_output_1368_0_g62248;
			float3 temp_output_12_0_g62252 = tex2DNode35_g62248.rgb;
			float dotResult28_g62252 = dot( float3( 0.2126729, 0.7151522, 0.072175 ) , temp_output_12_0_g62252 );
			float3 temp_cast_1 = (dotResult28_g62252).xxx;
			float temp_output_21_0_g62252 = ( 1.0 - _Saturation );
			float3 lerpResult31_g62252 = lerp( temp_cast_1 , temp_output_12_0_g62252 , temp_output_21_0_g62252);
			float3 temp_output_48_0_g62248 = ( _BaseColor * lerpResult31_g62252 * _Brightness );
			float3 lerpResult898_g62248 = lerp( ( temp_output_48_0_g62248 * _ShadowColor.rgb ) , _ShadowColor.rgb , _ShadowColor.a);
			float3 temp_output_230_0_g68751 = Normal_WS1160_g62248;
			float dotResult151_g68751 = dot( temp_output_230_0_g68751 , ase_viewDirSafeWS );
			float Dot_NdotV210_g62248 = max( dotResult151_g68751 , 0.0 );
			float3 temp_output_230_0_g68741 = Normal_WS1160_g62248;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_lightDirWS = 0;
			#else //aseld
			float3 ase_lightDirWS = Unity_SafeNormalize( UnityWorldSpaceLightDir( ase_positionWS ) );
			#endif //aseld
			float3 normalizeResult447_g68741 = ASESafeNormalize( ( ase_viewDirSafeWS + ase_lightDirWS ) );
			float dotResult178_g68741 = dot( temp_output_230_0_g68741 , normalizeResult447_g68741 );
			float temp_output_882_109_g62248 = max( dotResult178_g68741 , 0.0 );
			float Dot_NdotH655_g62248 = temp_output_882_109_g62248;
			float Dot_NdotH_Total2269_g62248 = Dot_NdotH655_g62248;
			float2 appendResult375_g68421 = (float2(Dot_NdotV210_g62248 , Dot_NdotH_Total2269_g62248));
			float3 temp_output_230_0_g68740 = Normal_WS1160_g62248;
			float dotResult152_g68740 = dot( temp_output_230_0_g68740 , ase_lightDirWS );
			float temp_output_2182_111_g62248 = dotResult152_g68740;
			float temp_output_2154_0_g62248 = max( temp_output_2182_111_g62248 , 0.0 );
			float Dot_NdotL208_g62248 = temp_output_2154_0_g62248;
			float Dot_NdotL_total2267_g62248 = Dot_NdotL208_g62248;
			float2 appendResult376_g68421 = (float2(Dot_NdotH_Total2269_g62248 , Dot_NdotL_total2267_g62248));
			float3 normalizeResult447_g68739 = ASESafeNormalize( ( ase_viewDirSafeWS + ase_lightDirWS ) );
			float dotResult147_g68739 = dot( ase_viewDirSafeWS , normalizeResult447_g68739 );
			float temp_output_968_108_g62248 = max( dotResult147_g68739 , 0.0 );
			float Dot_VdotH975_g62248 = temp_output_968_108_g62248;
			float Dot_VdotH_Total2270_g62248 = Dot_VdotH975_g62248;
			float2 break372_g68421 = ( ( appendResult375_g68421 * appendResult376_g68421 * 2.0 ) / Dot_VdotH_Total2270_g62248 );
			float Shadow_65_g62248 = pow( saturate( ( ( min( min( break372_g68421.x , break372_g68421.y ) , 1.0 ) * ( 1.0 - _ShadowStrength ) ) - _ShadowOffset ) ) , _ShadowFalloff );
			float3 lerpResult905_g62248 = lerp( temp_output_48_0_g62248 , ( lerpResult898_g62248 * Occlusion435_g62248 ) , ( ( 1.0 - Shadow_65_g62248 ) * _ShadowColorEnable ));
			float3 _Color98_g68689 = lerpResult905_g62248;
			float3 Specular_Map64_g62248 = ( (_SpecularColor).rgb * (tex2D( _SpecularMap, i.vertexToFrag795_g62248 )).rgb );
			float3 specRGB168_g68689 = Specular_Map64_g62248;
			float _Metallic711_g68689 = Metallic403_g62248;
			float3 lerpResult654_g68689 = lerp( _Color98_g68689 , specRGB168_g68689 , ( _Metallic711_g68689 * 0.5 ));
			float3 specColor651_g68689 = lerpResult654_g68689;
			float temp_output_708_0_g68689 = Smoothness_417_g62248;
			float temp_output_706_0_g68689 = ( 1.0 - ( temp_output_708_0_g68689 * temp_output_708_0_g68689 ) );
			float _Roughness707_g68689 = ( temp_output_706_0_g68689 * temp_output_706_0_g68689 );
			float grazingTerm703_g68689 = saturate( ( _Metallic711_g68689 + _Roughness707_g68689 ) );
			float3 temp_cast_2 = (grazingTerm703_g68689).xxx;
			float NdotV372_g68689 = Dot_NdotV210_g62248;
			float temp_output_676_0_g68689 = saturate( ( 1.0 - NdotV372_g68689 ) );
			float3 lerpResult670_g68689 = lerp( specColor651_g68689 , temp_cast_2 , ( temp_output_676_0_g68689 * temp_output_676_0_g68689 * temp_output_676_0_g68689 * temp_output_676_0_g68689 * temp_output_676_0_g68689 ));
			float3 finalSpec683_g68689 = ( Indirect_Specular600_g62248 * lerpResult670_g68689 * max( _Metallic711_g68689 , 0.15 ) * ( 1.0 - ( _Roughness707_g68689 * _Roughness707_g68689 * _Roughness707_g68689 ) ) );
			float NdotL373_g68689 = Dot_NdotL_total2267_g62248;
			float2 appendResult44_g68693 = (float2(NdotL373_g68689 , NdotV372_g68689));
			float2 temp_output_330_0_g68693 = saturate( ( 1.0 - appendResult44_g68693 ) );
			float2 temp_output_331_0_g68693 = ( temp_output_330_0_g68693 * temp_output_330_0_g68693 * temp_output_330_0_g68693 * temp_output_330_0_g68693 * temp_output_330_0_g68693 );
			float3 normalizeResult176_g68745 = normalize( ( ase_lightDirWS + ase_viewDirSafeWS ) );
			float dotResult159_g68745 = dot( ase_lightDirWS , normalizeResult176_g68745 );
			float temp_output_965_112_g62248 = max( dotResult159_g68745 , 0.0 );
			float Dot_LdotH972_g62248 = temp_output_965_112_g62248;
			float Dot_LdotH_Total2265_g62248 = Dot_LdotH972_g62248;
			float LdotH643_g68689 = Dot_LdotH_Total2265_g62248;
			float2 break335_g68693 = ( ( 1.0 - temp_output_331_0_g68693 ) + ( temp_output_331_0_g68693 * ( ( LdotH643_g68689 * LdotH643_g68689 * _Roughness707_g68689 * 2.0 ) + 0.5 ) ) );
			float temp_output_336_0_g68693 = ( break335_g68693.x * break335_g68693.y );
			float3 Normal_Map14_g62248 = UnpackScaleNormal( tex2D( _BumpMap, i.vertexToFrag795_g62248 ), _NormalStrength );
			UnityGI gi646_g62248 = gi;
			float3 diffNorm646_g62248 = normalize( WorldNormalVector( i , Normal_Map14_g62248 ) );
			gi646_g62248 = UnityGI_Base( data, 1, diffNorm646_g62248 );
			float3 indirectDiffuse646_g62248 = gi646_g62248.indirect.diffuse + diffNorm646_g62248 * 0.0001;
			float3 Indirect_Diffuse644_g62248 = ( indirectDiffuse646_g62248 * Occlusion435_g62248 * _IndirectDiffuse );
			float3 diffuseColor77_g68689 = ( ( _Color98_g68689 * ( 1.0 - _Metallic711_g68689 ) * temp_output_336_0_g68693 ) + Indirect_Diffuse644_g62248 );
			float geoShadow142_g68689 = Shadow_65_g62248;
			float saferPower14_g68492 = abs( 2.0 );
			float2 _GaussianApprox = float2(-5.55473,6.98316);
			float Fresnel_Term201_g62248 = pow( saferPower14_g68492 , ( Dot_LdotH972_g62248 * ( ( Dot_LdotH972_g62248 * _GaussianApprox.x ) - _GaussianApprox.y ) ) );
			float fresnel104_g68689 = Fresnel_Term201_g62248;
			float3 SpecFresnel431_g68689 = ( specColor651_g68689 + ( ( 1.0 - specColor651_g68689 ) * fresnel104_g68689 ) );
			float temp_output_96_0_g68356 = acos( Dot_NdotH_Total2269_g62248 );
			float temp_output_53_0_g62248 = ( temp_output_414_0_g62248 * temp_output_414_0_g62248 );
			float temp_output_47_0_g62248 = ( 1.0 - temp_output_53_0_g62248 );
			float Roughness730_g62248 = ( temp_output_47_0_g62248 * temp_output_47_0_g62248 );
			float temp_output_226_0_g68356 = Roughness730_g62248;
			float Specular200_g62248 = exp( ( -( temp_output_96_0_g68356 * temp_output_96_0_g68356 ) / max( ( temp_output_226_0_g68356 * temp_output_226_0_g68356 ) , 0.0001 ) ) );
			float specularDistr105_g68689 = Specular200_g62248;
			float temp_output_659_0_g68689 = ( max( NdotV372_g68689 , 0.1 ) * max( NdotL373_g68689 , 0.1 ) * 4.0 );
			float3 specularity657_g68689 = ( ( geoShadow142_g68689 * ( SpecFresnel431_g68689 * lerpResult654_g68689 ) * ( specularDistr105_g68689 * lerpResult654_g68689 ) ) / temp_output_659_0_g68689 );
			float3 temp_output_1609_0_g62248 = ( MainLightColor1902_g62248 * ase_lightAtten );
			float3 Scene_Lighting1527_g62248 = temp_output_1609_0_g62248;
			float3 temp_output_829_0_g68689 = ( ( finalSpec683_g68689 + diffuseColor77_g68689 + specularity657_g68689 ) * Scene_Lighting1527_g62248 * NdotL373_g68689 );
			float3 temp_output_1554_0_g62248 = ( (_EmissionColor).rgb * (tex2D( _EmissionMap, i.vertexToFrag795_g62248 )).rgb * _EmissiveIntensity );
			float3 BaseColor_Map63_g62248 = temp_output_48_0_g62248;
			float temp_output_2_0_g62253 = _AlbedoAffectEmissive;
			float temp_output_3_0_g62253 = ( 1.0 - temp_output_2_0_g62253 );
			float3 appendResult7_g62253 = (float3(temp_output_3_0_g62253 , temp_output_3_0_g62253 , temp_output_3_0_g62253));
			float3 Emission1553_g62248 = ( ( temp_output_1554_0_g62248 * ( ( BaseColor_Map63_g62248 * temp_output_2_0_g62253 ) + appendResult7_g62253 ) ) * _EmissionEnable );
			float3 LightDir_WS1116_g62248 = ase_lightDirWS;
			float dotResult1674_g62248 = dot( -( LightDir_WS1116_g62248 + ( Normal_WS1160_g62248 * _TranslucencyNormalDistortion ) ) , ViewDir_WS1115_g62248 );
			float3 temp_output_1450_0_g62248 = ( (tex2D( _TranslucencyMap, i.vertexToFrag795_g62248 )).rgb / max( _TranslucencyFeather , 0.1 ) );
			float3 lerpResult1441_g62248 = lerp( temp_output_1450_0_g62248 , ( 1.0 - temp_output_1450_0_g62248 ) , _TranslucencyMapInverted);
			float3 Translucency1428_g62248 = ( ( ( ( pow( saturate( dotResult1674_g62248 ) , _TranslucencyScattering ) * _TranslucencyDirect ) + ( Indirect_Diffuse644_g62248 * _TranslucencyAmbient ) ) * MainLightColor1902_g62248 * BaseColor_Map63_g62248 * lerpResult1441_g62248 * (_TranslucencyColor).rgb * _TranslucencyStrength ) * _TranslucencyMapEnable );
			float Dot_NdotL_Inv1390_g62248 = max( -temp_output_2182_111_g62248 , 0.0 );
			float3 temp_output_1453_0_g62248 = ( (tex2D( _TransmissionMap, i.vertexToFrag795_g62248 )).rgb / max( _TransmissionFeather , 0.1 ) );
			float3 lerpResult1455_g62248 = lerp( temp_output_1453_0_g62248 , ( 1.0 - temp_output_1453_0_g62248 ) , _TransmissionMapInverted);
			float3 temp_output_1713_0_g62248 = (_TransmissionColor).rgb;
			float3 Transmission1400_g62248 = ( ( Dot_NdotL_Inv1390_g62248 * MainLightColor1902_g62248 * BaseColor_Map63_g62248 * ( lerpResult1455_g62248 * temp_output_1713_0_g62248 ) * _TransmissionStrength ) * _TransmissionMapEnable );
			c.rgb = ( ( ( temp_output_829_0_g68689 + Emission1553_g62248 ) + Translucency1428_g62248 ) + Transmission1400_g62248 );
			c.a = temp_output_186_0_g62248;
			clip( lerpResult142_g62248 - _MaskClipValue2 );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.5
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float3 customPack2 : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.vertexToFrag795_g62248;
				o.customPack2.xyz = customInputData.ase_positionRWS;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.vertexToFrag795_g62248 = IN.customPack1.xy;
				surfIN.ase_positionRWS = IN.customPack2.xyz;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
}
/*ASEBEGIN
Version=19902
Node;AmplifyShaderEditor.IntNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;263;2048,-1104;Inherit;False;Property;_Cull;Render Face;0;1;[Enum];Create;False;1;;0;1;Front,2,Back,1,Both,0;True;0;False;2;0;False;0;1;INT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;264;2048,-1024;Inherit;False;Constant;_MaskClipValue2;Mask Clip Value;19;1;[HideInInspector];Create;True;1;;0;0;True;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StickyNoteNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;451;1664,-896;Inherit;False;263.3185;132.2972;Physical Based Rendering Skin;;0,0,0,1;Physical Based Rendering Skin$-- GSF CookTorrance$-- NDF Gaussian$-- Gaussian Fresnel;0;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;450;1664,-704;Inherit;False;PBR Core;1;;62248;d226ce46eb9ddb04ba9f0a949b5fddfe;21,213,2,2520,3,240,3,215,1,536,0,545,1,1279,1,908,1,1588,1,1886,1,1463,0,1887,1,2285,0,2543,0,2239,0,2242,0,2246,0,2243,0,2706,0,2235,0,2756,0;0;4;FLOAT3;0;FLOAT;156;FLOAT;159;FLOAT;158
Node;AmplifyShaderEditor.StandardSurfaceOutputNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;179;2048,-944;Float;False;True;-1;3;AmplifyShaderEditor.MaterialInspector;0;0;CustomLighting;AmplifyShaderPack/Community/Physical Based Rendering Skin;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.58;True;True;0;True;TransparentCutout;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;True;_Cull;-1;0;True;_MaskClipValue2;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;179;9;450;156
WireConnection;179;10;450;159
WireConnection;179;13;450;0
ASEEND*/
//CHKSM=BCDD0EF7D1A4D949E677DCB3DC5F9C877D6E4D92
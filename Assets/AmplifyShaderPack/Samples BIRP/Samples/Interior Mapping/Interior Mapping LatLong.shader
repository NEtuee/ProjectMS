// Made with Amplify Shader Editor v1.9.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASE/Interior Mapping/LatLong"
{
	Properties
	{
		_RoomCountX( "Room Count X", Float ) = 4
		_RoomCountY( "Room Count Y", Float ) = 4
		[Header(FACADE)][ToggleUI] _EnableFacade( "Enable Facade", Float ) = 0
		_FacadeTilling( "Facade Tilling", Vector ) = ( 8, 8, 0, 0 )
		[SingleLineTexture] _FacadeMap( "Facade Map", 2D ) = "white" {}
		_FacadeMetalic( "Facade Metalic", Range( 0, 1 ) ) = 0
		_FacadeSmoothness( "Facade Smoothness", Range( 0, 1 ) ) = 0
		_FacadeAlphaMetalic( "Facade Alpha Metalic", Range( 0, 1 ) ) = 1
		_FacadeAlphaSmoothness( "Facade Alpha Smoothness", Range( 0, 1 ) ) = 0.5
		[SingleLineTexture] _LatLongMap0( "LatLong Map", 2D ) = "white" {}
		_Brightness0( "Brightness", Float ) = 5
		[SingleLineTexture] _LatLongMap1( "LatLong Map", 2D ) = "white" {}
		_Brightness1( "Brightness", Float ) = 5
		[SingleLineTexture] _LatLongMap2( "LatLong Map", 2D ) = "white" {}
		_Brightness2( "Brightness", Float ) = 5
		[SingleLineTexture] _Roof( "Roof", 2D ) = "white" {}
		_EmissiveIntensity( "Emissive Intensity", Range( 0, 5 ) ) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
		#include "UnityStandardBRDF.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#define ASE_VERSION 19902
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
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float2 uv_texcoord;
		};

		uniform sampler2D _LatLongMap0;
		uniform float _RoomCountX;
		uniform float _RoomCountY;
		uniform half _Brightness0;
		uniform sampler2D _LatLongMap1;
		uniform half _Brightness1;
		uniform sampler2D _LatLongMap2;
		uniform half _Brightness2;
		uniform sampler2D _FacadeMap;
		uniform float4 _FacadeTilling;
		uniform float _EnableFacade;
		uniform sampler2D _Roof;
		uniform float4 _Roof_ST;
		uniform half _EmissiveIntensity;
		uniform float _FacadeMetalic;
		uniform float _FacadeAlphaMetalic;
		uniform float _FacadeSmoothness;
		uniform float _FacadeAlphaSmoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			o.Normal = float3(0,0,1);
			float3 ase_positionWS = i.worldPos;
			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_positionWS );
			float3 ase_viewDirSafeWS = Unity_SafeNormalize( ase_viewVectorWS );
			float3 ase_normalWS = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_normalWSNorm = normalize( ase_normalWS );
			float3 ase_tangentWS = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_bitangentWS = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_tangentWS, ase_bitangentWS, ase_normalWS );
			float3 worldToTangentDir182_g180 = mul( ase_worldToTangent, reflect( ase_viewDirSafeWS , ase_normalWSNorm ) );
			float3 temp_output_173_0_g180 = ( float3( 1,1,1 ) / ( worldToTangentDir182_g180 * float3( -1,1,1 ) ) );
			float2 temp_output_62_0_g180 = i.uv_texcoord;
			float2 appendResult443 = (float2(_RoomCountX , _RoomCountY));
			float2 temp_output_170_0_g180 = ( temp_output_62_0_g180 * appendResult443 );
			float2 temp_output_167_0_g180 = ( ( frac( temp_output_170_0_g180 ) * float2( 2,-2 ) ) - float2( 1,-1 ) );
			float3 appendResult199_g180 = (float3(temp_output_167_0_g180 , -1.0));
			float3 break197_g180 = ( abs( temp_output_173_0_g180 ) - ( temp_output_173_0_g180 * appendResult199_g180 ) );
			float2 appendResult192_g180 = (float2(( atan2( break197_g180.z , break197_g180.x ) + 1.570796 ) , ( ( -0.5 * UNITY_PI ) + asin( (worldToTangentDir182_g180).y ) )));
			float2 temp_output_518_56 = ( ( appendResult192_g180 + temp_output_167_0_g180 ) * float2( 0.1591549,-0.3183099 ) );
			float2 temp_output_518_201 = ddx( temp_output_62_0_g180 );
			float2 temp_output_518_202 = ddy( temp_output_62_0_g180 );
			float3 temp_output_7_0_g181 = frac( ( (floor( temp_output_170_0_g180 )).xyx * float3( 0.1031, 0.103, 0.0973 ) ) );
			float dotResult8_g181 = dot( temp_output_7_0_g181 , ( (temp_output_7_0_g181).yzx + 33.33 ) );
			float3 temp_output_12_0_g181 = ( temp_output_7_0_g181 + dotResult8_g181 );
			float4 lerpResult508 = lerp( ( tex2D( _LatLongMap0, temp_output_518_56, temp_output_518_201, temp_output_518_202 ) * _Brightness0 ) , ( tex2D( _LatLongMap1, temp_output_518_56, temp_output_518_201, temp_output_518_202 ) * _Brightness1 ) , round( frac( ( ( (temp_output_12_0_g181).xx + (temp_output_12_0_g181).yz ) * (temp_output_12_0_g181).zy ) ) ).y);
			float4 lerpResult435 = lerp( lerpResult508 , ( tex2D( _LatLongMap2, temp_output_518_56, temp_output_518_201, temp_output_518_202 ) * _Brightness2 ) , float4( 0,0,0,0 ));
			float3 temp_output_446_0 = (lerpResult435).rgb;
			float3 InteriorMapping461 = temp_output_446_0;
			float4 tex2DNode459 = tex2D( _FacadeMap, ( ( i.uv_texcoord * (_FacadeTilling).xy ) + (_FacadeTilling).zw ) );
			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
			float fresnelNdotV469 = dot( ase_normalWS, ase_viewDirWS );
			float fresnelNode469 = ( 0.0 + 1.0 * pow( max( 1.0 - fresnelNdotV469 , 0.0001 ), 0.1 ) );
			float temp_output_467_0 = ( ( tex2DNode459.a * 0.2 ) * ( 1.0 - fresnelNode469 ) );
			float3 lerpResult463 = lerp( (tex2DNode459).rgb , temp_output_446_0 , temp_output_467_0);
			float3 Facade464 = lerpResult463;
			float3 lerpResult474 = lerp( InteriorMapping461 , Facade464 , _EnableFacade);
			float2 uv_Roof = i.uv_texcoord * _Roof_ST.xy + _Roof_ST.zw;
			float3 lerpResult482 = lerp( lerpResult474 , (tex2D( _Roof, uv_Roof )).rgb , ase_normalWS.y);
			o.Albedo = lerpResult482;
			float3 Emission513 = ( ( ( temp_output_467_0 * temp_output_446_0 ) * round( round( frac( ( ( (temp_output_12_0_g181).xx + (temp_output_12_0_g181).yz ) * (temp_output_12_0_g181).zy ) ) ).y ) ) * _EmissiveIntensity );
			float3 lerpResult475 = lerp( float3( 0,0,0 ) , Emission513 , _EnableFacade);
			float3 lerpResult477 = lerp( lerpResult475 , float3( 0,0,0 ) , ase_normalWS.y);
			o.Emission = lerpResult477;
			float FacadeAlpha465 = tex2DNode459.a;
			float lerpResult490 = lerp( _FacadeMetalic , _FacadeAlphaMetalic , FacadeAlpha465);
			float lerpResult472 = lerp( 0.0 , lerpResult490 , _EnableFacade);
			float lerpResult480 = lerp( lerpResult472 , 0.0 , ase_normalWS.y);
			o.Metallic = lerpResult480;
			float lerpResult485 = lerp( _FacadeSmoothness , _FacadeAlphaSmoothness , FacadeAlpha465);
			float lerpResult473 = lerp( 0.0 , lerpResult485 , _EnableFacade);
			float lerpResult478 = lerp( lerpResult473 , 0.0 , ase_normalWS.y);
			o.Smoothness = lerpResult478;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
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
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
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
Node;AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;450;-3072.638,724.733;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;441;-3143.187,1351.552;Inherit;False;Property;_RoomCountX;Room Count X;0;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;442;-3144.375,1425.24;Inherit;False;Property;_RoomCountY;Room Count Y;1;0;Create;True;0;0;0;False;0;False;4;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;451;-2841.571,786.0541;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;443;-2970.722,1377.699;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;454;-2838.154,1343.858;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;457;-2781.171,794.4387;Inherit;False;Property;_FacadeTilling;Facade Tilling;3;0;Create;True;0;0;0;False;0;False;8,8,0,0;4,4,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;447;-1176.45,1867.108;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;502;-2785.251,1724.324;Inherit;True;Property;_LatLongMap1;LatLong Map;11;1;[SingleLineTexture];Create;False;0;0;0;False;0;False;None;48de6c31db55ac946a2eb4671db8c0d2;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;504;-2792.952,1519.2;Inherit;True;Property;_LatLongMap0;LatLong Map;9;1;[SingleLineTexture];Create;False;0;0;0;False;0;False;None;48de6c31db55ac946a2eb4671db8c0d2;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;518;-2791.202,1354.457;Inherit;False;UV Interior LatLong;17;;180;50f8fdc498805ca4f82dcfe307e97b3c;0;2;62;FLOAT2;0,0;False;58;FLOAT2;1,1;False;4;FLOAT2;56;FLOAT2;201;FLOAT2;202;FLOAT2;61
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;453;-2584.171,795.4387;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;449;-1188.178,1355.104;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;433;-2490.424,1528.886;Inherit;True;Property;_TextureSample9;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Derivative;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;439;-2490.676,1723.342;Inherit;True;Property;_TextureSample10;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Derivative;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;503;-2792.034,1923.664;Inherit;True;Property;_LatLongMap2;LatLong Map;13;1;[SingleLineTexture];Create;False;0;0;0;False;0;False;None;48de6c31db55ac946a2eb4671db8c0d2;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;505;-2175.506,1598.24;Half;False;Property;_Brightness0;Brightness;10;0;Create;False;1;;0;0;False;0;False;5;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;506;-2184.952,1807.2;Half;False;Property;_Brightness1;Brightness;12;0;Create;False;1;;0;0;False;0;False;5;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;452;-2432.462,724.2098;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;456;-2585.171,862.4387;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;432;-1139.471,1270.919;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RoundOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;510;-1135.96,1446.131;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;434;-2497.459,1922.681;Inherit;True;Property;_TextureSample11;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Derivative;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;437;-1888.099,1534.704;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;438;-1892.281,1726.804;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;444;-1846.652,1424.74;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;507;-2183.88,2006.171;Half;False;Property;_Brightness2;Brightness;14;0;Create;False;1;;0;0;False;0;False;5;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;455;-2292.268,846.2394;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;460;-2142.821,680.1063;Inherit;True;Property;_FacadeMap;Facade Map;4;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;ce6a56788ccccef4c8603eb6903c8746;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FresnelNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;469;-1800.952,1119.2;Inherit;False;Standard;WorldNormal;ViewDir;False;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;509;-961.0814,1271.275;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;514;-920.9521,1391.2;Half;False;Property;_EmissiveIntensity;Emissive Intensity;16;0;Create;False;1;;0;0;False;0;False;1;3.25;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;436;-1885.099,1924.704;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;508;-1656.952,1535.2;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;459;-1880.665,812.5724;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;466;-1560.952,1007.2;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;468;-1560.952,1119.2;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;448;-1179.178,1865.744;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;517;-640,1280;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;435;-1498.11,1898.35;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;467;-1400.952,1007.2;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;430;-1128.952,191.2003;Inherit;False;389.2787;211.2052;Mask for the roof area;6;497;496;495;494;493;431;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;440;-1395.872,815.1141;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;462;-1208.952,927.2003;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;513;-448,1280;Inherit;False;Emission;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;446;-1332.103,1893.916;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;431;-920.9521,239.2003;Inherit;False;134.5;130;World Normal Y;1;492;;0,0,0,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;497;-1112.952,239.2003;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;463;-1176.952,815.2003;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;465;-600.9521,927.2003;Inherit;False;FacadeAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;493;-776.9521,271.2003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;498;-1320.952,15.20026;Inherit;False;513;Emission;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RelayNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;492;-904.9521,287.2003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;461;-1085.722,1891.817;Inherit;False;InteriorMapping;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;464;-600.9521,815.2003;Inherit;False;Facade;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;471;-2216.952,-688.7997;Inherit;True;Property;_Roof;Roof;15;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;aab8de1052c54173b3d61e7f8b05aedf;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;475;-1096.952,15.20026;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;476;-776.9521,127.2003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;486;-1608.952,-320.7997;Inherit;False;Property;_FacadeSmoothness;Facade Smoothness;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;487;-1608.952,-240.7997;Inherit;False;Property;_FacadeAlphaSmoothness;Facade Alpha Smoothness;8;0;Create;True;0;0;0;False;0;False;0.5;0.35;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;488;-1496.952,-160.7997;Inherit;False;465;FacadeAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;489;-1592.952,-576.7997;Inherit;False;Property;_FacadeMetalic;Facade Metalic;5;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;491;-1592.952,-496.7997;Inherit;False;Property;_FacadeAlphaMetalic;Facade Alpha Metalic;7;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;501;-1320.952,-912.7997;Inherit;False;Property;_EnableFacade;Enable Facade;2;2;[Header];[ToggleUI];Create;True;1;FACADE;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;470;-1944.952,-688.7997;Inherit;True;Property;_TextureSample2;Texture Sample 2;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;477;-712.9521,15.20026;Inherit;False;3;0;FLOAT3;1,1,1;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;485;-1272.952,-320.7997;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;490;-1272.952,-576.7997;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;494;-776.9521,271.2003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;495;-776.9521,271.2003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;496;-776.9521,271.2003;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;499;-1384.952,-816.7997;Inherit;False;461;InteriorMapping;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;500;-1352.952,-736.7997;Inherit;False;464;Facade;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;472;-1080.952,-576.7997;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;473;-1096.952,-320.7997;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;474;-1048.952,-816.7997;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;479;-776.9521,-192.7997;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;481;-776.9521,-448.7997;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;483;-776.9521,-592.7997;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;484;-1608.952,-688.7997;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;516;-528,0;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;478;-712.9521,-320.7997;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;480;-712.9521,-576.7997;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;482;-712.9521,-704.7997;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;515;-528,-528;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;0;-432,-704;Float;False;True;-1;2;AmplifyShaderEditor.MaterialInspector;0;0;Standard;ASE/Interior Mapping/LatLong;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;451;0;450;0
WireConnection;443;0;441;0
WireConnection;443;1;442;0
WireConnection;454;0;451;0
WireConnection;447;0;446;0
WireConnection;518;62;454;0
WireConnection;518;58;443;0
WireConnection;453;0;457;0
WireConnection;449;0;447;0
WireConnection;433;0;504;0
WireConnection;433;1;518;56
WireConnection;433;3;518;201
WireConnection;433;4;518;202
WireConnection;433;7;504;1
WireConnection;439;0;502;0
WireConnection;439;1;518;56
WireConnection;439;3;518;201
WireConnection;439;4;518;202
WireConnection;439;7;502;1
WireConnection;452;0;450;0
WireConnection;452;1;453;0
WireConnection;456;0;457;0
WireConnection;432;0;467;0
WireConnection;432;1;449;0
WireConnection;510;0;444;1
WireConnection;434;0;503;0
WireConnection;434;1;518;56
WireConnection;434;3;518;201
WireConnection;434;4;518;202
WireConnection;434;7;503;1
WireConnection;437;0;433;0
WireConnection;437;1;505;0
WireConnection;438;0;439;0
WireConnection;438;1;506;0
WireConnection;444;0;518;61
WireConnection;455;0;452;0
WireConnection;455;1;456;0
WireConnection;509;0;432;0
WireConnection;509;1;510;0
WireConnection;436;0;434;0
WireConnection;436;1;507;0
WireConnection;508;0;437;0
WireConnection;508;1;438;0
WireConnection;508;2;444;1
WireConnection;459;0;460;0
WireConnection;459;1;455;0
WireConnection;459;7;460;1
WireConnection;466;0;459;4
WireConnection;468;0;469;0
WireConnection;448;0;446;0
WireConnection;517;0;509;0
WireConnection;517;1;514;0
WireConnection;435;0;508;0
WireConnection;435;1;436;0
WireConnection;467;0;466;0
WireConnection;467;1;468;0
WireConnection;440;0;459;0
WireConnection;462;0;448;0
WireConnection;513;0;517;0
WireConnection;446;0;435;0
WireConnection;463;0;440;0
WireConnection;463;1;462;0
WireConnection;463;2;467;0
WireConnection;465;0;459;4
WireConnection;493;0;492;0
WireConnection;492;0;497;2
WireConnection;461;0;446;0
WireConnection;464;0;463;0
WireConnection;475;1;498;0
WireConnection;475;2;501;0
WireConnection;476;0;493;0
WireConnection;470;0;471;0
WireConnection;470;7;471;1
WireConnection;477;0;475;0
WireConnection;477;2;476;0
WireConnection;485;0;486;0
WireConnection;485;1;487;0
WireConnection;485;2;488;0
WireConnection;490;0;489;0
WireConnection;490;1;491;0
WireConnection;490;2;488;0
WireConnection;494;0;492;0
WireConnection;495;0;492;0
WireConnection;496;0;492;0
WireConnection;472;1;490;0
WireConnection;472;2;501;0
WireConnection;473;1;485;0
WireConnection;473;2;501;0
WireConnection;474;0;499;0
WireConnection;474;1;500;0
WireConnection;474;2;501;0
WireConnection;479;0;495;0
WireConnection;481;0;494;0
WireConnection;483;0;496;0
WireConnection;484;0;470;0
WireConnection;516;0;477;0
WireConnection;478;0;473;0
WireConnection;478;2;479;0
WireConnection;480;0;472;0
WireConnection;480;2;481;0
WireConnection;482;0;474;0
WireConnection;482;1;484;0
WireConnection;482;2;483;0
WireConnection;515;0;516;0
WireConnection;0;0;482;0
WireConnection;0;2;515;0
WireConnection;0;3;480;0
WireConnection;0;4;478;0
ASEEND*/
//CHKSM=69D5E3B487DA28324764FC1EB827600BD553CD77
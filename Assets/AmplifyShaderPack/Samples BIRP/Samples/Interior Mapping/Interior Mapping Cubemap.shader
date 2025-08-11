// Made with Amplify Shader Editor v1.9.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASE/Interior Mapping/Cubemap"
{
	Properties
	{
		_RoomCountX( "Room Count X", Float ) = 8
		_RoomCountY( "Room Count Y", Float ) = 8
		[SingleLineTexture] _InteriorCube( "Interior Cube", CUBE ) = "white" {}
		_Brightness( "Brightness", Float ) = 5
		[Header(FACADE)][ToggleUI] _EnableFacade( "Enable Facade", Float ) = 0
		[SingleLineTexture] _FacadeMap( "Facade Map", 2D ) = "white" {}
		_FacadeTilling( "Facade Tilling", Vector ) = ( 4, 4, 0, 0 )
		_FacadeMetalic( "Facade Metalic", Range( 0, 1 ) ) = 0
		_FacadeSmoothness( "Facade Smoothness", Range( 0, 1 ) ) = 0
		_FacadeAlphaMetalic( "Facade Alpha Metalic", Range( 0, 1 ) ) = 1
		_FacadeAlphaSmoothness( "Facade Alpha Smoothness", Range( 0, 1 ) ) = 0.5
		[SingleLineTexture] _Roof( "Roof", 2D ) = "white" {}
		_EmissiveIntensity( "Emissive Intensity", Range( 0, 1 ) ) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Off
		CGINCLUDE
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
			float3 viewDir;
			INTERNAL_DATA
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
		};

		uniform samplerCUBE _InteriorCube;
		uniform float _RoomCountX;
		uniform float _RoomCountY;
		uniform float _Brightness;
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
			float3 temp_output_28_0_g127 = ( i.viewDir * float3( -1,1,1 ) );
			float3 temp_output_17_0_g127 = ( float3( 1,1,1 ) / temp_output_28_0_g127 );
			float2 appendResult476 = (float2(_RoomCountX , _RoomCountY));
			float2 temp_output_32_0_g127 = ( i.uv_texcoord * appendResult476 );
			float3 appendResult29_g127 = (float3(( ( frac( temp_output_32_0_g127 ) * float2( 2,-2 ) ) - float2( 1,-1 ) ) , -1.0));
			float3 break19_g127 = ( abs( temp_output_17_0_g127 ) - ( temp_output_17_0_g127 * appendResult29_g127 ) );
			float3 temp_output_30_0_g127 = ( ( min( min( break19_g127.x , break19_g127.y ) , break19_g127.z ) * temp_output_28_0_g127 ) + appendResult29_g127 );
			float3 break120_g127 = temp_output_30_0_g127;
			float3 appendResult121_g127 = (float3(break120_g127.x , ( break120_g127.y * -1.0 ) , break120_g127.z));
			float3 temp_output_453_0 = (( texCUBElod( _InteriorCube, float4( appendResult121_g127, 0.0) ) * _Brightness )).rgb;
			float3 InteriorMapping456 = temp_output_453_0;
			float4 tex2DNode445 = tex2D( _FacadeMap, ( ( i.uv_texcoord * (_FacadeTilling).xy ) + (_FacadeTilling).zw ) );
			float3 ase_positionWS = i.worldPos;
			float3 ase_viewVectorWS = ( _WorldSpaceCameraPos.xyz - ase_positionWS );
			float3 ase_viewDirWS = normalize( ase_viewVectorWS );
			float3 ase_normalWS = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float fresnelNdotV472 = dot( ase_normalWS, ase_viewDirWS );
			float fresnelNode472 = ( 0.0 + 1.0 * pow( max( 1.0 - fresnelNdotV472 , 0.0001 ), 0.1 ) );
			float temp_output_469_0 = ( ( tex2DNode445.a * 0.2 ) * ( 1.0 - fresnelNode472 ) );
			float3 lerpResult452 = lerp( (tex2DNode445).rgb , temp_output_453_0 , temp_output_469_0);
			float3 Facade458 = lerpResult452;
			float3 lerpResult414 = lerp( InteriorMapping456 , Facade458 , _EnableFacade);
			float2 uv_Roof = i.uv_texcoord * _Roof_ST.xy + _Roof_ST.zw;
			float3 lerpResult422 = lerp( lerpResult414 , (tex2D( _Roof, uv_Roof )).rgb , ase_normalWS.y);
			o.Albedo = lerpResult422;
			float2 break36_g127 = floor( temp_output_32_0_g127 );
			float3 appendResult10_g127 = (float3(break36_g127.x , break36_g127.y , break36_g127.x));
			float dotResult5_g127 = dot( appendResult10_g127 , float3( 127.1, 311.7, 74.7 ) );
			float3 appendResult3_g127 = (float3(break36_g127.y , break36_g127.x , break36_g127.x));
			float dotResult4_g127 = dot( appendResult3_g127 , float3( 269.5, 183.3, 246.1 ) );
			float3 appendResult8_g127 = (float3(break36_g127.x , break36_g127.y , break36_g127.y));
			float dotResult7_g127 = dot( appendResult8_g127 , float3( 113.5, 271.9, 124.6 ) );
			float3 appendResult6_g127 = (float3(dotResult5_g127 , dotResult4_g127 , dotResult7_g127));
			float3 temp_output_39_0_g127 = round( frac( ( sin( appendResult6_g127 ) * 43758.55 ) ) );
			float3 Emission460 = ( ( ( temp_output_469_0 * temp_output_453_0 ) * round( (temp_output_39_0_g127).y ) ) * _EmissiveIntensity );
			float3 lerpResult415 = lerp( float3( 0,0,0 ) , Emission460 , _EnableFacade);
			float3 lerpResult417 = lerp( lerpResult415 , float3( 0,0,0 ) , ase_normalWS.y);
			o.Emission = lerpResult417;
			float FacadeAlpha457 = tex2DNode445.a;
			float lerpResult430 = lerp( _FacadeMetalic , _FacadeAlphaMetalic , FacadeAlpha457);
			float lerpResult412 = lerp( 0.0 , lerpResult430 , _EnableFacade);
			float lerpResult420 = lerp( lerpResult412 , 0.0 , ase_normalWS.y);
			o.Metallic = lerpResult420;
			float lerpResult425 = lerp( _FacadeSmoothness , _FacadeAlphaSmoothness , FacadeAlpha457);
			float lerpResult413 = lerp( 0.0 , lerpResult425 , _EnableFacade);
			float lerpResult418 = lerp( lerpResult413 , 0.0 , ase_normalWS.y);
			o.Smoothness = lerpResult418;
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
				surfIN.viewDir = IN.tSpace0.xyz * worldViewDir.x + IN.tSpace1.xyz * worldViewDir.y + IN.tSpace2.xyz * worldViewDir.z;
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
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;444;-2864,64;Inherit;False;3044;1235;;37;480;479;478;477;476;475;474;473;472;471;470;469;468;467;466;465;464;463;462;461;460;459;458;457;456;455;454;453;452;451;450;449;448;447;446;445;483;;0,0,0,1;0;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;468;-2704,144;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;475;-2464,224;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;477;-2800,1072;Inherit;False;Property;_RoomCountY;Room Count Y;1;0;Create;True;0;0;0;False;0;False;8;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;479;-2800,992;Inherit;False;Property;_RoomCountX;Room Count X;0;0;Create;True;0;0;0;False;0;False;8;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;473;-2464,976;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;476;-2608,992;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;478;-2368,224;Inherit;False;Property;_FacadeTilling;Facade Tilling;6;0;Create;True;0;0;0;False;0;False;4,4,0,0;4,4,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;450;-2144,224;Inherit;False;FLOAT2;0;1;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;474;-2416,992;Inherit;False;UV Interior Cubemap;-1;;127;22dbe19408af0d14db5aa16670d14906;1,59,0;2;62;FLOAT2;0,0;False;58;FLOAT2;1,1;False;2;FLOAT3;56;FLOAT3;61
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;462;-864,1008;Inherit;False;FLOAT;1;1;2;3;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;447;-1984,144;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;449;-2144,320;Inherit;False;FLOAT2;2;3;2;3;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;466;-1664,1008;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;454;-704,912;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RoundOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;461;-704,1008;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;448;-1808,304;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;451;-1664,112;Inherit;True;Property;_FacadeMap;Facade Map;5;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;ce6a56788ccccef4c8603eb6903c8746;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;465;-1904,816;Inherit;True;Property;_InteriorCube;Interior Cube;2;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;07945fa9a8dd46c4f9cfb63a938d1897;False;white;LockedToCube;Cube;False;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;467;-1648,960;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FresnelNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;472;-1392,512;Inherit;False;Standard;WorldNormal;ViewDir;False;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;459;-528,912;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;480;-704,1104;Half;False;Property;_EmissiveIntensity;Emissive Intensity;12;0;Create;False;1;;0;0;False;0;False;1;0.932;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;445;-1392,288;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;463;-1248,896;Inherit;False;Property;_Brightness;Brightness;3;0;Create;True;0;0;0;False;0;False;5;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;464;-1584,816;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;LockedToCube;False;Object;-1;MipLevel;Cube;False;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;470;-1072,512;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.2;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;471;-1072,608;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;483;-352,896;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;455;-1056,816;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;469;-912,544;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;408;-595.3596,-393.8439;Inherit;False;389.2787;211.2052;Mask for the roof area;6;437;436;435;434;433;409;;0,0,0,1;0;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;446;-912,288;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;460;-176,896;Inherit;False;Emission;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;453;-896,816;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;409;-387.3596,-345.8439;Inherit;False;134.5;130;World Normal Y;1;432;;0,0,0,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;437;-579.3596,-345.8439;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;433;-243.3596,-313.8439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;452;-704,288;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;457;-144,400;Inherit;False;FacadeAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;438;-787.3596,-569.8439;Inherit;False;460;Emission;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RelayNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;432;-371.3596,-297.8439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;411;-1683.36,-1273.844;Inherit;True;Property;_Roof;Roof;11;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;aab8de1052c54173b3d61e7f8b05aedf;False;white;Auto;Texture2D;False;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;415;-563.3596,-569.8439;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;416;-243.3596,-457.8439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;426;-1075.36,-905.8439;Inherit;False;Property;_FacadeSmoothness;Facade Smoothness;8;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;427;-1075.36,-825.8439;Inherit;False;Property;_FacadeAlphaSmoothness;Facade Alpha Smoothness;10;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;429;-1059.36,-1161.844;Inherit;False;Property;_FacadeMetalic;Facade Metalic;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;431;-1059.36,-1081.844;Inherit;False;Property;_FacadeAlphaMetalic;Facade Alpha Metalic;9;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;456;-160,800;Inherit;False;InteriorMapping;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;458;-144,272;Inherit;False;Facade;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;428;-963.3596,-745.8439;Inherit;False;457;FacadeAlpha;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;441;-787.3596,-1497.844;Inherit;False;Property;_EnableFacade;Enable Facade;4;2;[Header];[ToggleUI];Create;True;1;FACADE;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;410;-1411.36,-1273.844;Inherit;True;Property;_TextureSample2;Texture Sample 2;6;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;417;-179.3596,-569.8439;Inherit;False;3;0;FLOAT3;1,1,1;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;425;-739.3596,-905.8439;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;430;-739.3596,-1161.844;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;434;-243.3596,-313.8439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;435;-243.3596,-313.8439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;436;-243.3596,-313.8439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;440;-819.3596,-1321.844;Inherit;False;458;Facade;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;439;-851.3596,-1401.844;Inherit;False;456;InteriorMapping;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;412;-547.3596,-1161.844;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;413;-563.3596,-905.8439;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;414;-515.3596,-1401.844;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;419;-243.3596,-777.8439;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;421;-243.3596,-1033.844;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;423;-243.3596,-1177.844;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SwizzleNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;424;-1075.36,-1273.844;Inherit;False;FLOAT3;0;1;2;3;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;442;0,-624;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;418;-179.3596,-905.8439;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;420;-179.3596,-1161.844;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;422;-179.3596,-1289.844;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;443;0,-1152;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;0;80,-1280;Float;False;True;-1;2;AmplifyShaderEditor.MaterialInspector;0;0;Standard;ASE/Interior Mapping/Cubemap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;17;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;475;0;468;0
WireConnection;473;0;475;0
WireConnection;476;0;479;0
WireConnection;476;1;477;0
WireConnection;450;0;478;0
WireConnection;474;62;473;0
WireConnection;474;58;476;0
WireConnection;462;0;474;61
WireConnection;447;0;468;0
WireConnection;447;1;450;0
WireConnection;449;0;478;0
WireConnection;466;0;474;56
WireConnection;454;0;469;0
WireConnection;454;1;453;0
WireConnection;461;0;462;0
WireConnection;448;0;447;0
WireConnection;448;1;449;0
WireConnection;467;0;466;0
WireConnection;459;0;454;0
WireConnection;459;1;461;0
WireConnection;445;0;451;0
WireConnection;445;1;448;0
WireConnection;445;7;451;1
WireConnection;464;0;465;0
WireConnection;464;1;467;0
WireConnection;464;7;465;1
WireConnection;470;0;445;4
WireConnection;471;0;472;0
WireConnection;483;0;459;0
WireConnection;483;1;480;0
WireConnection;455;0;464;0
WireConnection;455;1;463;0
WireConnection;469;0;470;0
WireConnection;469;1;471;0
WireConnection;446;0;445;0
WireConnection;460;0;483;0
WireConnection;453;0;455;0
WireConnection;433;0;432;0
WireConnection;452;0;446;0
WireConnection;452;1;453;0
WireConnection;452;2;469;0
WireConnection;457;0;445;4
WireConnection;432;0;437;2
WireConnection;415;1;438;0
WireConnection;415;2;441;0
WireConnection;416;0;433;0
WireConnection;456;0;453;0
WireConnection;458;0;452;0
WireConnection;410;0;411;0
WireConnection;410;7;411;1
WireConnection;417;0;415;0
WireConnection;417;2;416;0
WireConnection;425;0;426;0
WireConnection;425;1;427;0
WireConnection;425;2;428;0
WireConnection;430;0;429;0
WireConnection;430;1;431;0
WireConnection;430;2;428;0
WireConnection;434;0;432;0
WireConnection;435;0;432;0
WireConnection;436;0;432;0
WireConnection;412;1;430;0
WireConnection;412;2;441;0
WireConnection;413;1;425;0
WireConnection;413;2;441;0
WireConnection;414;0;439;0
WireConnection;414;1;440;0
WireConnection;414;2;441;0
WireConnection;419;0;435;0
WireConnection;421;0;434;0
WireConnection;423;0;436;0
WireConnection;424;0;410;0
WireConnection;442;0;417;0
WireConnection;418;0;413;0
WireConnection;418;2;419;0
WireConnection;420;0;412;0
WireConnection;420;2;421;0
WireConnection;422;0;414;0
WireConnection;422;1;424;0
WireConnection;422;2;423;0
WireConnection;443;0;442;0
WireConnection;0;0;422;0
WireConnection;0;2;443;0
WireConnection;0;3;420;0
WireConnection;0;4;418;0
ASEEND*/
//CHKSM=0E4791AEBA68D8E2860C8418FEED4699058746FF
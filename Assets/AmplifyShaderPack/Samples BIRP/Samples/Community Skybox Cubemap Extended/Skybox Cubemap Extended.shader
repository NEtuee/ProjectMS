// Made with Amplify Shader Editor v1.9.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "AmplifyShaderPack/Community/Skybox Cubemap Extended"
{
	Properties
	{
		[Gamma][Header(Cubemap)] _TintColor( "Tint Color", Color ) = ( 0.5, 0.5, 0.5, 1 )
		_Exposure( "Exposure", Range( 0, 8 ) ) = 1
		[NoScaleOffset] _Tex( "Cubemap (HDR)", CUBE ) = "black" {}
		[Header(Rotation)][Toggle( _ENABLEROTATION_ON )] _EnableRotation( "Enable Rotation", Float ) = 0
		[IntRange] _Rotation( "Rotation", Range( 0, 360 ) ) = 0
		_RotationSpeed( "Rotation Speed", Float ) = 1
		[Header(Fog)][Toggle( _ENABLEFOG_ON )] _EnableFog( "Enable Fog", Float ) = 0
		_FogHeight( "Fog Height", Range( 0, 1 ) ) = 1
		_FogSmoothness( "Fog Smoothness", Range( 0.01, 1 ) ) = 0.01
		_FogFill( "Fog Fill", Range( 0, 1 ) ) = 0.5
		[HideInInspector] _Tex_HDR( "DecodeInstructions", Vector ) = ( 0, 0, 0, 0 )
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Background"  "Queue" = "Background+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Off
		ZWrite Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 2.0
		#pragma shader_feature_local _ENABLEFOG_ON
		#pragma shader_feature_local _ENABLEROTATION_ON
		#define ASE_VERSION 19902
		#pragma surface surf Unlit keepalpha noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd vertex:vertexDataFunc 
		struct Input
		{
			float3 vertexToFrag774;
			float3 worldPos;
		};

		uniform half4 _Tex_HDR;
		uniform samplerCUBE _Tex;
		uniform half _Rotation;
		uniform half _RotationSpeed;
		uniform half4 _TintColor;
		uniform half _Exposure;
		uniform half _FogHeight;
		uniform half _FogSmoothness;
		uniform half _FogFill;


		inline half3 DecodeHDR1189( float4 Data )
		{
			return DecodeHDR(Data, _Tex_HDR);
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_positionWS = mul( unity_ObjectToWorld, v.vertex );
			float lerpResult268 = lerp( 1.0 , ( unity_OrthoParams.y / unity_OrthoParams.x ) , unity_OrthoParams.w);
			half CAMERA_MODE300 = lerpResult268;
			float3 appendResult1129 = (float3(ase_positionWS.x , ( ase_positionWS.y * CAMERA_MODE300 ) , ase_positionWS.z));
			float3 normalizeResult1130 = normalize( appendResult1129 );
			float3 appendResult56 = (float3(cos( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) , 0.0 , ( sin( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) * -1.0 )));
			float3 appendResult266 = (float3(0.0 , CAMERA_MODE300 , 0.0));
			float3 appendResult58 = (float3(sin( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) ) , 0.0 , cos( radians( ( _Rotation + ( _Time.y * _RotationSpeed ) ) ) )));
			float3 normalizeResult247 = normalize( ase_positionWS );
			#ifdef _ENABLEROTATION_ON
				float3 staticSwitch1164 = mul( float3x3( appendResult56, appendResult266, appendResult58 ), normalizeResult247 );
			#else
				float3 staticSwitch1164 = normalizeResult1130;
			#endif
			o.vertexToFrag774 = staticSwitch1164;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			half4 Data1189 = texCUBE( _Tex, i.vertexToFrag774 );
			half3 localDecodeHDR1189 = DecodeHDR1189( Data1189 );
			half4 CUBEMAP222 = ( float4( localDecodeHDR1189 , 0.0 ) * unity_ColorSpaceDouble * _TintColor * _Exposure );
			float3 ase_positionWS = i.worldPos;
			float3 normalizeResult319 = normalize( ase_positionWS );
			float lerpResult678 = lerp( saturate( pow(  (0.0 + ( abs( normalizeResult319.y ) - 0.0 ) * ( 1.0 - 0.0 ) / ( _FogHeight - 0.0 ) ) , ( 1.0 - _FogSmoothness ) ) ) , 0.0 , _FogFill);
			half FOG_MASK359 = lerpResult678;
			float4 lerpResult317 = lerp( unity_FogColor , CUBEMAP222 , FOG_MASK359);
			#ifdef _ENABLEFOG_ON
				float4 staticSwitch1179 = lerpResult317;
			#else
				float4 staticSwitch1179 = CUBEMAP222;
			#endif
			o.Emission = staticSwitch1179.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
}
/*ASEBEGIN
Version=19902
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;260;-896,1792;Half;False;Property;_RotationSpeed;Rotation Speed;6;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;701;-896,1664;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;48;-896,1536;Half;False;Property;_Rotation;Rotation;5;1;[IntRange];Create;True;0;0;0;False;0;False;0;0;0;360;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;255;-640,1664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OrthoParams, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;267;-892.4822,894.6918;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;276;-512,1536;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1007;-444.4821,894.6918;Half;False;Constant;_Float7;Float 7;47;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;309;-588.4823,894.6918;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RadiansOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;47;-384,1536;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;268;-252.4821,894.6918;Inherit;False;3;0;FLOAT;1;False;1;FLOAT;0.5;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;62;-224,1792;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1080;128,1664;Half;False;Constant;_Float26;Float 26;50;0;Create;True;0;0;0;False;0;False;-1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;59;128,1600;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;300;3.51777,894.6918;Half;False;CAMERA_MODE;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;60;320,1600;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;310;128,1760;Inherit;False;300;CAMERA_MODE;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1081;128,1840;Half;False;Constant;_Float27;Float 27;50;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;365;128,1984;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SinOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;61;128,1920;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1127;768,1936;Inherit;False;300;CAMERA_MODE;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.CosOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;55;128,1536;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;318;-896,2560;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;238;768,1760;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;56;512,1536;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1128;1024,1920;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;266;512,1728;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;58;512,1920;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;319;-640,2560;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.MatrixFromVectors, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;54;768,1536;Inherit;False;FLOAT3x3;0;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3x3;0
Node;AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;247;1024,1664;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1129;1152,1792;Inherit;False;FLOAT3;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;320;-448,2560;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;49;1280,1616;Inherit;False;2;2;0;FLOAT3x3;0,0,0,1,1,1,1,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1130;1280,1792;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;313;-896,2752;Half;False;Property;_FogHeight;Fog Height;8;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;325;-896,2880;Half;False;Property;_FogSmoothness;Fog Smoothness;9;0;Create;True;0;0;0;False;0;False;0.01;0.01;0.01;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1109;-128,2784;Half;False;Constant;_Float40;Float 40;55;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;314;-128,2560;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1108;-128,2688;Half;False;Constant;_Float39;Float 39;55;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1164;1600,1728;Float;False;Property;_EnableRotation;Enable Rotation;4;0;Create;True;0;0;0;False;1;Header(Rotation);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;329;128,2880;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;315;64,2560;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexToFragmentNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;774;2048,1728;Inherit;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;677;320,2560;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;41;2432,1536;Inherit;True;Property;_Tex;Cubemap (HDR);3;1;[NoScaleOffset];Create;False;0;0;0;False;0;False;-1;None;None;True;0;False;black;LockedToCube;False;Object;-1;Auto;Cube;False;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;679;512,2880;Half;False;Property;_FogFill;Fog Fill;10;0;Create;True;0;0;0;False;0;False;0.5;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;316;512,2560;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1110;512,2752;Half;False;Constant;_Float41;Float 41;55;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorSpaceDouble, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1175;2816,1616;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CustomExpressionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1189;2816,1536;Half;False;DecodeHDR(Data, _Tex_HDR);3;Create;1;True;Data;FLOAT4;0,0,0,0;In;;Float;False;DecodeHDR;True;False;0;;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1177;2816,1968;Half;False;Property;_Exposure;Exposure;2;0;Create;True;0;0;0;False;0;False;1;1;0;8;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1173;2816,1792;Half;False;Property;_TintColor;Tint Color;1;1;[Gamma];Create;True;0;0;0;False;1;Header(Cubemap);False;0.5,0.5,0.5,1;0.5,0.5,0.5,1;False;True;0;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;678;768,2560;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1174;3328,1536;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;359;1152,2560;Half;False;FOG_MASK;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;222;3712,1536;Half;False;CUBEMAP;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;436;-896,320;Inherit;False;359;FOG_MASK;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FogAndAmbientColorsNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;312;-896,128;Inherit;False;unity_FogColor;0;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;228;-896,240;Inherit;False;222;CUBEMAP;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;317;-512,128;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1179;-224,224;Float;False;Property;_EnableFog;Enable Fog;7;0;Create;True;0;0;0;False;1;Header(Fog);False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector4Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1190;2432,1792;Half;False;Property;_Tex_HDR;DecodeInstructions;11;1;[HideInInspector];Create;False;0;0;0;True;0;False;0,0,0,0;1,1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;26;128,64;Float;False;True;-1;0;AmplifyShaderEditor.MaterialInspector;0;0;Unlit;AmplifyShaderPack/Community/Skybox Cubemap Extended;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;True;False;False;False;False;False;Off;2;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0;True;False;0;True;Background;;Background;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;16;FLOAT4;0,0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1180;-946,1486;Inherit;False;2411;608;Cubemap Coordinates;0;CUBEMAP;0,0.4980392,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;431;-940.4822,846.6917;Inherit;False;860;219;Switch between Perspective / Orthographic camera;0;CAMERA MODE;1,0,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;432;-44.48222,846.6917;Inherit;False;305;165;CAMERA MODE OUTPUT;0;;0.4980392,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;700;-944,2512;Inherit;False;1898;485;Fog Coords on Screen;0;BUILT-IN FOG;0,0.4980392,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1087;1552,1664;Inherit;False;394;188;Enable Clouds Rotation;0;;0,1,0.4980392,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1183;1998,1678;Inherit;False;265;160;Per Vertex;0;;1,0,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1191;2382,1486;Inherit;False;1115;565;Base;0;;0,0.4980392,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;699;1104,2512;Inherit;False;293;165;FOG_MASK OUTPUT;0;;0.4980392,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;915;3664,1488;Inherit;False;293;165;CUBEMAP OUTPUT;0;;0.4980392,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1167;-946,78;Inherit;False;618;357;;0;FINAL COLOR;0.4980392,1,0,1;0;0
Node;AmplifyShaderEditor.CommentaryNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1185;-274,174;Inherit;False;306;188;Enable Fog;0;;0,1,0.4980392,1;0;0
WireConnection;255;0;701;0
WireConnection;255;1;260;0
WireConnection;276;0;48;0
WireConnection;276;1;255;0
WireConnection;309;0;267;2
WireConnection;309;1;267;1
WireConnection;47;0;276;0
WireConnection;268;0;1007;0
WireConnection;268;1;309;0
WireConnection;268;2;267;4
WireConnection;62;0;47;0
WireConnection;59;0;62;0
WireConnection;300;0;268;0
WireConnection;60;0;59;0
WireConnection;60;1;1080;0
WireConnection;365;0;62;0
WireConnection;61;0;62;0
WireConnection;55;0;62;0
WireConnection;56;0;55;0
WireConnection;56;1;1081;0
WireConnection;56;2;60;0
WireConnection;1128;0;238;2
WireConnection;1128;1;1127;0
WireConnection;266;0;1081;0
WireConnection;266;1;310;0
WireConnection;266;2;1081;0
WireConnection;58;0;61;0
WireConnection;58;1;1081;0
WireConnection;58;2;365;0
WireConnection;319;0;318;0
WireConnection;54;0;56;0
WireConnection;54;1;266;0
WireConnection;54;2;58;0
WireConnection;247;0;238;0
WireConnection;1129;0;238;1
WireConnection;1129;1;1128;0
WireConnection;1129;2;238;3
WireConnection;320;0;319;0
WireConnection;49;0;54;0
WireConnection;49;1;247;0
WireConnection;1130;0;1129;0
WireConnection;314;0;320;1
WireConnection;1164;1;1130;0
WireConnection;1164;0;49;0
WireConnection;329;0;325;0
WireConnection;315;0;314;0
WireConnection;315;1;1108;0
WireConnection;315;2;313;0
WireConnection;315;3;1108;0
WireConnection;315;4;1109;0
WireConnection;774;0;1164;0
WireConnection;677;0;315;0
WireConnection;677;1;329;0
WireConnection;41;1;774;0
WireConnection;316;0;677;0
WireConnection;1189;0;41;0
WireConnection;678;0;316;0
WireConnection;678;1;1110;0
WireConnection;678;2;679;0
WireConnection;1174;0;1189;0
WireConnection;1174;1;1175;0
WireConnection;1174;2;1173;0
WireConnection;1174;3;1177;0
WireConnection;359;0;678;0
WireConnection;222;0;1174;0
WireConnection;317;0;312;0
WireConnection;317;1;228;0
WireConnection;317;2;436;0
WireConnection;1179;1;228;0
WireConnection;1179;0;317;0
WireConnection;26;2;1179;0
ASEEND*/
//CHKSM=13793DB3C1D3C420AECB4D8CEB48C9FECA5F4EEE
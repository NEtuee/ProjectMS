// Made with Amplify Shader Editor v1.9.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/AmplifyShaderPack/Community/VolumetricPixelizationScreenAndMask"
{
	Properties
	{
		_MainTex ( "Screen", 2D ) = "black" {}
		
	}

	SubShader
	{
		LOD 0

		

		ZTest Always
		Cull Off
		ZWrite Off

		
		Pass
		{
			CGPROGRAM

			#define ASE_VERSION 19902


			#pragma vertex vert_img_custom
			#pragma fragment frag
			#pragma target 3.5
			#include "UnityCG.cginc"
			#include "UnityShaderVariables.cginc"
			#define ASE_NEEDS_TEXTURE_COORDINATES0
			#define ASE_NEEDS_FRAG_TEXTURE_COORDINATES0


			struct appdata_img_custom
			{
				float4 vertex : POSITION;
				half2 texcoord : TEXCOORD0;
				
			};

			struct v2f_img_custom
			{
				float4 pos : SV_POSITION;
				half2 uv   : TEXCOORD0;
				half2 stereoUV : TEXCOORD2;
		#if UNITY_UV_STARTS_AT_TOP
				half4 uv2 : TEXCOORD1;
				half4 stereoUV2 : TEXCOORD3;
		#endif
				float4 ase_texcoord4 : TEXCOORD4;
			};

			uniform sampler2D _MainTex;
			uniform half4 _MainTex_TexelSize;
			uniform half4 _MainTex_ST;

			uniform float3 _SpherePosition;
			UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
			uniform float4 _CameraDepthTexture_TexelSize;
			uniform float _SphereRadius;
			uniform float _MaskDensity;
			uniform float _MaskExponent;
			float2 UnStereo( float2 UV )
			{
				#if UNITY_SINGLE_PASS_STEREO
				float4 scaleOffset = unity_StereoScaleOffset[ unity_StereoEyeIndex ];
				UV.xy = (UV.xy - scaleOffset.zw) / scaleOffset.xy;
				#endif
				return UV;
			}
			
			float3 InvertDepthDir72_g1( float3 In )
			{
				float3 result = In;
				#if !defined(ASE_SRP_VERSION) || ASE_SRP_VERSION <= 70301
				result *= float3(1,1,-1);
				#endif
				return result;
			}
			


			v2f_img_custom vert_img_custom ( appdata_img_custom v  )
			{
				v2f_img_custom o;
				float4 ase_positionCS = UnityObjectToClipPos( v.vertex );
				float4 screenPos = ComputeScreenPos( ase_positionCS );
				o.ase_texcoord4 = screenPos;
				
				o.pos = UnityObjectToClipPos( v.vertex );
				o.uv = float4( v.texcoord.xy, 1, 1 );

				#if UNITY_UV_STARTS_AT_TOP
					o.uv2 = float4( v.texcoord.xy, 1, 1 );
					o.stereoUV2 = UnityStereoScreenSpaceUVAdjust ( o.uv2, _MainTex_ST );

					if ( _MainTex_TexelSize.y < 0.0 )
						o.uv.y = 1.0 - o.uv.y;
				#endif
				o.stereoUV = UnityStereoScreenSpaceUVAdjust ( o.uv, _MainTex_ST );
				return o;
			}

			half4 frag ( v2f_img_custom i ) : SV_Target
			{
				#ifdef UNITY_UV_STARTS_AT_TOP
					half2 uv = i.uv2;
					half2 stereoUV = i.stereoUV2;
				#else
					half2 uv = i.uv;
					half2 stereoUV = i.stereoUV;
				#endif

				half4 finalColor;

				// ase common template code
				float3 CamPosition11_g7 = _WorldSpaceCameraPos;
				float3 SphereCenter4_g7 = _SpherePosition;
				float3 SphereToCam17_g7 = ( CamPosition11_g7 - SphereCenter4_g7 );
				float4 screenPos = i.ase_texcoord4;
				float4 ase_positionSSNorm = screenPos / screenPos.w;
				ase_positionSSNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_positionSSNorm.z : ase_positionSSNorm.z * 0.5 + 0.5;
				float2 UV22_g3 = ase_positionSSNorm.xy;
				float2 localUnStereo22_g3 = UnStereo( UV22_g3 );
				float2 break64_g1 = localUnStereo22_g3;
				float depth01_69_g1 = SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_positionSSNorm.xy );
				#ifdef UNITY_REVERSED_Z
				float staticSwitch38_g1 = ( 1.0 - depth01_69_g1 );
				#else
				float staticSwitch38_g1 = depth01_69_g1;
				#endif
				float3 appendResult39_g1 = (float3(break64_g1.x , break64_g1.y , staticSwitch38_g1));
				float4 appendResult42_g1 = (float4((appendResult39_g1*2.0 + -1.0) , 1.0));
				float4 temp_output_43_0_g1 = mul( unity_CameraInvProjection, appendResult42_g1 );
				float3 temp_output_46_0_g1 = ( (temp_output_43_0_g1).xyz / (temp_output_43_0_g1).w );
				float3 In72_g1 = temp_output_46_0_g1;
				float3 localInvertDepthDir72_g1 = InvertDepthDir72_g1( In72_g1 );
				float4 appendResult49_g1 = (float4(localInvertDepthDir72_g1 , 1.0));
				float3 WorldPosition122_g7 = mul( unity_CameraToWorld, appendResult49_g1 ).xyz;
				float3 temp_output_9_0_g7 = ( _WorldSpaceCameraPos - WorldPosition122_g7 );
				float3 normalizeResult10_g7 = normalize( temp_output_9_0_g7 );
				float3 ViewDirection12_g7 = normalizeResult10_g7;
				float dotResult20_g7 = dot( SphereToCam17_g7 , ViewDirection12_g7 );
				float DirectionsSimilarity21_g7 = dotResult20_g7;
				float dotResult24_g7 = dot( SphereToCam17_g7 , SphereToCam17_g7 );
				float SphereRadius5_g7 = _SphereRadius;
				float Ratio25_g7 = ( ( DirectionsSimilarity21_g7 * DirectionsSimilarity21_g7 ) - ( dotResult24_g7 - ( SphereRadius5_g7 * SphereRadius5_g7 ) ) );
				float SqrtRatio37_g7 = sqrt( Ratio25_g7 );
				float temp_output_40_0_g7 = ( SqrtRatio37_g7 + -DirectionsSimilarity21_g7 );
				float DistanceToPixel114_g7 = -length( temp_output_9_0_g7 );
				float RejectionValue162_g7 = ( temp_output_40_0_g7 < DistanceToPixel114_g7 ? -1.0 : Ratio25_g7 );
				float temp_output_41_0_g7 = ( -DirectionsSimilarity21_g7 - SqrtRatio37_g7 );
				float3 ExitPoint44_g7 = ( CamPosition11_g7 + ( ViewDirection12_g7 * max( temp_output_41_0_g7 , DistanceToPixel114_g7 ) ) );
				float DistanceToEntryPoint139_g7 = temp_output_40_0_g7;
				float3 EntryPoint45_g7 = ( ( DistanceToEntryPoint139_g7 * ViewDirection12_g7 ) + CamPosition11_g7 );
				float temp_output_170_0_g7 = ( 1.0 - ( distance( SphereCenter4_g7 , ( ( ExitPoint44_g7 + EntryPoint45_g7 ) * 0.5 ) ) / SphereRadius5_g7 ) );
				float LinearDensity173_g7 = temp_output_170_0_g7;
				float4 appendResult136 = (float4(tex2D( _MainTex, i.uv.xy ).rgb , pow( ( ( RejectionValue162_g7 < 0.0 ? 0.0 : LinearDensity173_g7 ) * _MaskDensity ) , _MaskExponent )));
				

				finalColor = appendResult136;

				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "AmplifyShaderEditor.MaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19902
Node;AmplifyShaderEditor.Vector3Node, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;4;380.8583,44.91288;Float;False;Global;_SpherePosition;_SpherePosition;7;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;10;379.863,211.7047;Float;False;Global;_SphereRadius;_SphereRadius;2;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;140;220.753,-42.58594;Inherit;False;Reconstruct World Position From Depth;-1;;1;e7094bcbcc80eb140b2a3dbe6a861de8;0;0;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;8;993.168,198.614;Float;False;Global;_MaskDensity;_MaskDensity;2;0;Create;True;0;0;0;False;0;False;1;0;0;3;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;141;724.8454,-38.27131;Inherit;False;Volumetric Pixelization;-1;;7;21b394060dee4e24d99ea8f6db991160;2,164,1,154,1;3;3;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;7;FLOAT;97;FLOAT;178;FLOAT;88;FLOAT;0;FLOAT3;68;FLOAT3;75;INT;53
Node;AmplifyShaderEditor.SimpleMultiplyOpNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;9;1362.593,152.5419;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;137;1240.171,296.0089;Float;False;Global;_MaskExponent;_MaskExponent;2;0;Create;True;0;0;0;False;0;False;3;0;0.2;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;131;752.8643,-365.0398;Inherit;False;0;0;_MainTex;Shader;False;0;5;SAMPLER2D;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexCoordVertexDataNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;1;673.2322,-285.6174;Inherit;False;0;2;0;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;139;1581.171,162.0089;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;132;937.8646,-362.0398;Inherit;True;Property;_TextureSample0;Texture Sample 0;4;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;False;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;6;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT3;5
Node;AmplifyShaderEditor.DynamicAppendNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;136;1872.132,-94.7847;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StickyNoteNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;142;704,272;Inherit;False;407.3646;154.2742;New Note;;1,1,1,1;AmplifyShaderPack/Community/VolumetricPixelizationScreenAndMask$$Hidden/PostProcess/Pixelize/ScreenAndMask;0;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode, AmplifyShaderEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null;0;2109.486,-96.0928;Float;False;True;-1;2;AmplifyShaderEditor.MaterialInspector;0;9;Hidden/AmplifyShaderPack/Community/VolumetricPixelizationScreenAndMask;c71b220b631b6344493ea3cf87110c93;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;True;7;False;;False;True;0;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;3;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;141;3;140;0
WireConnection;141;1;4;0
WireConnection;141;2;10;0
WireConnection;9;0;141;97
WireConnection;9;1;8;0
WireConnection;139;0;9;0
WireConnection;139;1;137;0
WireConnection;132;0;131;0
WireConnection;132;1;1;0
WireConnection;136;0;132;0
WireConnection;136;3;139;0
WireConnection;0;0;136;0
ASEEND*/
//CHKSM=48150CDCD8F8974C1159FCBB01906FF7A079FAEB
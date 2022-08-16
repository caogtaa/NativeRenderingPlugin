// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Hidden/WorldTexelDensity"
{
	Properties
	{
		_TextureSample3("Texture Sample 3", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
			float2 uv_texcoord;
		};

		uniform sampler2D WorldGrid;
		uniform float MetersPerUnit;
		uniform sampler2D _TextureSample3;
		uniform float4 _TextureSample3_ST;

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_worldPos = i.worldPos;
			float3 break249 = ( ase_worldPos / max( MetersPerUnit , 1E-05 ) );
			float2 appendResult77 = (float2(break249.x , break249.y));
			float4 tex2DNode285 = tex2D( WorldGrid, appendResult77 );
			float2 appendResult79 = (float2(break249.z , break249.y));
			float4 tex2DNode286 = tex2D( WorldGrid, appendResult79 );
			float3 ase_worldNormal = i.worldNormal;
			float3 temp_output_292_0 = abs( ase_worldNormal );
			float3 break293 = temp_output_292_0;
			float3 normalizeResult298 = normalize( ( pow( temp_output_292_0 , 32.0 ) / ( break293.x + break293.y + break293.z ) ) );
			float3 break296 = normalizeResult298;
			float4 lerpResult284 = lerp( tex2DNode285 , tex2DNode286 , break296.x);
			float2 appendResult75 = (float2(break249.x , break249.z));
			float4 tex2DNode252 = tex2D( WorldGrid, appendResult75 );
			float4 lerpResult287 = lerp( lerpResult284 , tex2DNode252 , break296.y);
			o.Albedo = lerpResult287.rgb;
			o.Emission = ( lerpResult287 * float4( 0.03921569,0.03921569,0.03921569,0 ) ).rgb;
			float2 uv_TextureSample3 = i.uv_texcoord * _TextureSample3_ST.xy + _TextureSample3_ST.zw;
			o.Gloss = tex2Dlod( _TextureSample3, float4( uv_TextureSample3, 0, 2.14) ).r;
			o.Alpha = 1;
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=18301
-2376;312;2272;1297;636.746;265.3263;1;True;True
Node;AmplifyShaderEditor.WorldNormalVector;86;-1655.774,1221.306;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;247;-2278.337,558.5964;Inherit;False;Global;MetersPerUnit;MetersPerUnit;1;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;292;-1469.712,1223.377;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;293;-1347.156,1312.197;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WorldPosInputsNode;74;-2120.204,412.0974;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMaxOpNode;251;-2055.337,557.5963;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1E-05;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;250;-1901.337,477.5964;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;297;-1112.003,1212.007;Inherit;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;32;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;294;-1089.057,1314.297;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;295;-955.9547,1241.515;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;249;-1780.336,475.5964;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.NormalizeNode;298;-753.8261,1243.529;Inherit;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;122;-1002.648,931.217;Inherit;True;Global;WorldGrid;WorldGrid;0;0;Create;True;0;0;False;0;False;a2cd95debdbc7584faea5455103d2c37;;False;white;Auto;Texture2D;-1;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.DynamicAppendNode;79;-1489.608,660.2385;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;77;-1486.187,490.2953;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;285;-527.5394,379.5269;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;False;0;False;-1;04f191d4d848fc24d929eac03882a43c;04f191d4d848fc24d929eac03882a43c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;296;-519.8421,1240.843;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;75;-1481.202,369.0973;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;286;-595.4705,738.1406;Inherit;True;Property;_TextureSample2;Texture Sample 2;0;0;Create;True;0;0;False;0;False;-1;04f191d4d848fc24d929eac03882a43c;04f191d4d848fc24d929eac03882a43c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;284;71.50696,455.9895;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;252;-473.8749,-41.20101;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;False;-1;04f191d4d848fc24d929eac03882a43c;04f191d4d848fc24d929eac03882a43c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;318;281.254,146.6737;Inherit;False;Constant;_Float0;Float 0;2;0;Create;True;0;0;False;0;False;2.14;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;287;425.41,479.4818;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;281;-1047.427,-189.119;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;310;-909.2777,229.2726;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.BreakToComponentsNode;272;-915.5892,-186.8445;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;303;-805.2358,609.5444;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;311;-666.6785,232.2726;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;305;-563.6741,626.27;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;312;-545.1171,236.9982;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;273;-672.9893,-183.8446;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;282;-431.4276,-167.119;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;315;-127.8001,304.3931;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;316;690.2544,555.6737;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0.03921569,0.03921569,0.03921569,0;False;1;COLOR;0
Node;AmplifyShaderEditor.BreakToComponentsNode;302;-1047.835,606.5444;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.FloorOpNode;308;-1178.477,227.8725;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;301;-1179.672,604.27;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;278;-283.2062,-146.3832;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;309;-1041.115,226.9982;Inherit;False;2;0;FLOAT2;0,0;False;1;FLOAT2;2,2;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;279;-134.1114,-111.7242;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;306;-416.4521,646.0058;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;314;-277.8947,268.734;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.95;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;317;470.254,95.67371;Inherit;True;Property;_TextureSample3;Texture Sample 3;1;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;MipLevel;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;307;-266.3574,681.6648;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FractNode;304;-683.6743,614.27;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FractNode;283;-551.4278,-179.119;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.FloorOpNode;276;-1184.789,-188.2446;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FloorOpNode;300;-1317.034,605.1443;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;313;-425.1167,248.9982;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;850.5657,430.1035;Float;False;True;-1;2;;0;0;Lambert;Hidden/WorldTexelDensity;False;False;False;False;True;True;True;True;True;True;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;292;0;86;0
WireConnection;293;0;292;0
WireConnection;251;0;247;0
WireConnection;250;0;74;0
WireConnection;250;1;251;0
WireConnection;297;0;292;0
WireConnection;294;0;293;0
WireConnection;294;1;293;1
WireConnection;294;2;293;2
WireConnection;295;0;297;0
WireConnection;295;1;294;0
WireConnection;249;0;250;0
WireConnection;298;0;295;0
WireConnection;79;0;249;2
WireConnection;79;1;249;1
WireConnection;77;0;249;0
WireConnection;77;1;249;1
WireConnection;285;0;122;0
WireConnection;285;1;77;0
WireConnection;296;0;298;0
WireConnection;75;0;249;0
WireConnection;75;1;249;2
WireConnection;286;0;122;0
WireConnection;286;1;79;0
WireConnection;284;0;285;0
WireConnection;284;1;286;0
WireConnection;284;2;296;0
WireConnection;252;0;122;0
WireConnection;252;1;75;0
WireConnection;287;0;284;0
WireConnection;287;1;252;0
WireConnection;287;2;296;1
WireConnection;281;0;276;0
WireConnection;310;0;309;0
WireConnection;272;0;281;0
WireConnection;303;0;302;0
WireConnection;303;1;302;1
WireConnection;311;0;310;0
WireConnection;311;1;310;1
WireConnection;305;0;304;0
WireConnection;312;0;311;0
WireConnection;273;0;272;0
WireConnection;273;1;272;1
WireConnection;282;0;283;0
WireConnection;315;0;314;0
WireConnection;315;1;285;0
WireConnection;316;0;287;0
WireConnection;302;0;301;0
WireConnection;308;0;77;0
WireConnection;301;0;300;0
WireConnection;278;0;282;0
WireConnection;309;0;308;0
WireConnection;279;0;278;0
WireConnection;279;1;252;0
WireConnection;306;0;305;0
WireConnection;314;0;313;0
WireConnection;317;2;318;0
WireConnection;307;0;306;0
WireConnection;307;1;286;0
WireConnection;304;0;303;0
WireConnection;283;0;273;0
WireConnection;276;0;75;0
WireConnection;300;0;79;0
WireConnection;313;0;312;0
WireConnection;0;0;287;0
WireConnection;0;2;316;0
WireConnection;0;4;317;0
ASEEND*/
//CHKSM=A460FAE83600B65464AC83A45B1B5188E0F448E8
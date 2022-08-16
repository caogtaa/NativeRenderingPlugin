Shader "Hidden/CopyAlpha"
{
	Properties
    {
        //_MainTex ("Texture To Copy", 2D) = "black" {}
        _MainTexArray ("Array Texture To Copy", 2DArray) = "black" {}
        _Slice ("Slice", float) = 0
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma require 2darray
            #include "UnityCG.cginc"

            struct v2f
            {
                float3 uv : TEXCOORD0; // texture coordinate
                float4 vertex : SV_POSITION; // clip space position
            };

            float _Slice;

            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(vertex);
                o.uv.xy = vertex.xy;
                o.uv.z = _Slice;
                return o;
            }
            
            //sampler2D _MainTex;
            UNITY_DECLARE_TEX2DARRAY(_MainTexArray);

            fixed4 frag (v2f i) : SV_Target
            {   
                //float4 outAlpha = tex2D(_MainTexArray, i.uv).a;
                float4 outAlpha = UNITY_SAMPLE_TEX2DARRAY(_MainTexArray, i.uv).a;
                outAlpha.a = outAlpha.r;
                return outAlpha;
            }
            ENDCG
        }
    }
}
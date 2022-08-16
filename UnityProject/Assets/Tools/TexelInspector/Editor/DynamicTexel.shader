Shader "DynamicTexel"
{
	Properties
    {
        _MainTex ("Grid Texture", 2D) = "white" {}
        _InputTexture ("Input Texture", 2D) = "black" {}
        _Text ("Text", 2D) = "black" {}         // 数字纹理
        _CheckerFade ("Checker Fade", Range (0, 1)) = 0.5
        _ScaleX ("Scale X", float) = 1
        _ScaleY ("Scale Y", float) = 1
        _PixelsX ("Pixels X", int) = 2048
        _PixelsY ("Pixels Y", int) = 2048
        _MipLevel ("Mip Level", int) = 0
        _SourcePixelsX ("Source Pixels X", int) = 2048
        _SourcePixelsY ("Source Pixels Y", int) = 2048
        
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION; // vertex position
                float2 uv : TEXCOORD0; // texture coordinate
            };

            struct v2f
            {
                float2 uv : TEXCOORD0; // texture coordinate
                float4 vertex : SV_POSITION; // clip space position
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            sampler2D _Text;
            sampler2D _InputTexture;
            float _CheckerFade;
            float _ScaleX;
            float _ScaleY;
            int _PixelsX;
            int _PixelsY;
            int _MipLevel;
            int _SourcePixelsX;
            int _SourcePixelsY;
            uniform float4 WorldTexelColor;
            uniform int PixelsPerMeter;
            uniform int UnitCheckers;

            float invLerp(float from, float to, float value)
            {
                return (value - from) / (to - from);
            }

            fixed2 drawDigit(int number, float2 uv, float2 scale, float2 offset)
            {
                float2 digitUvs = float2(saturate(invLerp(offset.x - (0.375 * scale.x), offset.x + (0.375 * scale.x), uv.x)) * 0.1 + (float)number / 10, invLerp(offset.y, offset.y + (1 * scale.y), uv.y));
                if (digitUvs.x > 1 || digitUvs.x < 0 || digitUvs.y > 1 || digitUvs.y < 0)
                    return fixed2(0, 0);
                fixed2 drawnDigit = tex2D(_Text, digitUvs);
                return drawnDigit;
            }

            fixed2 drawResolutionEdge(float valueToDraw, float2 uvs, float aspectRatioOffset)
            {            
                float thousands = valueToDraw / 1000;
                float hundreds = (valueToDraw - floor(thousands) * 1000) / 100;
                float tens = (hundreds * 100 - floor(hundreds) * 100) / 10;
                float ones = (tens * 10 - floor(tens) * 10);
                fixed2 numbers = 0; 
                if (valueToDraw < 10) 
                {
                    numbers += drawDigit(floor(ones), uvs, float2(0.1, 0.1), float2(0.5, 0.02));
                }
                else if (valueToDraw < 100)
                {
                    numbers += drawDigit(floor(tens), uvs, float2(0.1, 0.1), float2(0.475, 0.02));
                    numbers += drawDigit(floor(ones), uvs, float2(0.1, 0.1), float2(0.525, 0.02));
                }
                else if (valueToDraw < 1000)
                {
                    numbers += drawDigit(floor(hundreds), uvs, float2(0.1, 0.1), float2(0.45, 0.02));
                    numbers += drawDigit(floor(tens), uvs, float2(0.1, 0.1), float2(0.5, 0.02));
                    numbers += drawDigit(floor(ones), uvs, float2(0.1, 0.1), float2(0.55, 0.02));
                }
                else
                {
                    numbers += drawDigit(floor(thousands), uvs, float2(0.1, 0.1), float2(0.425, 0.02));
                    numbers += drawDigit(floor(hundreds), uvs, float2(0.1, 0.1), float2(0.475, 0.02));
                    numbers += drawDigit(floor(tens), uvs, float2(0.1, 0.1), float2(0.525, 0.02));
                    numbers += drawDigit(floor(ones), uvs, float2(0.1, 0.1), float2(0.575, 0.02));
                }
                return numbers;               
            }

            fixed4 frag (v2f i) : SV_Target
            {   
                float lightenStrength = 0.5;
                
                fixed2 dimensionNumbers = drawResolutionEdge(_SourcePixelsX, i.uv, _ScaleX / _ScaleY);
                dimensionNumbers += drawResolutionEdge(_SourcePixelsY, float2(i.uv.y, 1 - i.uv.x), _ScaleX / _ScaleY);       
                dimensionNumbers.g = max(1 - dimensionNumbers.g, 0.5);
                dimensionNumbers.r *= 0.65;
                fixed2 mipNumber = drawDigit(_MipLevel, i.uv, float2(0.2, 0.2), float2(0.5, 0.425)) * lightenStrength;
                
                float2 scaledUvs = i.uv * float2(_ScaleX, _ScaleY);
                
                fixed pixels = (floor(i.uv.x * _PixelsX) + floor(i.uv.y * _PixelsY)) % 2;
                
                //float2 pixelUV = scaledUvs * float2(_PixelsX, _PixelsY) * 0.5;        
                //fixed pixels = tex2D(_MainTex, frac(pixelUV)).r;
                
                fixed brick = lerp(pixels, tex2D(_MainTex, frac(scaledUvs)).b, 0.75);
                brick = max(brick, pixels);
                
                fixed4 coloredBrick = brick * WorldTexelColor;
                fixed scaleArrows = tex2D(_MainTex, frac(i.uv)).g * lightenStrength;
                fixed4 tile = coloredBrick + scaleArrows;
                fixed checker = 1 - ((floor(scaledUvs.x) + floor(scaledUvs.y)) % 2);
                fixed scaledChecker = (floor(scaledUvs.x * UnitCheckers) + floor(scaledUvs.y * UnitCheckers)) % 2;
                checker = max(checker, 0.85) * max(scaledChecker, _CheckerFade);
                tile = saturate(saturate(tile * checker) * dimensionNumbers.g) + dimensionNumbers.r + mipNumber.r;
                //tile = saturate(saturate(tile * max(1 - checker, 0.75)) * dimensionNumbers.g) + dimensionNumbers.r + mipNumber.r;
                tile.a = tex2Dlod(_InputTexture, float4(i.uv.x, i.uv.y, 0, _MipLevel)).a;
                return tile;
            }
            ENDCG
        }
    }
}
Shader "Unlit/Bokeh"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Parameter ("Parameter", Float) = 1.0
        _Color1 ("Color 1", Color) = (0.4, 0.1, 0.2, 1)
        _Color2 ("Color 2", Color) = (0.6, 0.4, 0.2, 1)
        _Color3 ("Color 3", Color) = (0.4, 0.3, 0.2, 1)
        _Color4 ("Color 4", Color) = (0.4, 0.2, 0.1, 1)
        _Color5 ("Color 5", Color) = (0.2, 0.0, 0.4, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Transparent"
            "Queue" = "Transparent"
        }
        LOD 100

        Blend SrcAlpha One
        ZWrite Off
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            void Rotate(inout float2 p, float a)
            {
                p = cos(a) * p + sin(a) * float2(p.y, -p.x);
            }

            float Circle(float2 p, float r)
            {
                return (length(p / r) - 1.0f) * r;
            }

            float Rand(float2 c)
            {
                return frac(sin(dot(c.xy, float2(12.9898f, 78.233f))) * 43758.5453f);
            }

            float saturate(float x)
            {
                return clamp(x, 0.0f, 1.0f);
            }

            float2 mod(float2 x, float y)
            {
                return x - y * floor(x/y);
            }

            float mod1(float x, float y)
            {
                return x - y * floor(x/y);
            }

            void BokehLayer(inout float3 color, float2 p, float3 c)
            {
                float wrap = 450.0f;
                if (mod1(floor(p.y / wrap + 0.5f), 2.0f) == 0.0f)
                {
                    p.x += wrap * 0.5f;
                }

                float2 p2 = mod(p + 0.5f * wrap, wrap) - 0.5f * wrap;
                float2 cell = floor(p / wrap + 0.5f);
                float cellR = Rand(cell);

                c *= frac(cellR * 3.33f + 3.33f);
                float radius = lerp(30.0f, 70.0f, frac(cellR * 7.77f + 7.77f));
                p2.x *= lerp(0.9f, 1.1f, frac(cellR * 11.13f + 11.13f));
                p2.y *= lerp(0.9f, 1.1f, frac(cellR * 17.17f + 17.17f));

                float sdf = Circle(p2, radius);
                float circle = 1.0f - smoothstep(0.0f, 1.0f, sdf * 0.04f);
                float glow = exp(-sdf * 0.025f) * 0.3f * (1.0f - circle);
                color += c * (circle + glow);
            }
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Parameter;
            float4 _Color1;
            float4 _Color2;
            float4 _Color3;
            float4 _Color4;
            float4 _Color5;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 p = (2.0f * i.uv - 1.0) * 1000;

                float3 color = 0.0;

                float time = _Parameter - 15.0f;

                Rotate(p, 0.2f + time * 0.03f);
                BokehLayer(color, p + float2(-50.0f * time + 0.0f, 0.0f), 3.0f * _Color1.rgb);
                Rotate(p, 0.3f - time * 0.05f);
                BokehLayer(color, p + float2(-70.0f * time + 33.0f, -33.0f), 3.5f * _Color2.rgb);
                Rotate(p, 0.5f + time * 0.07f);
                BokehLayer(color, p + float2(-60.0f * time + 55.0f, 55.0f), 3.0f * _Color3.rgb);
                Rotate(p, 0.9f - time * 0.03f);
                BokehLayer(color, p + float2(-25.0f * time + 77.0f, 77.0f), 3.0f * _Color4.rgb);
                Rotate(p, 0.0f + time * 0.05f);
                BokehLayer(color, p + float2(-15.0f * time + 99.0f, 99.0f), 3.0f * _Color5.rgb);

                return float4(color, 1.0f);
            }
            ENDHLSL
        }
    }
}
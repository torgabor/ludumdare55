Shader "Universal Render Pipeline/2D/Speed Lines"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Speed ("Speed", Float) = 1.0
        _Scale ("Scale", Float) = 45.0
        _Intensity ("Intensity", Range(0, 5)) = 2.5
         _ColorA ("Color A", Color) = (0, 0, 1.1, 1)
         _ColorB ("Color B", Color) = (0, 1, 1, 1)
         _ColorC ("Color Accent", Color) = (1, 0.25, 1, 1)
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
        }

        Blend SrcAlpha One
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex vert
            #pragma fragment frag

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float _Speed;
                float _Scale;
                float _Intensity;
                float4 _ColorA;
                float4 _ColorB;
                float4 _ColorC;
            CBUFFER_END

            float Rand(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float Noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                
                float a = Rand(i + float2(0, 0));
                float b = Rand(i + float2(1, 0));
                float c = Rand(i + float2(0, 1));
                float d = Rand(i + float2(1, 1));
                
                return (a + (b-a)*u.x + (c-a)*u.y + (a-b-c+d)*u.x*u.y) / 4.0;
            }

            float Mirror(float t, float r)
            {
                t = frac(t + r);
                return 2.0 * abs(t - 0.5);
            }

            float RadialNoise(float t, float d)
            {
                d = pow(d, 0.01);
                float timeOffset = -_Time.y * _Speed;
                
                float2 p = float2(Mirror(t, 0.1), d + timeOffset);
                float f1 = Noise(p * _Scale);
                
                p = 2.1 * float2(Mirror(t, 0.4), d + timeOffset);
                float f2 = Noise(p * _Scale);
                
                p = 3.7 * float2(Mirror(t, 0.8), d + timeOffset);
                float f3 = Noise(p * _Scale);
                
                p = 5.8 * float2(Mirror(t, 0.0), d + timeOffset);
                float f4 = Noise(p * _Scale);
                
                return pow((f1 + 0.5*f2 + 0.25*f3 + 0.125*f4) * 3.0, 1.0);
            }

            float3 Colorize(float x)
            {
                x = saturate(x);
                float3 c = lerp(_ColorA.rgb, _ColorB.rgb, x);
                c = lerp(c, float3(1,1,1), x*4.0-3.0) * x;
                c = max(c, 0);
                c = lerp(c, _ColorC.rgb, smoothstep(1.0, 0.2, x) * smoothstep(0.15, 0.9, x));
                return c;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = (IN.uv * 2 - 1) * 0.5;
                float d = dot(uv, uv);
                float t = atan2(uv.y, uv.x) / (2 * PI);
                float v = RadialNoise(t, d);
                v = v * _Intensity - 1.4;
                v = lerp(0, v, 0.8 * smoothstep(0.0, 0.8, d));
                
                float3 col = Colorize(v);
                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
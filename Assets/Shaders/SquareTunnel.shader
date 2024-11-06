Shader "Unlit/SquareTunnel"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}        
        _Alpha ("Alpha", Float) = 1.0
        _Color1 ("Color 1", Color) = (1, 0, 0, 1)
        _Color2 ("Color 2", Color) = (0, 1, 0, 1)
        _Speed ("Speed", Float) = 1.0   
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha


        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
            
            float4 _MainTex_ST;
            float4 _Color1;
            float4 _Color2;
            float _Alpha;
            float _Speed;
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv * 2.0 - 1.0;
                //uv.x *= _ScreenParams.x/_ScreenParams.y;
                
                float a = atan2(uv.y, uv.x);
                
                float r = pow(pow(uv.x * uv.x, 4.0) + pow(uv.y * uv.y, 4.0), 1.0/8.0);
                
                float2 p = float2(1.0/r + _Time.y * _Speed, a / 3.14159 * 2.0 + 0.5);
                
                
                float f = smoothstep(0.0, 0.3, frac(p.x*2.0));
                
                float3 col = lerp(_Color1.rgb, _Color2.rgb, f*(smoothstep(0.1, 0.3, max(abs(uv.x), abs(uv.y))))) * clamp(r, 0.0, 1.0);
                
                return float4(col, _Alpha);
            }
            ENDHLSL
        }
    }
}
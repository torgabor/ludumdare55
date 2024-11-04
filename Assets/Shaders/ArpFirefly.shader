Shader "Custom/Firefly"
{
    Properties
    {        
        _Power ("Power", Float) = 4        
        _Color ("Color", Color) = (1,1,1,1)
        _Radius ("Radius", Float) = 0.5
        _MainTex ("Main Texture", 2D) = "white" {}
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
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Power;
                float _Radius;
            CBUFFER_END
            float inverseLerp(float a, float b, float value)
            {
                return (value - a) / (b - a);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }


            float3 calculateFirefly(float2 uv, float radius, float power)
            {                               
                float2 center = float2(0,0);
                float distanceToPoint = distance(uv, center);
                float3 pointLight = pow(radius / distanceToPoint, power);
                
                return pointLight;
            }


            float4 frag(Varyings IN) : SV_Target
            {
                // Center UV coordinates (0,0 at center instead of corner)
                float2 uv = IN.uv * 2.0 - 1.0;
                
                float4 finalColor = _Color;
                // Tint towards white based on alpha above 1
                
                float whiteAmount = saturate(finalColor.a - 1); // 0 to 1 range
                finalColor.rgb = lerp(finalColor.rgb, float3(1,1,1), whiteAmount);
                finalColor.a = min(finalColor.a, 1);
                
                finalColor.rgb *= calculateFirefly(uv, _Radius, _Power);
                return finalColor;  
            }
            ENDHLSL
        }
    }
}

Shader "Rogue Knight/Skeleton Slash"
{
    Properties
    {
        _BaseColor ("Slash Color", Color) = (1, 1, 1, 0.92)
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 position : POSITION;
                float3 uvAndAge : TEXCOORD0;
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float3 uvAndAge : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.position = TransformObjectToHClip(input.position.xyz);
                output.uvAndAge = input.uvAndAge;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float alongSlash = input.uvAndAge.x;
                float acrossSlash = input.uvAndAge.y;
                float age = input.uvAndAge.z;
                float head = age * 3.5 + 0.18;
                float sweep = smoothstep(0.0, 0.10, head - alongSlash);
                float edge = smoothstep(0.0, 0.22, acrossSlash) * (1.0 - smoothstep(0.70, 1.0, acrossSlash));
                float ends = smoothstep(0.0, 0.12, alongSlash) * (1.0 - smoothstep(0.90, 1.0, alongSlash));
                return half4(_BaseColor.rgb, _BaseColor.a * edge * ends * sweep * (1.0 - smoothstep(0.38, 1.0, age)));
            }
            ENDHLSL
        }
    }
}


Shader "Murky Aces/Tank/Fuel Display"
{
    Properties
    {
        [MainTexture] _BaseMap("Display Texture", 2D) = "white" {}

        _Fill("Fuel Fill", Range(0, 1)) = 1
        _FillColor("Filled Color", Color) = (0.05, 0.4, 0.075, 1)
        _EmptyColor("Empty Color", Color) = (0.007, 0.023, 0.01, 1)

        // Координаты области полосы: слева, снизу, справа, сверху.
        _BarRect("Bar Rect", Vector) =
            (0.09375, 0.30078, 0.90625, 0.67578)
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
        }

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite On

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BarRect;
                half4 _FillColor;
                half4 _EmptyColor;
                float _Fill;
            CBUFFER_END

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

            Varyings Vert(Attributes input)
            {
                Varyings output;

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Сначала берём исходную картинку.
                half4 color = SAMPLE_TEXTURE2D(
                    _BaseMap, sampler_BaseMap, input.uv);

                bool insideBar =
                    input.uv.x >= _BarRect.x &&
                    input.uv.y >= _BarRect.y &&
                    input.uv.x < _BarRect.z &&
                    input.uv.y < _BarRect.w;

                if (insideBar)
                {
                    // Положение внутри полосы:
                    // 0 — левый край, 1 — правый край.
                    float barPosition =
                        (input.uv.x - _BarRect.x) /
                        (_BarRect.z - _BarRect.x);

                    color = barPosition < saturate(_Fill)
                        ? _FillColor
                        : _EmptyColor;
                }

                return half4(color.rgb, 1);
            }

            ENDHLSL
        }
    }
}
Shader "Murky Aces/Tank/Night Vision Display"
{
    Properties
    {
        [MainTexture] _BaseMap("Camera Image", 2D) = "black" {}

        _NightVision("Night Vision", Range(0, 1)) = 0
        _Brightness("Night Vision Brightness", Range(1, 8)) = 3
        _ShadowPower("Shadow Power", Range(0.2, 1)) = 0.5
        _Tint("Night Vision Tint", Color) = (0.15, 1, 0.15, 1)
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
                half4 _Tint;
                float _NightVision;
                float _Brightness;
                float _ShadowPower;
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
                float3 original = SAMPLE_TEXTURE2D(
                    _BaseMap, sampler_BaseMap, input.uv).rgb;

                // Яркость исходного изображения.
                float luminance = max(
                    dot(original, float3(0.2126, 0.7152, 0.0722)),
                    0.0);

                // Проявляем тени и усиливаем сигнал.
                float amplified =
                    pow(luminance, _ShadowPower) * _Brightness;

                // Плавно ограничиваем яркость вместо резкого обрезания.
                float intensity = amplified / (1.0 + amplified);

                float3 nightVision = intensity * _Tint.rgb;

                float3 result = lerp(
                    original,
                    nightVision,
                    saturate(_NightVision));

                return half4(result, 1);
            }

            ENDHLSL
        }
    }
}
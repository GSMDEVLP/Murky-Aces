Shader "Murky Aces/Tank/Radar Display"
{
    Properties
    {
        _RadarEnabled("Radar Enabled", Range(0, 1)) = 0

        _OffColor("Off Color", Color) = (0.002, 0.004, 0.002, 1)
        _BackgroundColor("Background Color", Color) = (0.003, 0.012, 0.006, 1)
        _GridColor("Grid Color", Color) = (0.04, 0.3, 0.1, 1)
        _CenterColor("Center Color", Color) = (0.15, 0.8, 0.35, 1)

        _DisplayRadius("Display Radius", Range(0.1, 0.49)) = 0.42
        _LineWidth("Line Width", Range(0.001, 0.03)) = 0.007
        _MarkerRadius("Marker Radius", Range(0.005, 0.08)) = 0.025
        [HideInInspector] _ContactCount("Contact Count", Float) = 0
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

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _OffColor;
                half4 _BackgroundColor;
                half4 _GridColor;
                half4 _CenterColor;

                float _RadarEnabled;
                float _DisplayRadius;
                float _LineWidth;
                float _MarkerRadius;
                float _ContactCount;

                float4 _ContactPositions[64];
                float4 _ContactColors[64];
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

                output.uv = input.uv;

                return output;
            }

            float DrawLine(float signedDistance)
            {
                float smoothing =
                    max(fwidth(signedDistance), 0.0001);

                return 1.0 - smoothstep(
                    _LineWidth,
                    _LineWidth + smoothing,
                    abs(signedDistance));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                // Центр экрана — (0.5, 0.5).
                // На внешнем кольце расстояние от центра равно 1.
                float2 position =
                    (input.uv - float2(0.5, 0.5)) /
                    max(_DisplayRadius, 0.001);

                float radius = length(position);
                float smoothing = max(fwidth(radius), 0.0001);

                float circleMask = 1.0 - smoothstep(
                    1.0 - smoothing,
                    1.0 + smoothing,
                    radius);

                float rings = max(
                    DrawLine(radius - 1.0),
                    max(
                        DrawLine(radius - 0.666667),
                        DrawLine(radius - 0.333333)));

                float cross = max(
                    DrawLine(position.x),
                    DrawLine(position.y));

                float grid = max(rings, cross) * circleMask;
                        
                half3 color = lerp(
                    _BackgroundColor.rgb,
                    _GridColor.rgb,
                    grid);

                float centerMarker = 1.0 - smoothstep(
                    0.025,
                    0.025 + smoothing,
                    radius);

                color = lerp(
                    color,
                    _CenterColor.rgb,
                    centerMarker);
                int contactCount = (int)clamp(_ContactCount, 0.0, 64.0);

                [loop]
                for (int i = 0; i < contactCount; i++)
                {
                    float distanceToContact = length(
                        position - _ContactPositions[i].xy);

                    float markerSmoothing = max(
                        fwidth(distanceToContact),
                        0.0001);

                    float marker = 1.0 - smoothstep(
                        _MarkerRadius,
                        _MarkerRadius + markerSmoothing,
                        distanceToContact);

                    marker *= circleMask;

                    color = lerp(
                        color,
                        _ContactColors[i].rgb,
                        marker);
                }
                
                color = lerp(
                    _OffColor.rgb,
                    color,
                    saturate(_RadarEnabled));

                return half4(color, 1);
            }

            ENDHLSL
        }
    }
}
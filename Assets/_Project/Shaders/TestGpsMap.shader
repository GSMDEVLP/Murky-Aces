Shader "Murky Aces/Test GPS Map"
{
    Properties
    {
        [MainTexture] _BaseMap("Map", 2D) = "white" {}

        _TankColor("Tank Color", Color) = (0.1, 0.8, 1, 1)
        _OutsideColor("Outside Map Color", Color) = (0.015, 0.02, 0.018, 1)
        _MarkerSize("Marker Size In Pixels", Range(4, 24)) = 10

        [HideInInspector] _TankUv("Tank UV", Vector) = (0.5, 0.5, 0, 0)
        [HideInInspector] _TankForward("Tank Forward", Vector) = (0, 1, 0, 0)
        [HideInInspector] _MapSize("Map Size", Vector) = (200, 200, 0, 0)
        [HideInInspector] _ViewSize("View Size", Vector) = (106.6667, 80, 0, 0)
        [HideInInspector] _MarkerVisible("Marker Visible", Float) = 0
        _CursorColor("Cursor Color", Color) = (1, 1, 1, 1)
        _CursorSize("Cursor Size", Range(4, 24)) = 8

        [HideInInspector] _CursorUv("Cursor UV", Vector) = (0.5, 0.5, 0, 0)
        [HideInInspector] _CursorVisible("Cursor Visible", Float) = 0
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
                float4 _BaseMap_TexelSize;
                half4 _TankColor;
                half4 _OutsideColor;

                float4 _TankUv;
                float4 _TankForward;
                float4 _MapSize;
                float4 _ViewSize;

                float _MarkerSize;
                float _MarkerVisible;

                half4 _CursorColor;
                float4 _CursorUv;
                float _CursorSize;
                float _CursorVisible;
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

            half4 Frag(Varyings input) : SV_Target
            {
                float2 mapUv = input.uv;
                float2 viewSize = max(_ViewSize.xy, 0.001);

                if (_MarkerVisible > 0.5)
                {
                    float2 forward = normalize(_TankForward.xy);
                    float2 right = float2(forward.y, -forward.x);
                    float2 mapSize = max(_MapSize.xy, 0.001);

                    float2 screenOffset =
                        (input.uv - float2(0.5, 0.5)) * viewSize;

                    float2 worldOffset =
                        right * screenOffset.x +
                        forward * screenOffset.y;

                    mapUv = _TankUv.xy + worldOffset / mapSize;
                }

                half3 color = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    saturate(mapUv)).rgb;

                // За границами локации отображаем пустой фон.
                float2 boundsMask =
                    step(0.0, mapUv) * step(mapUv, 1.0);

                color = lerp(
                    _OutsideColor.rgb,
                    color,
                    boundsMask.x * boundsMask.y);

                if (_MarkerVisible < 0.5)
                    return half4(color, 1);

                float aspect = viewSize.x / max(viewSize.y, 0.001);
                float markerSize = max(_MarkerSize, 1.0);
                float textureHeight = max(_BaseMap_TexelSize.w, 1.0);

                float2 markerOffset =
                    (input.uv - float2(0.5, 0.5)) *
                    float2(aspect, 1.0) *
                    textureHeight /
                    markerSize;

                float along = markerOffset.y;
                float across = abs(markerOffset.x);

                float edge = max(
                    max(-0.6 - along, along - 1.0),
                    across - (1.0 - along) * 0.375);

                float smoothing = max(fwidth(edge), 0.0001);

                float marker = 1.0 - smoothstep(
                    0.0, smoothing, edge);

                color = lerp(color, _TankColor.rgb, marker);
                if (_CursorVisible > 0.5)
                {
                    float2 cursorOffset = abs(
                        (input.uv - _CursorUv.xy) *
                        float2(aspect, 1.0) *
                        textureHeight);

                    float armLength = max(_CursorSize, 2.0);

                    float horizontalEdge = max(
                        cursorOffset.x - armLength,
                        cursorOffset.y - 1.0);

                    float verticalEdge = max(
                        cursorOffset.x - 1.0,
                        cursorOffset.y - armLength);

                    float cursorEdge = min(horizontalEdge, verticalEdge);

                    float cursorSmoothing = max(
                        fwidth(cursorEdge),
                        0.0001);

                    float cursor = 1.0 - smoothstep(
                        0.0,
                        cursorSmoothing,
                        cursorEdge);

                    color = lerp(
                        color,
                        _CursorColor.rgb,
                        cursor * _CursorColor.a);
                }

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}

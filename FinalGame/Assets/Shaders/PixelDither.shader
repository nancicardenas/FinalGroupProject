Shader "Custom/GhostPixelDither"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (1.0, 0.67, 0.47, 0.6)
        _DitherScale("Dither Scale", Float) = 3.0
        _DitherSpeed("Dither Shift Speed", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                // Screen position for pixel-grid dither
                float4 screenPos  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                // Controls how large the dither cells are (higher = finer pattern)
                float _DitherScale;
                // How fast the dither pattern shifts (creates shimmer)
                float _DitherSpeed;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // ComputeScreenPos gives screen UV that we use for the pixel grid
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Perspective-correct screen UVs
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                // Scale to screen resolution so dither is pixel-stable
                float2 pixelPos = screenUV * _ScreenParams.xy / _DitherScale;

                // Checkerboard: floor both axes, sum, check even/odd
                float checker = fmod(floor(pixelPos.x) + floor(pixelPos.y), 2.0);

                // Shift which pixels are "on" over time so the pattern shimmers
                float timeShift = fmod(floor(_Time.y * _DitherSpeed), 2.0);
                // XOR the checker with time: alternates which pixels are visible each tick
                float pattern = abs(checker - timeShift);

                half4 color = _BaseColor;
                // Visible pixels get full alpha, dithered pixels get reduced alpha
                color.a = _BaseColor.a * (0.3 + 0.7 * pattern);

                return color;
            }
            ENDHLSL
        }
    }
}
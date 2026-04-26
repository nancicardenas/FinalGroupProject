Shader "Custom/GhostPulseGlow"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (0.4, 1.0, 1.0, 0.55)
        _PulseSpeed("Pulse Speed", Float) = 2.0
        _PulseAmount("Pulse Amount", Range(0, 0.5)) = 0.2
    }

    SubShader
    {
        // Transparent so we can see through the ghost
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        // Alpha blending: source color * source alpha + dest color * (1 - source alpha)
        Blend SrcAlpha OneMinusSrcAlpha
        // Don't write to depth buffer — ghost shouldn't occlude other transparent objects
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
            };

            CBUFFER_START(UnityPerMaterial)
                // Ghost tint color — alpha channel controls base transparency
                half4 _BaseColor;
                // How fast the brightness oscillates (cycles per second)
                float _PulseSpeed;
                // How much the alpha varies from center (0 = steady, 0.5 = dramatic)
                float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // sin() oscillates -1 to 1 at PulseSpeed Hz
                // Remap to 0-1 then scale by PulseAmount
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float alphaOffset = pulse * _PulseAmount;

                half4 color = _BaseColor;
                // Add the pulse to base alpha, clamp so we never exceed 1 or go below 0
                color.a = clamp(_BaseColor.a + alphaOffset, 0.0, 1.0);

                // Brighten the RGB slightly at pulse peak for a glow feel
                color.rgb *= 1.0 + pulse * 0.3;

                return color;
            }
            ENDHLSL
        }
    }
}
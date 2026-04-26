Shader "Custom/GhostStripeFade"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (0.27, 0.6, 1.0, 0.55)
        _StripeCount("Stripe Count", Float) = 12.0
        _StripeSpeed("Stripe Scroll Speed", Float) = 1.0
        _StripeFade("Stripe Fade Amount", Range(0, 0.5)) = 0.25
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
                // Object-space Y for stripe calculation
                float objectY : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                // How many stripes fit across the mesh height
                float _StripeCount;
                // How fast stripes scroll downward
                float _StripeSpeed;
                // How much dimmer the dark stripes are (0 = no stripes, 0.5 = dramatic)
                float _StripeFade;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.objectY = IN.positionOS.y;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // sin() creates repeating bands. Multiply Y by StripeCount for density.
                // Subtract time to scroll downward.
                float stripe = sin((IN.objectY * _StripeCount - _Time.y * _StripeSpeed) * 6.2832);

                // Remap sin from (-1,1) to (0,1)
                stripe = stripe * 0.5 + 0.5;

                half4 color = _BaseColor;
                // Dark stripes reduce alpha, bright stripes keep full alpha
                color.a = _BaseColor.a - stripe * _StripeFade;
                color.a = clamp(color.a, 0.05, 1.0);

                return color;
            }
            ENDHLSL
        }
    }
}
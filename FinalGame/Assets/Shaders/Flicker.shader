Shader "Custom/GhostFlicker"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (0.67, 0.53, 1.0, 0.55)
        _FlickerSpeed("Flicker Speed", Float) = 15.0
        _FlickerIntensity("Flicker Intensity", Range(0, 1)) = 0.4
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
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                // How many flicker ticks per second
                float _FlickerSpeed;
                // How much alpha drops during a flicker (0 = none, 1 = full disappear)
                float _FlickerIntensity;
            CBUFFER_END

            // Pseudo-random hash: takes a single float, returns 0-1
            float rand(float x)
            {
                return frac(sin(x * 127.1) * 43758.5453);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // floor() creates discrete time steps — each step holds for 1/FlickerSpeed seconds
                // This makes the entire mesh flicker in unison (not per-pixel)
                float tick = floor(_Time.y * _FlickerSpeed);

                // Random value for this tick: some ticks bright, some dim
                float r = rand(tick);

                half4 color = _BaseColor;
                // Subtract a random portion of alpha. Some frames nearly invisible, some normal.
                color.a = _BaseColor.a - r * _FlickerIntensity;
                // Clamp to avoid negative alpha
                color.a = max(color.a, 0.05);

                return color;
            }
            ENDHLSL
        }
    }
}
Shader "Custom/GhostWaveDistort"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (1.0, 0.8, 0.27, 0.55)
        _WaveSpeed("Wave Speed", Float) = 3.0
        _WaveAmplitude("Wave Amplitude", Float) = 0.08
        _WaveFrequency("Wave Frequency", Float) = 8.0
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
                // How fast the wave scrolls through the mesh
                float _WaveSpeed;
                // How far vertices move sideways (object-space units)
                float _WaveAmplitude;
                // How many wave peaks fit along the mesh height
                float _WaveFrequency;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // Offset X position using a sine wave based on Y height and time
                // Each vertex moves left/right depending on how high it is on the mesh
                float wave = sin(IN.positionOS.y * _WaveFrequency + _Time.y * _WaveSpeed);
                float4 displaced = IN.positionOS;
                displaced.x += wave * _WaveAmplitude;

                OUT.positionHCS = TransformObjectToHClip(displaced.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
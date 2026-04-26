Shader "Custom/GhostNoiseDissolve"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (0.8, 0.87, 1.0, 0.6)
        _NoiseScale("Noise Scale", Float) = 10.0
        _DissolveSpeed("Dissolve Speed", Float) = 1.5
        _DissolveEdge("Edge Width", Range(0.01, 0.3)) = 0.08
        _EdgeBrightness("Edge Brightness", Float) = 3.0
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
                // Object-space position for 3D noise lookup
                float3 objectPos  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                // Controls noise granularity (higher = finer grain)
                float _NoiseScale;
                // How fast the dissolve threshold oscillates
                float _DissolveSpeed;
                // Width of the bright edge at the dissolve boundary
                float _DissolveEdge;
                // How bright the dissolve edge is (multiplier)
                float _EdgeBrightness;
            CBUFFER_END

            // Simple 3D noise from two 2D hashes blended by Z
            float hash2D(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise3D(float3 p)
            {
                // Use XY plane hash at two Z levels, then lerp between them
                float z0 = floor(p.z);
                float z1 = z0 + 1.0;
                float fz = frac(p.z);

                float n0 = hash2D(p.xy + z0 * 17.0);
                float n1 = hash2D(p.xy + z1 * 17.0);

                // smoothstep interpolation between the two layers
                return lerp(n0, n1, fz * fz * (3.0 - 2.0 * fz));
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Pass object position for noise sampling (so pattern moves with mesh)
                OUT.objectPos = IN.positionOS.xyz;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample noise at object position scaled for granularity
                float n = noise3D(IN.objectPos * _NoiseScale);

                // Dissolve threshold oscillates 0 to 1 and back
                // sin gives -1 to 1, remap to 0-1
                float threshold = sin(_Time.y * _DissolveSpeed) * 0.5 + 0.5;

                // Pixels with noise below threshold are dissolved (transparent)
                // Pixels near the boundary get a bright edge
                float diff = n - threshold;

                half4 color = _BaseColor;

                if (diff < 0.0)
                {
                    // Below threshold: dissolved — make transparent
                    color.a = 0.0;
                }
                else if (diff < _DissolveEdge)
                {
                    // In the edge band: bright glow at dissolve boundary
                    float edgeFactor = 1.0 - (diff / _DissolveEdge);
                    color.rgb *= _EdgeBrightness * edgeFactor;
                    color.a = _BaseColor.a;
                }
                // else: normal ghost color at full base alpha

                return color;
            }
            ENDHLSL
        }
    }
}
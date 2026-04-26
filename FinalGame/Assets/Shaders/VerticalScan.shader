Shader "Custom/GhostVerticalScan"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (0.4, 1.0, 0.6, 0.55)
        _ScanSpeed("Scan Speed", Float) = 1.5
        _ScanWidth("Scan Width", Range(0.01, 0.3)) = 0.08
        _ScanBrightness("Scan Brightness", Float) = 1.5
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
                // Object-space Y passed to fragment for scanline position
                float objectY : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                // How fast the scanline sweeps upward
                float _ScanSpeed;
                // How thick the bright band is (in object-space units, normalized 0-1)
                float _ScanWidth;
                // How much brighter the scanline is (multiplier on RGB)
                float _ScanBrightness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                // Pass object-space Y so scanline stays attached to the mesh
                // Remap from local coords to roughly 0-1 range
                OUT.objectY = IN.positionOS.y;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // frac() keeps the scanline position cycling 0 to 1
                float scanPos = frac(_Time.y * _ScanSpeed);

                // Distance from this pixel's Y to the scanline center
                // frac(objectY) normalizes the mesh height to 0-1
                float dist = abs(frac(IN.objectY) - scanPos);
                // Handle wrap-around (when scanline is near 0 and pixel is near 1)
                dist = min(dist, 1.0 - dist);

                // smoothstep creates a soft-edged band: 1.0 at center, 0.0 outside ScanWidth
                float scanline = 1.0 - smoothstep(0.0, _ScanWidth, dist);

                half4 color = _BaseColor;
                // Brighten pixels inside the scanline band
                color.rgb *= 1.0 + scanline * (_ScanBrightness - 1.0);
                // Slightly boost alpha in the scanline for extra visibility
                color.a = _BaseColor.a + scanline * 0.2;
                color.a = clamp(color.a, 0.0, 1.0);

                return color;
            }
            ENDHLSL
        }
    }
}
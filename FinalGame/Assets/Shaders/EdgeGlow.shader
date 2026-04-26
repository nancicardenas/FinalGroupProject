Shader "Custom/GhostEdgeGlow"
{
    Properties
    {
        [MainColor] _BaseColor("Ghost Color", Color) = (1.0, 0.47, 0.53, 0.45)
        _EdgeColor("Edge Glow Color", Color) = (1.0, 0.8, 0.85, 1.0)
        _EdgePower("Edge Power", Range(0.5, 5.0)) = 2.0
        _EdgeIntensity("Edge Intensity", Range(0, 3)) = 1.5
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
                // We need normals to compute the view-angle fresnel
                float3 normalOS  : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                // World-space normal interpolated across triangle
                float3 normalWS   : TEXCOORD0;
                // World-space view direction (camera to vertex)
                float3 viewDirWS  : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                // Color of the bright rim
                half4 _EdgeColor;
                // Higher power = thinner edge (more concentrated at silhouette)
                float _EdgePower;
                // Multiplier on edge brightness
                float _EdgeIntensity;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Transform normal from object space to world space
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // Compute world position, then direction from vertex to camera
                float3 posWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - posWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(IN.viewDirWS);

                // Fresnel: dot product of normal and view direction
                // At edges (silhouette), normal is perpendicular to view → dot ≈ 0 → fresnel ≈ 1
                // At center (facing camera), dot ≈ 1 → fresnel ≈ 0
                float NdotV = saturate(dot(normal, viewDir));
                float fresnel = pow(1.0 - NdotV, _EdgePower) * _EdgeIntensity;

                // Blend base color with edge glow based on fresnel
                half4 color;
                color.rgb = _BaseColor.rgb + _EdgeColor.rgb * fresnel;
                // Boost alpha at edges so the rim is more visible
                color.a = _BaseColor.a + fresnel * 0.3;
                color.a = clamp(color.a, 0.0, 1.0);

                return color;
            }
            ENDHLSL
        }
    }
}
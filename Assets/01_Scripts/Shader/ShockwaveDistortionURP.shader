Shader "Custom/URP/ShockwaveDistortion"
{
    Properties
    {
        _NoiseTex("Noise Texture", 2D) = "bump" {}
        _TintColor("Tint Color", Color) = (0.6, 0.9, 1.0, 1.0)

        _DistortionStrength("Distortion Strength", Range(0, 1)) = 0.15
        _RingRadius("Ring Radius", Range(0, 1)) = 0.25
        _RingWidth("Ring Width", Range(0.001, 0.5)) = 0.08
        _EdgeGlow("Edge Glow", Range(0, 10)) = 2.0
        _Opacity("Opacity", Range(0, 1)) = 1.0

        _NoiseTiling("Noise Tiling", Float) = 2.0
        _NoiseSpeed("Noise Speed", Float) = 0.5
        _RadialPush("Radial Push", Range(-1, 1)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _NoiseTex_ST;
                float4 _TintColor;

                float _DistortionStrength;
                float _RingRadius;
                float _RingWidth;
                float _EdgeGlow;
                float _Opacity;

                float _NoiseTiling;
                float _NoiseSpeed;
                float _RadialPush;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float3 normalWS   : TEXCOORD2;
                float3 viewDirWS  : TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                OUT.uv = IN.uv;
                OUT.normalWS = normalize(normalInputs.normalWS);

                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = normalize(GetCameraPositionWS() - worldPos);

                return OUT;
            }

            float RingMask(float2 uv, float radius, float width)
            {
                float2 centered = uv - 0.5;
                float dist = length(centered);

                float inner = smoothstep(radius - width, radius, dist);
                float outer = 1.0 - smoothstep(radius, radius + width, dist);

                return saturate(inner * outer);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                float2 centered = uv - 0.5;
                float dist = length(centered);

                // 防止 normalize(0,0)
                float2 radialDir = (dist > 0.0001) ? centered / dist : float2(0, 0);

                // 环状冲击波遮罩
                float ring = RingMask(uv, _RingRadius, _RingWidth);

                // 噪声扰动
                float2 noiseUV = uv * _NoiseTiling + float2(0, _Time.y * _NoiseSpeed);
                float4 noiseSample = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV);

                // 把 [0,1] 映射到 [-1,1]
                float2 noiseVec = noiseSample.xy * 2.0 - 1.0;

                // 径向推动 + 噪声扰动
                float2 distortionOffset =
                    (radialDir * _RadialPush + noiseVec) *
                    _DistortionStrength *
                    ring;

                // 屏幕 UV
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                // 采样被扭曲后的场景颜色
                float2 distortedScreenUV = screenUV + distortionOffset;
                half3 sceneColor = SampleSceneColor(distortedScreenUV);

                // 做一个亮边，增强“能量壳”感觉
                float edge = pow(ring, 0.6) * _EdgeGlow;

                // 简单 fresnel 提升边缘感
                float fresnel = pow(1.0 - saturate(dot(normalize(IN.normalWS), normalize(IN.viewDirWS))), 3.0);

                half3 finalColor = sceneColor + (_TintColor.rgb * edge) + (_TintColor.rgb * fresnel * 0.2 * ring);

                // alpha 主要由 ring 决定
                float alpha = ring * _Opacity;

                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
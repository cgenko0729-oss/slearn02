Shader "Unlit/Tutor_01_FlatColor"
{
    // Properties
    // {
    //     _Color ("Main Color", Color) = (1,0,0,1)
    // }
    // SubShader
    // {
    //     Tags { "RenderType"="Opaque" "Queue" = "Geometry"}
    //     LOD 100

    //     Pass
    //     {
            
    //         HLSLPROGRAM

    //         #pragma vertex vert
    //         #pragma fragment frag

    //                     #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


    //         #include "UnityCG.cginc"

    //         CBUFFER_START(UnityPerMaterial)
    //             float4 _Color;
    //         CBUFFER_END

    //         struct Attributes
    //         {
    //             float4 positionOS : POSITION;
    //             };

    //         struct Varyings{
    //             float4 positionHCS : SV_POSITION;
    //             };

    //         Varyings vert(Attributes input){
    //             Varyings output;
    //             output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
    //             return output;
    //             }

    //         half4 frag(Varyings input) : SV_Target
    //         {
    //             return half4(_Color.rgb, _Color.a);
    //         }

    //         ENDHLSL

    //     }
    //}
}

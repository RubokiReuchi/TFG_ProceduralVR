Shader "Custom/Dither"
{
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "DitherPass"

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        // The Blit.hlsl file provides the vertex shader (Vert),
        // input structure (Attributes) and output strucutre (Varyings)
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        #pragma vertex Vert
        #pragma fragment frag

        TEXTURE2D_X(_CameraOpaqueTexture);
        SAMPLER(sampler_CameraOpaqueTexture);

        float _DitherSpread;
        int _ColorResolution;

        half4 frag(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
            float4 color = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.texcoord);
            float2 uv = input.positionCS.xy * input.positionCS.xy;
            float DITHER_THRESHOLDS[16] =
            {
                1.0 / 17.0,  9.0 / 17.0,  3.0 / 17.0, 11.0 / 17.0,
                13.0 / 17.0,  5.0 / 17.0, 15.0 / 17.0,  7.0 / 17.0,
                4.0 / 17.0, 12.0 / 17.0,  2.0 / 17.0, 10.0 / 17.0,
                16.0 / 17.0,  8.0 / 17.0, 14.0 / 17.0,  6.0 / 17.0
            };
            uint index = (uint(uv.x) % 4) * 4 + uint(uv.y) % 4;
            float4 dithered = color - DITHER_THRESHOLDS[index];
            dithered -= float4(0.5, 0.5, 0, 0);
            dithered *= _DitherSpread;
            float4 mixed = dithered + color;
            mixed *= _ColorResolution;
            mixed += float4(0.5, 0.5, 0, 0);
            mixed = floor(mixed);
            mixed /= _ColorResolution;
            color = mixed;
            return color;
        }
        ENDHLSL
    }
    }
}
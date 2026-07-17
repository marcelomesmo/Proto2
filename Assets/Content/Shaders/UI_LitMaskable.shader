Shader "UI/URP 2D/Lit Maskable"
{
    Properties
    {
        [PerRendererData]
        _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1, 1, 1, 1)

        /*
         * Used by the 2D Renderer blend-style mask system.
         * Leave this white when you do not require a mask texture.
         */
        _MaskTex ("Light Mask", 2D) = "white" {}

        // Standard uGUI Mask support.
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"

            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Universal2D"

            Tags
            {
                "LightMode" = "Universal2D"
            }

            HLSLPROGRAM

            #pragma target 3.0

            #pragma vertex Vert
            #pragma fragment Frag

            // Required by RectMask2D and regular UI alpha clipping.
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            /*
             * Generates the USE_SHAPE_LIGHT_TYPE_0–3 variants used by
             * the 2D Renderer blend styles.
             */
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                half4 color       : COLOR;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;

                // Screen-space UV used to sample the Light 2D textures.
                half2 lightingUV  : TEXCOORD1;

                // Original Canvas-space position used by RectMask2D.
                float2 uiPosition : TEXCOORD2;

                half4 color       : COLOR;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4 _Color;
            CBUFFER_END

            /*
             * Set by CanvasRenderer. Among other things, this supports
             * Alpha8 UI textures correctly.
             */
            half4 _TextureSampleAdd;

            // Set by RectMask2D.
            float4 _ClipRect;

            float GetUIClipping(
                float2 position,
                float4 clipRect
            )
            {
                float2 inside =
                    step(clipRect.xy, position) *
                    step(position, clipRect.zw);

                return inside.x * inside.y;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS =
                    TransformObjectToHClip(input.positionOS);

                output.uv =
                    input.uv * _MainTex_ST.xy +
                    _MainTex_ST.zw;

                output.color =
                    input.color * _Color;

                /*
                 * This matches the screen-space lighting UV calculation
                 * used by URP's Sprite-Lit-Default shader.
                 */
                output.lightingUV =
                    half2(
                        ComputeScreenPos(
                            output.positionCS /
                            output.positionCS.w
                        ).xy
                    );

                output.uiPosition = input.positionOS.xy;

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half4 textureColor =
                    SAMPLE_TEXTURE2D(
                        _MainTex,
                        sampler_MainTex,
                        input.uv
                    );

                half4 mainColor =
                    (textureColor + _TextureSampleAdd) *
                    input.color;

                half4 mask =
                    SAMPLE_TEXTURE2D(
                        _MaskTex,
                        sampler_MaskTex,
                        input.uv
                    );

                SurfaceData2D surfaceData;
                InputData2D inputData;

                /*
                 * No normal map is required for ordinary Light 2D
                 * illumination. This initializes a flat normal.
                 */
                InitializeSurfaceData(
                    mainColor.rgb,
                    mainColor.a,
                    mask,
                    surfaceData
                );

                InitializeInputData(
                    input.uv,
                    input.lightingUV,
                    inputData
                );

                half4 finalColor =
                    CombinedShapeLightShared(
                        surfaceData,
                        inputData
                    );

                // RectMask2D clipping.
                #ifdef UNITY_UI_CLIP_RECT

                    finalColor.a *= GetUIClipping(
                        input.uiPosition,
                        _ClipRect
                    );

                #endif

                #ifdef UNITY_UI_ALPHACLIP

                    clip(finalColor.a - 0.001h);

                #endif

                return finalColor;
            }

            ENDHLSL
        }
    }
}
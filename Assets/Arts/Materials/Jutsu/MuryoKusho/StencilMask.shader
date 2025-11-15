Shader "Custom/StencilMask"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0,255)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-1" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            ZWrite Off
            ColorMask 0
            
            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
            }
            
            HLSLPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                
                struct appdata{
                    float4 vertex : POSITION;
                };
                
                struct v2f {
                    float4 pos : SV_POSITION;
                };
                
                v2f vert(appdata v)
                {
                    v2f o;
                    o.pos = TransformObjectToHClip(v.vertex.xyz);
                    return o;
                }
                
                half4 frag (v2f i) : SV_TARGET
                {
                    return 0;
                }
            ENDHLSL    
        }
    }
}

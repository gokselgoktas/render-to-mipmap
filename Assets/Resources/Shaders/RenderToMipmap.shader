Shader "Hidden/Render to Mipmap"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct Input
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct Varyings
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
    };

    sampler2D _MainTex;

    float4 _MainTex_TexelSize;
    float _SourceLevel;

    Varyings vertex(in Input input)
    {
        Varyings output;

        output.vertex = UnityObjectToClipPos(input.vertex);
        output.uv = input.uv.xy;

#if UNITY_UV_STARTS_AT_TOP
        if (_MainTex_TexelSize.y < 0)
            output.uv.y = 1. - input.uv.y;
#endif

        return output;
    }

    float4 blitToMip0(in Varyings input) : SV_Target
    {
        return float4(1., 0., 0., 1.);
    }

    float4 blitToMip1(in Varyings input) : SV_Target
    {
        return float4(0., 1., 0., 1.);
    }

    float4 blitToMip2(in Varyings input) : SV_Target
    {
        return float4(0., 0., 1., 1.);
    }

    float4 blitToMip3(in Varyings input) : SV_Target
    {
        return float4(1., 1., 1., 1.);
    }

    float4 blit(in Varyings input) : SV_Target
    {
        return tex2Dlod(_MainTex, float4(input.uv, 0., _SourceLevel));
    }
    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment blitToMip0
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment blitToMip1
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment blitToMip2
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment blitToMip3
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vertex
            #pragma fragment blit
            ENDCG
        }
    }
}

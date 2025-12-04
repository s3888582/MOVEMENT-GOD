Shader "Custom/UnlitTwoSidedCutout"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" }
        Cull Off
        Lighting Off
        ZWrite On
        Blend Off

        Pass
        {
            AlphaTest Greater [_Cutoff]
            SetTexture [_MainTex] { combine texture }
        }
    }
}

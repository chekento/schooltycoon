Shader "KoSch/CampusSurface"
{
    Properties
    {
        _Color ("Campus colour", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 150
        CGPROGRAM
        #pragma surface surf Lambert fullforwardshadows addshadow
        #pragma target 3.0
        #pragma multi_compile_instancing
        struct Input { float3 worldPos; };
        fixed4 _Color;
        void surf(Input IN, inout SurfaceOutput o)
        {
            o.Albedo = _Color.rgb;
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}

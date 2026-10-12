Shader "KoSch/CampusGlass"
{
 Properties { _Color("Tint",Color)=(.7,.9,.9,.48) _Glossiness("Smoothness",Range(0,1))=.8 }
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard alpha:fade
 #pragma target 3.0
 struct Input { float2 uv_MainTex; };fixed4 _Color;half _Glossiness;
 void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=_Color.rgb;o.Smoothness=_Glossiness;o.Alpha=_Color.a;}
 ENDCG
 } Fallback "Transparent/Diffuse"
}

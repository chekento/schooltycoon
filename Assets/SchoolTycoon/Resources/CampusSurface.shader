Shader "KoSch/CampusSurface"
{
 Properties { _Color ("Tint",Color)=(1,1,1,1) _Metallic("Metal",Range(0,1))=0 _Glossiness("Smoothness",Range(0,1))=.27 _Surface("Surface",Float)=0 }
 SubShader {
 Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 #pragma multi_compile_instancing
 struct Input { float3 worldPos; float3 worldNormal; };
 fixed4 _Color; half _Metallic,_Glossiness;float _Surface;float _RektorMotion;
 void vert(inout appdata_full v){if(_Surface>4.5&&_Surface<5.5){float3 wp=mul(unity_ObjectToWorld,v.vertex).xyz;v.vertex.x+=sin(_Time.y*1.2+wp.x*.7+wp.z)*.012*_RektorMotion*saturate(v.vertex.y+.5);}}
 void surf(Input IN,inout SurfaceOutputStandard o){float grain=0;if(_Surface>.5&&_Surface<1.5)grain=sin(IN.worldPos.x*48+sin(IN.worldPos.z*3)*2)*.018;else if(_Surface>3.5&&_Surface<4.5)grain=sin(IN.worldPos.x*190)*sin(IN.worldPos.z*190)*.008;
 o.Albedo=_Color.rgb*(.98+.02*saturate(IN.worldNormal.y)+grain);o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=1;if(_Surface>6.5)o.Emission=_Color.rgb*.12; }
 ENDCG
 }
 Fallback "Diffuse"
}

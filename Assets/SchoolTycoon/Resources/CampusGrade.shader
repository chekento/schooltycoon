Shader "Hidden/KoSch/CampusGrade"
{
 Properties { _MainTex("Image",2D)="white"{} }
 SubShader { Cull Off ZWrite Off ZTest Always
 Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize;
 fixed4 frag(v2f_img i):SV_Target { float3 c=tex2D(_MainTex,i.uv).rgb;float3 glow=0;
 for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)glow+=max(0,tex2D(_MainTex,i.uv+float2(x,y)*_MainTex_TexelSize.xy*3).rgb-.8);
 c+=glow*.025;float l=dot(c,float3(.2126,.7152,.0722));c=lerp(l.xxx,c,1.04)*float3(1.018,1.012,.99);float2 v=i.uv-.5;c*=1-dot(v,v)*.045;return float4(c,1); }
 ENDCG }
 }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using KoSch.SchoolTycoon.Core;
namespace KoSch.SchoolTycoon
{
    public sealed class CampusArt : IDisposable
    {
        private readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        private readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
        public Transform Draw(Transform parent,VisualPart part)
        {
            var go=new GameObject(part.Name,typeof(MeshFilter),typeof(MeshRenderer));Transform t=go.transform;t.SetParent(parent,false);
            t.localPosition=new Vector3(part.X,part.Y,part.Z);t.localRotation=Quaternion.Euler(0,part.Rotation,0);
            string key=part.Shape==VisualShape.Box ? "box:"+part.W+":"+part.H+":"+part.D : part.Shape.ToString();Mesh mesh;
            if(!meshes.TryGetValue(key,out mesh))
            {
                if(part.Shape==VisualShape.Box)mesh=RoundedBox(new Vector3(part.W,part.H,part.D));
                else{var prim=GameObject.CreatePrimitive(part.Shape==VisualShape.Sphere?PrimitiveType.Sphere:part.Shape==VisualShape.Cylinder?PrimitiveType.Cylinder:PrimitiveType.Capsule);mesh=prim.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.Destroy(prim);}
                meshes.Add(key,mesh);
            }
            t.localScale=part.Shape==VisualShape.Box?Vector3.one:new Vector3(part.W,part.H/(part.Shape==VisualShape.Cylinder||part.Shape==VisualShape.Capsule?2:1),part.D);
            go.GetComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.GetComponent<MeshRenderer>();renderer.sharedMaterial=Material(part);
            if(part.Surface==VisualSurface.Glass||part.Surface==VisualSurface.Water)renderer.shadowCastingMode=ShadowCastingMode.Off;
            return t;
        }
        public void DrawAll(Transform p,IEnumerable<VisualPart> parts){foreach(var part in parts)Draw(p,part);}
        private Material Material(VisualPart p)
        {
            string key=p.Color+":"+p.Surface;Material m;if(materials.TryGetValue(key,out m))return m;
            bool glass=p.Surface==VisualSurface.Glass;Shader shader=Resources.Load<Shader>(glass?"CampusGlass":"CampusSurface")??Shader.Find("Standard");
            m=new Material(shader){color=CampusRenderer.ColorHex(p.Color),enableInstancing=true};m.SetFloat("_Surface",(int)p.Surface);
            m.SetFloat("_Metallic",p.Surface==VisualSurface.Metal?.32f:0);m.SetFloat("_Glossiness",glass?.8f:p.Surface==VisualSurface.Water?.72f:.27f);
            if(glass){Color c=m.color;c.a=.48f;m.color=c;}materials.Add(key,m);return m;
        }
        private static Mesh RoundedBox(Vector3 size)
        {
            float radius=Mathf.Min(.055f,Mathf.Min(size.x,Mathf.Min(size.y,size.z))*.21f);Vector3 half=size*.5f,inner=half-Vector3.one*radius;
            var v=new List<Vector3>();var n=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            Vector3[] normals={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
            Vector3[] us={Vector3.back,Vector3.forward,Vector3.right,Vector3.right,Vector3.right,Vector3.left};
            Vector3[] vs={Vector3.up,Vector3.up,Vector3.back,Vector3.forward,Vector3.up,Vector3.up};
            for(int f=0;f<6;f++)
            {
                Vector3 face=normals[f],u=us[f],vv=vs[f];float a=Vector3.Dot(half,Abs(u)),b=Vector3.Dot(half,Abs(vv)),c=Vector3.Dot(half,Abs(face));
                float[] xs={-a,-a+radius,a-radius,a},ys={-b,-b+radius,b-radius,b};int start=v.Count;
                for(int y=0;y<4;y++)for(int x=0;x<4;x++)
                {Vector3 p=face*c+u*xs[x]+vv*ys[y];Vector3 center=new Vector3(Mathf.Clamp(p.x,-inner.x,inner.x),Mathf.Clamp(p.y,-inner.y,inner.y),Mathf.Clamp(p.z,-inner.z,inner.z));Vector3 norm=(p-center).normalized;v.Add(center+norm*radius);n.Add(norm);uv.Add(new Vector2(x/3f,y/3f));}
                for(int y=0;y<3;y++)for(int x=0;x<3;x++){int i=start+y*4+x;tris.Add(i);tris.Add(i+1);tris.Add(i+4);tris.Add(i+1);tris.Add(i+5);tris.Add(i+4);}
            }
            var mesh=new Mesh{name="Soft bevel box"};mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateBounds();return mesh;
        }
        private static Vector3 Abs(Vector3 v){return new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));}
        public void Dispose(){foreach(Material m in materials.Values)UnityEngine.Object.Destroy(m);foreach(var kv in meshes)if(kv.Key.StartsWith("box:"))UnityEngine.Object.Destroy(kv.Value);}
    }
    public sealed class CampusPost : MonoBehaviour
    {
        private Material grade;
        private void OnRenderImage(RenderTexture src,RenderTexture dst)
        { if(grade==null){Shader s=Resources.Load<Shader>("CampusGrade");if(s!=null&&s.isSupported)grade=new Material(s);}if(grade!=null)Graphics.Blit(src,dst,grade);else Graphics.Blit(src,dst); }
        private void OnDestroy(){if(grade!=null)Destroy(grade);}
    }
}

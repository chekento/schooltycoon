using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using KoSch.SchoolTycoon.Core;
namespace KoSch.SchoolTycoon
{
    public sealed class CampusRenderer : MonoBehaviour
    {
        private SchoolApp app;private CampusArt art;private Transform buildings,people,ghost;private Light sun;
        private readonly Dictionary<int,Transform> rooms=new Dictionary<int,Transform>();private readonly List<CampusPerson> agents=new List<CampusPerson>();
        private Cell ghostCell=new Cell(-1,-1);private RoomKind? ghostKind;private bool ghostRotated;
        public void Initialize(SchoolApp owner)
        {
            app=owner;art=new CampusArt();RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=ColorHex("F2ECDD");RenderSettings.ambientEquatorColor=ColorHex("C6D8CD");RenderSettings.ambientGroundColor=ColorHex("97ADA0");RenderSettings.fog=false;
            sun=new GameObject("Soft afternoon sun").AddComponent<Light>();sun.type=LightType.Directional;sun.color=ColorHex("FFF1D4");sun.intensity=1.2f;sun.transform.rotation=Quaternion.Euler(48,-28,0);sun.shadows=LightShadows.Soft;sun.shadowBias=.03f;sun.shadowNormalBias=.2f;
            QualitySettings.shadowDistance=85;QualitySettings.shadows=ShadowQuality.All;QualitySettings.antiAliasing=4;
            Transform landscape=Group("Garden neighbourhood",transform);art.DrawAll(landscape,CampusDesign.Landscape());StaticBatchingUtility.Combine(landscape.gameObject);Rebuild();
        }
        public static Color ColorHex(string hex){Color c;ColorUtility.TryParseHtmlString("#"+hex,out c);return c;}
        public static string HairColor(string n){return n=="Black"?"403B37":n=="Blonde"?"DFC489":n=="Red"?"B78966":n=="Silver"?"C8D4D3":"796047";}
        private static Transform Group(string name,Transform parent){var go=new GameObject(name);go.transform.SetParent(parent,false);return go.transform;}
        private void Update(){if(sun!=null){float day=Mathf.InverseLerp(480,1020,app.State.ClockMinute);sun.color=Color.Lerp(ColorHex("FFF5DB"),ColorHex("FFD9B2"),day);sun.intensity=app.Simulation.Weather==WeatherKind.Rainy?.84f:1.2f;}}
        public void Rebuild()
        {
            ClearGhost();if(buildings!=null){buildings.gameObject.SetActive(false);Destroy(buildings.gameObject);}if(people!=null){people.gameObject.SetActive(false);Destroy(people.gameObject);}
            buildings=Group("School rooms",transform);people=Group("School community",transform);rooms.Clear();agents.Clear();
            foreach(Room room in app.State.Rooms)RoomVisual(room);
            foreach(Pupil p in app.State.Pupils.Where(p=>!p.Absent).Take(app.Settings==null?64:app.Settings.Graphics==0?24:app.Settings.Graphics==1?40:64))Person(p.Id,false,null,p);
            foreach(Employee e in app.State.Staff.Take(24))Person(e.Id+100,true,e,null);
            if(app.State.HasDirector)Person(201,true,new Employee{Name=app.State.Director.Name,Role=StaffRole.Counselor},null,true);
            SetViewFloor();
        }
        private void RoomVisual(Room r)
        {
            Transform root=Group(app.RoomName(r.Kind)+" "+r.Id,buildings);root.localPosition=new Vector3(r.X,r.Floor*CampusGrid.FloorHeight,r.Y);rooms[r.Id]=root;
            Cell door,inside;int side=2;float at=.5f;
            if(CampusGrid.TryDoor(r,CampusGrid.Reachable(app.State),out door,out inside))
            {if(door.Y>=r.Y+r.Height){side=0;at=inside.X-r.X+.5f;}else if(door.X<r.X){side=1;at=inside.Y-r.Y+.5f;}else if(door.X>=r.X+r.Width){side=3;at=inside.Y-r.Y+.5f;}else{side=2;at=inside.X-r.X+.5f;}}
            art.DrawAll(root,CampusDesign.Room(r,side,at,app.State.Rooms.Any(upper=>upper.Kind==RoomKind.Staircase&&upper.Floor==r.Floor+1&&upper.X==r.X&&upper.Y==r.Y)));StaticBatchingUtility.Combine(root.gameObject);
            if(!CampusGrid.Walkway(r)){SchoolClass c=app.State.Classes.FirstOrDefault(c=>c.RoomId==r.Id);Label(root,c==null?app.RoomName(r.Kind):c.Name+" · "+app.Simulation.ClassSize(c)+"/"+app.Simulation.RoomSeats(r),new Vector3(r.Width/2f,2.95f,r.Height/2f));}
        }
        public void SetViewFloor()
        {
            foreach(Room r in app.State.Rooms){Transform root;if(!rooms.TryGetValue(r.Id,out root))continue;root.gameObject.SetActive(r.Floor<=app.ViewFloor);foreach(CampusBillboard label in root.GetComponentsInChildren<CampusBillboard>(true))label.gameObject.SetActive(app.WorldLabels&&r.Floor==app.ViewFloor);}
            foreach(CampusPerson p in agents)p.UpdateVisibility();
        }
        private void Label(Transform parent,string text,Vector3 position)
        {Transform t=Group("Room label",parent);t.localPosition=position;var mesh=t.gameObject.AddComponent<TextMesh>();mesh.text=text;mesh.characterSize=.1f;mesh.fontSize=48;mesh.anchor=TextAnchor.MiddleCenter;mesh.color=ColorHex("39595B");t.gameObject.AddComponent<CampusBillboard>();}
        private void Person(int id,bool adult,Employee employee,Pupil pupil,bool director=false)
        {
            Transform root=Group(adult?employee.Name:pupil.Name,people);root.localPosition=new Vector3(14.5f,.15f,8.5f);
            Transform body=Group("Animated body",root);body.localScale=Vector3.one*(adult?.93f:.76f);var bones=new Transform[4];
            Vector3[] pivots={new Vector3(-.215f,.8f,0),new Vector3(.215f,.8f,0),new Vector3(-.084f,.42f,0),new Vector3(.084f,.42f,0)};
            for(int i=0;i<4;i++){bones[i]=Group("Limb "+i,body);bones[i].localPosition=pivots[i];}
            foreach(var part in CampusDesign.Person(id,adult,director?HairColor(app.State.Director.Hair):id%3==0?"51493E":id%3==1?"C2A47D":"8F765D",director?app.State.Director.Gender:"Diverse"))
            {Transform t=art.Draw(part.Bone==0?body:bones[part.Bone-1],part);if(part.Bone!=0)t.localPosition-=pivots[part.Bone-1];}
            if(pupil!=null&&app.Simulation.NeedsSupport(pupil)){var part=new VisualPart{Name="Support indicator",Shape=VisualShape.Sphere,Surface=VisualSurface.Matte,Color="E4C888",Y=1.43f,W=.12f,H=.12f,D=.12f};art.Draw(body,part);}
            var agent=root.gameObject.AddComponent<CampusPerson>();agent.Initialize(app,id,adult,employee,pupil,body,bones);agents.Add(agent);
        }
        public void ClearGhost(){if(ghost!=null){ghost.gameObject.SetActive(false);Destroy(ghost.gameObject);ghost=null;}ghostCell=new Cell(-1,-1);ghostKind=null;}
        public void ShowGhost(Cell c)
        {
            if(!app.BuildKind.HasValue){ClearGhost();return;}if(c.Equals(ghostCell)&&ghostKind==app.BuildKind&&ghostRotated==app.Rotated)return;
            ClearGhost();ghostCell=c;ghostKind=app.BuildKind;ghostRotated=app.Rotated;RoomSpec s=Catalog.Get(ghostKind.Value);int w=app.Rotated?s.Height:s.Width,h=app.Rotated?s.Width:s.Height;
            bool valid=app.Simulation.CanBuild(s.Kind,c.X,c.Y,app.Rotated,c.Floor).Success;ghost=Group("Placement preview",transform);
            for(int x=0;x<w;x++)for(int y=0;y<h;y++)art.Draw(ghost,new VisualPart{Name="Footprint",Shape=VisualShape.Box,Surface=VisualSurface.Matte,Color=valid?"97C9AA":"DCACA0",X=c.X+x+.5f,Y=c.Floor*3+.2f,Z=c.Y+y+.5f,W=.9f,H=.06f,D=.9f});
            Label(ghost,app.RoomName(s.Kind)+" · "+app.Simulation.BuildCost(s.Kind,c.Floor)+" €",new Vector3(c.X+w/2f,c.Floor*3+1.4f,c.Y+h/2f));
        }
        private void OnDestroy(){if(art!=null)art.Dispose();}
    }
    public sealed class CampusBillboard : MonoBehaviour {private void LateUpdate(){if(Camera.main!=null)transform.rotation=Camera.main.transform.rotation;}}
    public sealed class CampusPerson : MonoBehaviour
    {
        private SchoolApp app;private int id,lastPhase=-1,waypoint;private bool adult,indoors;private Employee employee;private Pupil pupil;
        private Transform body;private Transform[] bones;private readonly List<Vector3> points=new List<Vector3>();private Cell corridor=CampusGrid.Entrance;private Vector3 exit;private float gait,wait;
        public void Initialize(SchoolApp owner,int number,bool isAdult,Employee person,Pupil student,Transform b,Transform[] limbs)
        {app=owner;id=number;adult=isAdult;employee=person;pupil=student;body=b;bones=limbs;wait=id%11*.23f;}
        public void UpdateVisibility(){bool visible=transform.localPosition.y<=(app.ViewFloor+1)*3-.05f;foreach(Renderer r in GetComponentsInChildren<Renderer>())r.enabled=visible;}
        private Vector3 Point(Cell c)
        {Room r=CampusGrid.At(app.State,c);return new Vector3(r!=null&&r.Kind==RoomKind.Staircase?r.X+.15f:c.X+.5f+(id%3-1)*.09f,c.Floor*3+.15f,c.Y+.5f);}
        private void StairLink(Cell from,Cell to)
        {
            int floor=Mathf.Min(from.Floor,to.Floor);Room stair=CampusGrid.At(app.State,new Cell(from.X,from.Y,floor));bool reverse=floor%2==1;
            Vector3 bottom=new Vector3(stair.X+stair.Width/2f,floor*3+.15f,reverse?stair.Y+.25f:stair.Y+stair.Height-.25f);
            Vector3 top=new Vector3(stair.X+stair.Width/2f,(floor+1)*3+.15f,reverse?stair.Y+stair.Height-.25f:stair.Y+.25f);
            points.Add(from.Floor<to.Floor?bottom:top);points.Add(from.Floor<to.Floor?top:bottom);points.Add(Point(to));
        }
        private void Update()
        {
            if(app.InMainMenu||app.Paused||app.UI==null||app.UI.HasModal||!app.State.HasDirector||!app.Simulation.CanOperate)return;float dt=Time.deltaTime*app.Speed;
            if(waypoint<points.Count)
            {
                Vector3 target=points[waypoint],delta=target-transform.localPosition;float step=dt*(adult?1.2f:1.45f);
                if(delta.magnitude<step){transform.localPosition=target;waypoint++;}else{transform.localPosition+=delta.normalized*step;Vector3 dir=delta;dir.y=0;if(dir.sqrMagnitude>.001f)transform.localRotation=Quaternion.Slerp(transform.localRotation,Quaternion.LookRotation(-dir),dt*9);}
                gait+=dt*11;body.localPosition=new Vector3(0,app.Settings.ReducedMotion?0:Mathf.Abs(Mathf.Sin(gait))*.026f,0);
                for(int i=0;i<4;i++)bones[i].localRotation=Quaternion.Euler((app.Settings.ReducedMotion?0:Mathf.Sin(gait+(i%2)*Mathf.PI)*(i<2?18:24)),0,0);UpdateVisibility();return;
            }
            body.localPosition=new Vector3(0,app.Settings.ReducedMotion?0:Mathf.Sin(Time.time*2+id)*.005f,0);foreach(Transform bone in bones)bone.localRotation=Quaternion.identity;
            wait-=dt;if(lastPhase==-1&&wait>0)return;int phase=(app.State.ClockMinute-480)/60;if(phase==lastPhase)return;lastPhase=phase;
            Room chosen=app.Simulation.Destination(pupil,employee,app.State.ClockMinute,id);if(chosen==null)return;Cell door,inside;
            if(!CampusGrid.TryDoor(chosen,CampusGrid.Reachable(app.State),out door,out inside))return;var path=CampusGrid.Path(app.State,corridor,door);if(path.Count==0)return;
            points.Clear();waypoint=0;if(indoors)points.Add(exit);
            for(int i=0;i<path.Count;i++){if(i>0&&path[i].Floor!=path[i-1].Floor)StairLink(path[i-1],path[i]);else points.Add(Point(path[i]));}
            exit=new Vector3(inside.X+.5f,inside.Floor*3+.15f,inside.Y+.5f);points.Add(exit);
            points.Add(new Vector3(chosen.X+.55f+(id%4)*(chosen.Width-1.1f)/4,chosen.Floor*3+.15f,chosen.Y+.55f+((id/4)%3)*(chosen.Height-1.1f)/3));corridor=door;indoors=true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
namespace KoSch.SchoolTycoon.Core
{
    public enum VisualShape { Box, Sphere, Cylinder, Capsule }
    public enum VisualSurface { Matte, Timber, Metal, Glass, Fabric, Leaf, Water, Screen }
    public sealed class VisualPart
    {
        public string Name,Color;public VisualShape Shape;public VisualSurface Surface;
        public float X,Y,Z,W,H,D,Rotation;public int Bone;
    }
    /// <summary>Original shared art recipes. Unity meshes and offline art previews use these same parts.</summary>
    public static class CampusDesign
    {
        private static readonly string[] Accents={"E9A18D","7DADCA","C1A2CF","DAB76B","86B4A1","919FCE"};
        private static void Add(List<VisualPart> p,string name,float x,float y,float z,float w,float h,float d,string col,VisualShape shape=VisualShape.Box,VisualSurface surface=VisualSurface.Matte,float rotation=0,int bone=0)
        { p.Add(new VisualPart{Name=name,X=x,Y=y,Z=z,W=w,H=h,D=d,Color=col,Shape=shape,Surface=surface,Rotation=rotation,Bone=bone}); }
        private static void B(List<VisualPart> p,string n,float x,float y,float z,float w,float h,float d,string c,VisualSurface s=VisualSurface.Matte,float rot=0,int bone=0){Add(p,n,x,y,z,w,h,d,c,VisualShape.Box,s,rot,bone);}
        private static void S(List<VisualPart> p,string n,float x,float y,float z,float w,float h,float d,string c,VisualSurface s=VisualSurface.Matte,int bone=0){Add(p,n,x,y,z,w,h,d,c,VisualShape.Sphere,s,0,bone);}
        private static void C(List<VisualPart> p,string n,float x,float y,float z,float w,float h,float d,string c,VisualSurface s=VisualSurface.Matte){Add(p,n,x,y,z,w,h,d,c,VisualShape.Cylinder,s);}
        private static void Plant(List<VisualPart> p,float x,float z,float scale=1)
        {
            C(p,"Ceramic planter",x,.28f*scale,z,.34f*scale,.33f*scale,.34f*scale,"EBC5B1");
            for(int i=0;i<5;i++){float a=i*1.256f;S(p,"Glossy leaves",x+(float)Math.Cos(a)*.1f*scale,.5f*scale+i*.04f*scale,z+(float)Math.Sin(a)*.1f*scale,.21f*scale,.34f*scale,.2f*scale,i%2==0?"739A74":"91B889",VisualSurface.Leaf);}
        }
        private static void Books(List<VisualPart> p,float x,float y,float z,int index)
        { B(p,"Book pages",x,y,z,.21f,.035f,.27f,"FFF4DE");B(p,"Book cover",x,y+.022f,z,.23f,.012f,.29f,Accents[index%6]);B(p,"Book spine",x-.11f,y,z,.015f,.05f,.29f,Accents[index%6]); }
        private static void Chair(List<VisualPart> p,float x,float z,string color="88AFC0")
        {
            B(p,"Moulded chair seat",x,.4f,z,.29f,.06f,.28f,color);B(p,"Chair back",x,.6f,z-.12f,.3f,.32f,.06f,color);
            for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2)B(p,"Chair leg",x+a*.105f,.25f,z+b*.1f,.025f,.29f,.025f,"75858C",VisualSurface.Metal);
        }
        private static void Desk(List<VisualPart> p,float x,float z,float width=.7f)
        {
            B(p,"Oak desk top",x,.65f,z,width,.075f,.43f,"D9B892",VisualSurface.Timber);
            for(int a=-1;a<=1;a+=2)for(int b=-1;b<=1;b+=2)B(p,"Desk leg",x+a*(width/2-.08f),.4f,z+b*.14f,.04f,.45f,.04f,"869B9F",VisualSurface.Metal);
        }
        private static void Monitor(List<VisualPart> p,float x,float z)
        {
            B(p,"Monitor base",x,.7f,z,.22f,.025f,.14f,"697C8E",VisualSurface.Metal);B(p,"Monitor stem",x,.83f,z,.035f,.25f,.04f,"697C8E",VisualSurface.Metal);
            B(p,"Monitor bezel",x,.94f,z,.36f,.25f,.045f,"526B81");B(p,"Learning screen",x,.94f,z-.025f,.32f,.21f,.009f,"B6DAD1",VisualSurface.Screen);
            for(int i=0;i<3;i++)B(p,"Screen diagram",x-.09f+i*.075f,.94f+i*.012f,z-.032f,.04f,.06f+i*.026f,.006f,Accents[i]);
            B(p,"Keyboard",x,.7f,z-.16f,.3f,.02f,.09f,"EBE6DC");
        }
        private static void Shelf(List<VisualPart> p,float x,float z)
        {
            for(int i=-1;i<=1;i+=2)B(p,"Oak bookcase side",x+i*.47f,.84f,z,.055f,1.35f,.32f,"C6A486",VisualSurface.Timber);
            for(int i=0;i<4;i++)B(p,"Bookcase shelf",x,.2f+i*.38f,z,.98f,.055f,.34f,"C6A486",VisualSurface.Timber);
            for(int row=0;row<3;row++)for(int i=0;i<6;i++){float bx=x-.37f+i*.15f,by=.38f+row*.38f;B(p,"Coloured book",bx,by,z-.035f,.095f,.22f+(i%3)*.025f,.19f,Accents[(i+row)%6]);B(p,"Book label",bx,by,z-.137f,.05f,.018f,.01f,"F8F1DA");}
        }
        private static void Board(List<VisualPart> p,float w,float h)
        {
            B(p,"Board oak frame",w/2,1.65f,h-.18f,2.1f,1,.09f,"CBA77D",VisualSurface.Timber);B(p,"Writing board",w/2,1.65f,h-.24f,1.96f,.87f,.035f,"3D6B63");
            for(int i=0;i<5;i++)B(p,"Chalk notes",w/2-.72f+i*.28f,1.68f+(i%2)*.16f,h-.267f,.15f,.023f,.008f,"F8F4DE");
            C(p,"Clock face",w-.45f,2.13f,h-.2f,.28f,.05f,.28f,"F8F2DA");
        }
        public static List<VisualPart> Room(Room r,int doorSide=2,float doorAt=.5f)
        {
            var p=new List<VisualPart>();float w=r.Width,h=r.Height;string accent=Catalog.Get(r.Kind).Hex;
            B(p,"Structural floor edge",w/2,.035f,h/2,w-.02f,.14f,h-.02f,"96ABAC");
            B(p,"Soft floor",w/2,.13f,h/2,w-.08f,.05f,h-.08f,r.Kind==RoomKind.Garden?"98BA87":"EEE8DA");
            if(r.Kind==RoomKind.Corridor)
            { for(int x=0;x<w;x++)for(int z=0;z<h;z++)B(p,"Terrazzo inlay",x+.5f,.165f,z+.5f,.7f,.015f,.7f,"D7E3DC");return p; }
            if(r.Kind==RoomKind.Staircase)
            {
                bool reverse=r.Floor%2==1;
                for(int i=0;i<12;i++){float z=reverse?.25f+i*.135f:h-.25f-i*.135f,y=.14f+(i+1)*CampusGrid.FloorHeight/12;
                    B(p,"Oak stair tread",w/2,y,z,1.25f,.08f,.18f,"D3B18E",VisualSurface.Timber);
                    for(int side=-1;side<=1;side+=2){B(p,"Stair baluster",w/2+side*.69f,y+.4f,z,.035f,.78f,.035f,"7F9796",VisualSurface.Metal);S(p,"Handrail joint",w/2+side*.69f,y+.8f,z,.075f,.075f,.075f,"B2CBC1");}}
                B(p,"Upper stair landing",w/2,3.14f,reverse?h-.13f:.13f,1.5f,.09f,.25f,"C5D3CC");return p;
            }
            if(r.Kind==RoomKind.Garden)
            {
                B(p,"Garden sandstone path",w/2,.17f,h/2,.55f,.04f,h-.15f,"D9CFB8");Tree(p,.55f,h-.6f,.7f,0);
                if(Has(r,RoomItem.RaisedBeds))for(int i=0;i<3;i++){B(p,"Raised vegetable bed",.6f,.28f,.45f+i*.52f,.7f,.24f,.4f,"B89171",VisualSurface.Timber);B(p,"Garden soil",.6f,.41f,.45f+i*.52f,.61f,.035f,.32f,"8C7763");for(int j=0;j<3;j++)S(p,"Growing vegetables",.38f+j*.2f,.48f,.45f+i*.52f,.2f,.18f,.24f,"91AF6D",VisualSurface.Leaf);}
                Bench(p,w-.68f,.6f);if(Has(r,RoomItem.Plants))Plant(p,w-.45f,h-.5f);if(r.FurnishingVersion==1)ItemVisuals(p,r);return p;
            }
            // Back walls have real door openings; near walls are low to keep the cutaway readable.
            for(int x=0;x<w;x++)
            {
                if(!(doorSide==0 && Math.Abs(x+.5f-doorAt)<.7f))
                { B(p,"Ivory wall bay",x+.5f,1.44f,h-.045f,.98f,2.56f,.12f,"F5EFDF");if(x%2==0){B(p,"Window recess",x+.5f,1.68f,h-.115f,.69f,.98f,.065f,"BDCCCA");B(p,"Window glass",x+.5f,1.68f,h-.155f,.57f,.84f,.025f,"B4DAD9",VisualSurface.Glass);B(p,"Window mullion",x+.5f,1.68f,h-.17f,.035f,.88f,.02f,"EAEADB");}}
                B(p,"Facade pier",x+.02f,1.5f,h-.01f,.1f,2.72f,.17f,"D3DFD4");
            }
            for(int z=0;z<h;z++)if(!(doorSide==1&&Math.Abs(z+.5f-doorAt)<.7f))B(p,"Side wall",.04f,1.44f,z+.5f,.12f,2.56f,.98f,"F4ECDB");
            B(p,"Back cornice",w/2,2.8f,h-.015f,w,.14f,.18f,"D2DDCC");B(p,"Side cornice",.04f,2.8f,h/2,.18f,.14f,h,"D2DDCC");
            for(int x=0;x<w;x++)if(!(doorSide==2&&Math.Abs(x+.5f-doorAt)<.7f))B(p,"Cutaway front wall",x+.5f,.32f,.04f,.98f,.33f,.12f,"E2E9D9");
            for(int z=0;z<h;z++)if(!(doorSide==3&&Math.Abs(z+.5f-doorAt)<.7f))B(p,"Cutaway side wall",w-.04f,.32f,z+.5f,.12f,.33f,.98f,"E2E9D9");
            B(p,"Accent skirting",w/2,.22f,h-.12f,w-.12f,.1f,.05f,accent);
            for(int i=0;i<(int)(w*3);i++)B(p,"Oak floor strip",.18f+i*.32f,.161f,h/2,.29f,.008f,h-.23f,i%3==0?"E0CEB6":"E7D8C2",VisualSurface.Timber);
            Furnish(p,r);return p;
        }
        private static bool Has(Room r,RoomItem item){return r.FurnishingVersion==0||r.Items.Contains(item);}
        private static void Furnish(List<VisualPart> p,Room r)
        {
            float w=r.Width,h=r.Height;
            switch(r.Kind)
            {
                case RoomKind.Classroom:case RoomKind.ComputerLab:case RoomKind.LanguageLab:
                    int desks=r.FurnishingVersion==1?9+2*r.Items.Count(i=>i==RoomItem.DeskRow1||i==RoomItem.DeskRow2||i==RoomItem.DeskRow3):12+r.UpgradeLevel*2;int cols=desks==9?3:4;
                    for(int n=0;n<desks;n++)
                    {int col=n%cols,row=n/cols;float x=.6f+col*(w-.9f)/cols,z=.55f+row*(h-1.1f)/4;Desk(p,x,z,.62f);Chair(p,x-.14f,z-.29f);Chair(p,x+.14f,z-.29f);if(r.Kind==RoomKind.ComputerLab&&Has(r,RoomItem.Workstations))Monitor(p,x,z);else{if(Has(r,RoomItem.TeachingMaterials))Books(p,x-.09f,.715f,z,col);B(p,"Pencil",x+.14f,.71f,z,.025f,.014f,.19f,"DBAF60");if(r.Kind==RoomKind.LanguageLab&&Has(r,RoomItem.Headsets))C(p,"Headphones",x+.11f,.77f,z,.13f,.08f,.13f,"708BBA");}}
                    Board(p,w,h);break;
                case RoomKind.Library:
                    for(int i=0;i<(Has(r,RoomItem.ReferenceBooks)?3:1);i++)Shelf(p,.7f+i*1.18f,h-.4f);for(int i=0;i<2;i++){Desk(p,1+i*1.65f,.9f,1.1f);Chair(p,.8f+i*1.65f,.6f);Chair(p,1.2f+i*1.65f,.6f);Books(p,1+i*1.65f,.72f,.9f,i);}
                    B(p,"Reading rug",w/2,.18f,h/2,1.8f,.024f,1,"AECAC1",VisualSurface.Fabric);break;
                case RoomKind.ScienceLab:
                    for(int i=0;i<4;i++){float x=.9f+(i%2)*1.6f,z=.85f+(i/2)*1.15f;Desk(p,x,z,1.25f);Chair(p,x-.3f,z-.32f);Chair(p,x+.3f,z-.32f);if(Has(r,RoomItem.ChemistrySet)){C(p,"Beaker liquid",x-.26f,.78f,z,.14f,.16f,.14f,"88C6BA",VisualSurface.Glass);C(p,"Glass beaker rim",x-.26f,.87f,z,.16f,.025f,.16f,"C0E2D9",VisualSurface.Glass);}if(Has(r,RoomItem.Microscopes)){B(p,"Microscope base",x+.23f,.73f,z,.18f,.04f,.2f,"A4BCC1");B(p,"Microscope stand",x+.23f,.85f,z,.045f,.25f,.05f,"6B828D",VisualSurface.Metal);B(p,"Microscope lens",x+.2f,.98f,z-.04f,.1f,.07f,.17f,"526E7D");}}
                    Board(p,w,h);break;
                case RoomKind.ArtRoom:
                    if(Has(r,RoomItem.Easels))for(int i=0;i<3;i++){float x=.8f+i*1.12f;B(p,"Easel legs",x,.71f,1.45f,.08f,1.1f,.08f,"CAA17F",VisualSurface.Timber);B(p,"Artist canvas",x,1.14f,1.4f,.68f,.78f,.065f,"FFF1D6");S(p,"Painted colour field",x,1.17f,1.36f,.41f,.33f,.018f,Accents[i]);B(p,"Painting horizon",x,1.02f,1.345f,.55f,.045f,.01f,"A2C7B9");C(p,"Paint pot",x,.35f,1.1f,.13f,.13f,.13f,Accents[i+1]);}break;
                case RoomKind.MusicRoom:
                    B(p,"Music stage",w/2,.23f,h-.7f,w-.5f,.14f,1.05f,"C5A384",VisualSurface.Timber);if(Has(r,RoomItem.AcousticPiano)){B(p,"Piano body",1.05f,.9f,h-.65f,1.5f,1.15f,.55f,"5E7385");
                    for(int i=0;i<15;i++){B(p,"Ivory piano key",.42f+i*.085f,.76f,h-1.03f,.077f,.045f,.27f,"FFF8DF");if(i%7!=2&&i%7!=6)B(p,"Ebony piano key",.46f+i*.085f,.8f,h-.96f,.035f,.06f,.15f,"425164");}
                    }if(Has(r,RoomItem.Instruments)){C(p,"Bass drum",w-1,.52f,1,.56f,.52f,.56f,"DEAA99");C(p,"Cymbal",w-1.25f,1.2f,1,.65f,.03f,.65f,"DDC278",VisualSurface.Metal);}for(int i=0;i<3;i++){Chair(p,.7f+i,.7f,"C6ADCD");B(p,"Music stand",.7f+i,.72f,1.1f,.025f,1.1f,.025f,"71868F",VisualSurface.Metal);B(p,"Sheet music",.7f+i,1.19f,1.1f,.31f,.2f,.025f,"FAF3DB");}break;
                case RoomKind.Theatre:
                    B(p,"Performance stage",w/2,.33f,h-.75f,w-.35f,.34f,1.3f,"C8A080",VisualSurface.Timber);for(int i=0;i<2;i++)B(p,"Velvet stage curtain",.3f+i*(w-.6f),1.63f,h-.38f,.43f,2.5f,.22f,"C88F9A",VisualSurface.Fabric);
                    for(int i=0;i<8;i++)Chair(p,.75f+(i%4)*.8f,.6f+(i/4)*.6f,"B894B7");break;
                case RoomKind.Workshop:
                    for(int i=0;i<2;i++){Desk(p,1.1f+i*1.75f,1.2f,1.4f);if(Has(r,RoomItem.BenchTools))B(p,"Bench vice",1.15f+i*1.75f,.8f,1.1f,.19f,.22f,.22f,"7A98A3",VisualSurface.Metal);B(p,"Wood project",.8f+i*1.75f,.73f,1.3f,.33f,.12f,.3f,"D9B080",VisualSurface.Timber);}
                    B(p,"Pegboard",w/2,1.6f,h-.2f,2.8f,1.1f,.09f,"D7C4A5");if(Has(r,RoomItem.BenchTools))for(int i=0;i<9;i++)B(p,"Workshop tool",w/2-1.1f+i*.27f,1.58f+(i%2)*.13f,h-.27f,.04f,.28f,.055f,i%2==0?"6E8992":"CB966C",VisualSurface.Metal);break;
                case RoomKind.FoodLab:case RoomKind.Canteen:
                    for(int i=0;i<4;i++){float x=.5f+i*.92f;B(p,"Kitchen cabinet",x,.6f,h-.42f,.86f,.88f,.57f,"B1C6B4");B(p,"Stone worktop",x,1.08f,h-.42f,.9f,.06f,.61f,"EFE8D9");B(p,"Brass handle",x,.88f,h-.72f,.17f,.025f,.025f,"C7AD76",VisualSurface.Metal);}
                    B(p,"Oven door",1.4f,.61f,h-.73f,.61f,.53f,.025f,"617A89",VisualSurface.Glass);for(int i=0;i<2;i++){Desk(p,1.05f+i*1.75f,1.05f,1.2f);Chair(p,.8f+i*1.75f,.7f);Chair(p,1.3f+i*1.75f,.7f);C(p,"Plate",1.05f+i*1.75f,.73f,1.05f,.27f,.025f,.27f,"F6F2DE");S(p,"Fresh fruit",1.05f+i*1.75f,.8f,1.05f,.1f,.13f,.1f,"D7AD66");}break;
                case RoomKind.Toilet:
                    for(int i=0;i<2;i++){float x=.5f+i*.9f;C(p,"Toilet bowl",x,.4f,h-.55f,.4f,.34f,.47f,"FCF5E6");B(p,"Toilet tank",x,.67f,h-.31f,.38f,.52f,.18f,"F3F4E9");B(p,"Basin",x,.78f,.42f,.51f,.16f,.33f,"F3F4E9");B(p,"Mirror",x,1.34f,.14f,.48f,.64f,.03f,"B6D3D6",VisualSurface.Glass);}break;
                case RoomKind.Staffroom:
                    if(Has(r,RoomItem.RestSeats)){B(p,"Sofa frame",w-.51f,.52f,h/2,.65f,.6f,1.28f,"B29FBD",VisualSurface.Fabric);for(int i=0;i<3;i++)B(p,"Sofa cushion",w-.56f,.76f,h/2-.42f+i*.4f,.5f,.14f,.36f,"CAB9D2",VisualSurface.Fabric);}
                    Desk(p,1,.9f,.95f);Books(p,1,.73f,.9f,1);C(p,"Tea cup",1.2f,.77f,.9f,.1f,.12f,.1f,"E0B991");break;
                case RoomKind.Gym:
                    B(p,"Sports floor",w/2,.18f,h/2,w-.4f,.02f,h-.4f,"D7B68C",VisualSurface.Timber);for(int i=0;i<3;i++)B(p,"Court line",w/2,.195f,.45f+i*(h-.9f)/2,w-.7f,.009f,.035f,"FFF1D5");B(p,"Centre line",w/2,.197f,h/2,.035f,.009f,h-.6f,"FFF1D5");
                    B(p,"Basketball backboard",w/2,2.1f,h-.2f,.95f,.65f,.06f,"E8EFDB");B(p,"Basket rim",w/2,1.85f,h-.48f,.32f,.035f,.25f,"B98B63",VisualSurface.Metal);if(Has(r,RoomItem.SportsEquipment)){S(p,"Basketball",w/2+.6f,.36f,h/2,.35f,.35f,.35f,"D3A070");B(p,"Exercise mat",.7f,.21f,1,.55f,.08f,1.1f,"A6BCCA",VisualSurface.Fabric);}break;
            }
            if(Has(r,RoomItem.Plants))Plant(p,.32f,.37f,.85f);
            if(r.FurnishingVersion==1)ItemVisuals(p,r);
        }
        private static void ItemVisuals(List<VisualPart> p,Room r)
        {
            float w=r.Width,h=r.Height;foreach(RoomItem item in r.Items)
            {
                switch(item)
                {
                    case RoomItem.Storage:Shelf(p,w-.65f,h-.4f);break;
                    case RoomItem.TaskLighting:B(p,"Warm learning wall light",.2f,2.15f,h/2,.09f,.28f,.45f,"F2E2B9",VisualSurface.Screen);break;
                    case RoomItem.ErgonomicChairs:B(p,"Ergonomic lumbar cushions",w-.38f,.5f,.9f,.28f,.28f,.07f,"9EC7B0",VisualSurface.Fabric);break;
                    case RoomItem.SafetyKit:case RoomItem.LabSafety:case RoomItem.HygieneKit:B(p,"Safety cabinet",.16f,1.14f,h-.6f,.13f,.45f,.5f,"BDD0BE");B(p,"First aid sign",.237f,1.16f,h-.6f,.01f,.06f,.23f,"FFF4DC");B(p,"First aid cross",.24f,1.16f,h-.6f,.01f,.22f,.06f,"FFF4DC");break;
                    case RoomItem.AcousticPanels:for(int i=0;i<3;i++)B(p,"Sound absorbing wall panel",.15f,1.25f,.6f+i*.6f,.08f,.92f,.44f,"B1C6B9",VisualSurface.Fabric);break;
                    case RoomItem.DisplayScreen:B(p,"Interactive display frame",w/2,1.65f,h-.3f,1.62f,.91f,.08f,"647D85");B(p,"Interactive learning display",w/2,1.65f,h-.347f,1.5f,.81f,.019f,"A6CEC8",VisualSurface.Screen);for(int i=0;i<4;i++)B(p,"Interactive chart",w/2-.52f+i*.32f,1.6f,h-.363f,.18f,.25f+i*.09f,.012f,Accents[i]);break;
                    case RoomItem.AccessibilityDesk:Desk(p,w-.65f,.62f,.9f);B(p,"Accessible desk controls",w-.35f,.69f,.62f,.12f,.025f,.09f,"789F94");break;
                    case RoomItem.ClimateControl:B(p,"Climate unit",.16f,2.1f,h-.75f,.18f,.37f,.72f,"E8E9DD");for(int i=0;i<5;i++)B(p,"Air vent",.257f,2.01f+i*.04f,h-.75f,.009f,.011f,.55f,"AABCB6");break;
                    case RoomItem.DigitalLibrary:case RoomItem.RecordingDesk:case RoomItem.VideoCalls:case RoomItem.MediaStudio:case RoomItem.CADKit:Desk(p,w-.72f,h-.6f,.9f);Monitor(p,w-.72f,h-.6f);break;
                    case RoomItem.NetworkKit:B(p,"Network cabinet",w-.4f,.58f,h-.55f,.45f,.85f,.44f,"879FA6");for(int i=0;i<4;i++)B(p,"Network status light",w-.43f,.4f+i*.12f,h-.783f,.12f,.025f,.012f,"C9E4B4",VisualSurface.Screen);break;
                    case RoomItem.Fabricator:B(p,"Fabricator frame",w-.8f,.89f,h-.6f,.8f,1.4f,.8f,"89AAA7");B(p,"Fabricator glass",w-.8f,.98f,h-1.01f,.63f,.93f,.03f,"BCDDD0",VisualSurface.Glass);break;
                    case RoomItem.ElectronicsKit:B(p,"Circuit learning station",w-.8f,.75f,h-.6f,.55f,.08f,.35f,"75A593");for(int i=0;i<4;i++)B(p,"Circuit component",w-1+i*.13f,.83f,h-.6f,.07f,.07f,.08f,"D5BB88");break;
                    case RoomItem.CookingUtensils:case RoomItem.NutritionKit:case RoomItem.MealPrep:case RoomItem.FreshFood:C(p,"Food preparation bowl",w-.8f,1.16f,h-.45f,.35f,.2f,.35f,"E5C8A1");for(int i=0;i<3;i++)S(p,"Fresh produce",w-.9f+i*.09f,1.29f,h-.45f,.13f,.13f,.13f,Accents[i]);break;
                    case RoomItem.AllergyStation:B(p,"Labelled food storage",w-.7f,1.23f,h-.45f,.42f,.24f,.3f,"CCD9C1");break;
                    case RoomItem.ServingLine:B(p,"Extra serving station",w-.6f,.67f,1,.66f,.95f,.6f,"BBCCA9");break;
                    case RoomItem.StageLights:for(int i=0;i<3;i++)B(p,"Theatre spotlight",.6f+i*.8f,2.47f,h-.28f,.2f,.24f,.3f,"739D96",VisualSurface.Metal);break;
                    case RoomItem.SoundRig:B(p,"Stage speaker",w-.55f,1.18f,h-.5f,.36f,1.6f,.42f,"7D8292");S(p,"Speaker cone",w-.55f,1.35f,h-.72f,.22f,.22f,.025f,"B8C7C1");break;
                    case RoomItem.ProductionStage:B(p,"Flexible stage wing",.57f,.55f,h-.7f,.65f,.64f,1.1f,"CAA884",VisualSurface.Timber);break;
                    case RoomItem.LanguageBooks:Books(p,w-.7f,.76f,1.2f,4);break;
                    case RoomItem.Printmaking:B(p,"Printing press",w-.8f,.91f,h-.6f,.68f,.7f,.65f,"92ACB2",VisualSurface.Metal);C(p,"Printing roller",w-.8f,1.26f,h-.6f,.44f,.1f,.44f,"D3B4C4");break;
                    case RoomItem.QuietStudy:B(p,"Quiet study seat",w-.65f,.43f,1.1f,.7f,.55f,.7f,"B4CBB8",VisualSurface.Fabric);break;
                    case RoomItem.TeamSports:case RoomItem.AdaptiveSports:B(p,"Sports storage rack",w-.5f,.61f,h-.5f,.65f,.9f,.48f,"ACC5BE");for(int i=0;i<3;i++)S(p,"Sports ball",w-.71f+i*.21f,.91f,h-.5f,.2f,.2f,.2f,Accents[i]);break;
                    case RoomItem.Handwash:B(p,"Extra washbasin",w-.45f,.78f,1.15f,.5f,.17f,.33f,"EDF1E1");break;
                    case RoomItem.SensorTaps:B(p,"Sensor tap",.52f,.95f,.38f,.08f,.15f,.05f,"ABB9B5",VisualSurface.Metal);break;
                    case RoomItem.CoffeeStation:B(p,"Staff kitchenette",w-.6f,.6f,h-.4f,.78f,.88f,.45f,"C7B49E");C(p,"Tea pot",w-.6f,1.16f,h-.4f,.21f,.23f,.21f,"A2BDBB");break;
                    case RoomItem.TrainingCorner:B(p,"Team learning noticeboard",.16f,1.6f,h/2,.07f,.72f,1.02f,"D3C7AF");break;
                    case RoomItem.RainBarrel:C(p,"Rainwater barrel",w-.43f,.56f,h-.43f,.57f,.79f,.57f,"ABC2B5");break;
                    case RoomItem.OutdoorClass:Desk(p,w/2,1.1f,1.3f);Chair(p,w/2-.3f,.72f);Chair(p,w/2+.3f,.72f);break;
                    case RoomItem.WallArt:B(p,"Student gallery frame",.14f,1.56f,h/2,.07f,.76f,.92f,"D5B89B",VisualSurface.Timber);B(p,"Student gallery painting",.18f,1.56f,h/2,.016f,.64f,.8f,"B1CAB7");break;
                    case RoomItem.SignatureDecor:B(p,"Signature woven rug",w/2,.18f,h/2,1.6f,.026f,.92f,"C6B9CE",VisualSurface.Fabric);Plant(p,w-.4f,.38f,1.2f);break;
                }
            }
        }
        private static void Tree(List<VisualPart> p,float x,float z,float scale,int variety)
        {
            C(p,"Tree trunk",x,.85f*scale,z,.22f*scale,1.65f*scale,.22f*scale,"A18368",VisualSurface.Timber);
            for(int i=0;i<5;i++){float a=i*1.256f;S(p,"Sculpted canopy",x+(float)Math.Cos(a)*.4f*scale,1.85f*scale+(i%2)*.3f*scale,z+(float)Math.Sin(a)*.4f*scale,1.15f*scale,1.35f*scale,1.18f*scale,variety%5==0?"D7B2BB":i%2==0?"8BB285":"A4C394",VisualSurface.Leaf);}
        }
        private static void Bench(List<VisualPart> p,float x,float z)
        {
            for(int i=0;i<3;i++)B(p,"Bench oak slat",x,.4f,z-.14f+i*.13f,1.08f,.06f,.1f,"C7A481",VisualSurface.Timber);
            for(int i=-1;i<=1;i+=2)B(p,"Bench steel leg",x+i*.4f,.27f,z,.05f,.28f,.34f,"75938D",VisualSurface.Metal);
            B(p,"Bench back",x,.65f,z+.19f,1.1f,.3f,.065f,"D3B08A",VisualSurface.Timber);
        }
        public static List<VisualPart> Landscape()
        {
            var p=new List<VisualPart>();B(p,"Floating meadow plinth",16,-.45f,12,35,.8f,27,"7FA68E");B(p,"Soft meadow",16,-.03f,12,34.6f,.08f,26.6f,"B4CF9D");B(p,"Neighbourhood street",16,.005f,-1.4f,35,.05f,1.85f,"89999D");
            for(int i=0;i<18;i++)B(p,"Road stripe",i*2,.04f,-1.4f,.7f,.014f,.04f,"F2ECD7");
            for(int z=0;z<8;z++)for(int x=0;x<2;x++)B(p,"Entrance paver",14.1f+x*.8f,.06f,z+.45f,.75f,.07f,.88f,(x+z)%2==0?"DBDCCB":"D1D7C7");
            for(int i=0;i<20;i++)Tree(p,i<12?1+i*2.8f:i%2==0?.65f:31.4f,i<12?23.2f:2.5f+(i-12)*2.6f,.9f+(i%3)*.14f,i);
            for(int i=0;i<24;i++){float x=2+(i*7)%29,z=1+(i*11)%21;if(x>5&&x<27&&z>4&&z<22)continue;for(int j=0;j<3;j++)S(p,"Wildflower cluster",x+j*.14f,.18f,z,.15f,.2f,.15f,Accents[(i+j)%6]);}
            Bench(p,12.3f,4.1f);Bench(p,17.2f,4.1f);Plant(p,12.6f,2.4f,1.4f);Plant(p,17.1f,2.4f,1.4f);
            S(p,"Reflecting pond stone edge",3.7f,.015f,5.8f,3.1f,.14f,2.6f,"CCD3C0");S(p,"Reflecting pond",3.7f,.08f,5.8f,2.7f,.06f,2.2f,"99CAC4",VisualSurface.Water);
            for(int i=0;i<3;i++){float x=3+i*11;B(p,"Neighbour villa",x,1.1f,-4,4,2.2f,2.6f,Accents[i]);B(p,"Villa roof",x,2.3f,-4,4.2f,.18f,2.8f,"7D9098");for(int j=0;j<3;j++)B(p,"Villa window",x-1.2f+j*1.2f,1.3f,-2.68f,.65f,.9f,.05f,"CFDED5",VisualSurface.Glass);}
            for(int i=0;i<4;i++){float x=11.8f+i*1.9f;C(p,"Lantern post",x,1.05f,1.2f,.06f,2.1f,.06f,"79938B",VisualSurface.Metal);B(p,"Garden lantern",x,2.16f,1.2f,.25f,.3f,.25f,"F8E6BD",VisualSurface.Screen);}
            return p;
        }
        public static List<VisualPart> Person(int id,bool adult,string hair="796047",string gender="Diverse")
        {
            var p=new List<VisualPart>();string skin=id%3==0?"BE977A":"E6C8A4",shirt=adult?"799D9C":Accents[id%6];
            Add(p,"Rounded jacket",0,.65f,0,.36f,.49f,.25f,shirt,VisualShape.Capsule,VisualSurface.Fabric);
            B(p,"Jacket hem",0,.43f,0,.32f,.08f,.23f,shirt,VisualSurface.Fabric);S(p,"Head",0,1.06f,0,.31f,.34f,.3f,skin);S(p,"Hair cap",0,1.18f,.015f,.33f,.19f,.31f,hair);
            S(p,"Fringe",-.07f,1.15f,-.13f,.18f,.09f,.04f,hair);S(p,"Nose",0,1.045f,-.15f,.045f,.053f,.06f,skin);
            if(gender!="Male")S(p,"Back hair",0,1.03f,.12f,.32f,gender=="Female"?.35f:.22f,.17f,hair);
            for(int side=-1;side<=1;side+=2)
            {
                S(p,"Ear",side*.153f,1.06f,0,.043f,.09f,.054f,skin);S(p,"Eye white",side*.058f,1.075f,-.139f,.054f,.066f,.022f,"FFF8E9");S(p,"Pupil",side*.058f,1.075f,-.153f,.027f,.038f,.012f,"3C5363");S(p,"Eye glint",side*.057f-.006f,1.083f,-.159f,.009f,.013f,.009f,"FFFFFF");
                int arm=side==-1?1:2,leg=side==-1?3:4;
                Add(p,"Jacket sleeve",side*.215f,.65f,0,.12f,.31f,.14f,shirt,VisualShape.Capsule,VisualSurface.Fabric,0,arm);S(p,"Hand",side*.225f,.45f,-.015f,.095f,.13f,.1f,skin,VisualSurface.Matte,arm);
                B(p,"Trouser leg",side*.084f,.26f,0,.125f,.35f,.135f,"748A9A",VisualSurface.Fabric,0,leg);B(p,"Sneaker",side*.084f,.09f,-.035f,.16f,.1f,.24f,"EFEAD8",VisualSurface.Matte,0,leg);B(p,"Shoe sole",side*.084f,.044f,-.035f,.165f,.026f,.25f,"B5C5C3",VisualSurface.Matte,0,leg);
            }
            B(p,"Backpack pocket",0,.63f,.252f,.16f,.14f,.028f,"B28796");B(p,"Smile",0,.987f,-.145f,.049f,.012f,.013f,"9C7D6D");
            B(p,"White collar",0,.86f,-.13f,.15f,.045f,.035f,"F9F0DF");for(int i=0;i<3;i++)S(p,"Jacket button",0,.58f+i*.075f,-.133f,.022f,.022f,.012f,"DCCBA7");B(p,adult?"Staff badge":"Backpack",adult?.11f:0,adult?.75f:.63f,adult?-.15f:.17f,adult?.07f:.25f,adult?.1f:.33f,adult?.025f:.14f,adult?"F3E4C8":"C193A2");return p;
        }
    }
}

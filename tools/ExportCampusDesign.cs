using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using KoSch.SchoolTycoon.Core;
internal static class ExportCampusDesign
{
    public static int Main(string[] args)
    {
        var parts=new List<VisualPart>(CampusDesign.Landscape());
        Action<Room> room=r=>{foreach(VisualPart p in CampusDesign.Room(r,2,.5f,r.Kind!=RoomKind.Staircase||r.Floor<2)){p.X+=r.X;p.Y+=r.Floor*3;p.Z+=r.Y;parts.Add(p);}};
        room(new Room{Kind=RoomKind.Corridor,X=12,Y=8,Width=6,Height=2});
        var sample=new[]{new Room{Kind=RoomKind.Classroom,X=12,Y=10,Width=4,Height=3},new Room{Kind=RoomKind.Classroom,X=16,Y=10,Width=4,Height=3},new Room{Kind=RoomKind.Library,X=7,Y=10,Width=4,Height=3},new Room{Kind=RoomKind.MusicRoom,X=18,Y=5,Width=4,Height=3},new Room{Kind=RoomKind.ArtRoom,X=20,Y=10,Width=4,Height=3},new Room{Kind=RoomKind.Garden,X=7,Y=5,Width=3,Height=3},new Room{Kind=RoomKind.Garden,X=20,Y=15,Width=3,Height=3},new Room{Kind=RoomKind.FoodLab,X=12,Y=15,Width=4,Height=3},new Room{Kind=RoomKind.Toilet,X=16,Y=6,Width=2,Height=2},new Room{Kind=RoomKind.Staffroom,X=10,Y=6,Width=2,Height=3},new Room{Kind=RoomKind.Workshop,X=7,Y=15,Width=4,Height=3}};
        foreach(Room r in sample)room(r);
        for(int floor=0;floor<3;floor++){room(new Room{Kind=RoomKind.Staircase,X=12,Y=6,Width=2,Height=2,Floor=floor});if(floor>0){room(new Room{Kind=RoomKind.Corridor,X=12,Y=8,Width=6,Height=2,Floor=floor});room(new Room{Kind=floor==1?RoomKind.ScienceLab:RoomKind.ComputerLab,X=12,Y=10,Width=4,Height=3,Floor=floor,UpgradeLevel=1});}}
        for(int x=7;x<24;x++)if(x<12||x>=18)room(new Room{Kind=RoomKind.Corridor,X=x,Y=9,Width=1,Height=1});
        for(int x=10;x<=20;x++)room(new Room{Kind=RoomKind.Corridor,X=x,Y=14,Width=1,Height=1});
        for(int i=0;i<24;i++)foreach(VisualPart p in CampusDesign.Person(i,i>=20)){float sc=i>=20?.93f:.76f;p.X=p.X*sc+12.6f+(i%5)*1.65f;p.Y=p.Y*sc+.15f;p.Z=p.Z*sc+8.4f+(i/5)*1.25f;p.W*=sc;p.H*=sc;p.D*=sc;parts.Add(p);}
        if(args.Length>1&&args[1]=="progression")
        {
            parts.Clear();parts.Add(new VisualPart{Name="Gallery meadow",Shape=VisualShape.Box,Surface=VisualSurface.Matte,Color="AFC79F",X=4.5f,Y=-.08f,Z=1.5f,W=10,H=.2f,D=4.5f});
            var basic=new Room{Kind=RoomKind.Classroom,X=0,Y=0,Width=4,Height=3,FurnishingVersion=1};room(basic);
            var equipped=new Room{Kind=RoomKind.Classroom,X=5,Y=0,Width=4,Height=3,FurnishingVersion=1,UpgradeLevel=2};
            foreach(ItemSpec item in RektorCatalog.For(RoomKind.Classroom))equipped.Items.Add(item.Id);room(equipped);
        }
        File.WriteAllText(args.Length>0?args[0]:"campus-parts.json",JsonSerializer.Serialize(parts,new JsonSerializerOptions{IncludeFields=true}));Console.WriteLine(parts.Count+" original model parts exported. Offline art preview, not a Unity capture.");return 0;
    }
}

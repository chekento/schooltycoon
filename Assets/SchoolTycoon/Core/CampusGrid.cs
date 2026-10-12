using System;
using System.Collections.Generic;
using System.Linq;
namespace KoSch.SchoolTycoon.Core
{
    public static class CampusGrid
    {
        public const int Width=32, Height=24, MaxFloors=6;
        public const float FloorHeight=3f;
        public static readonly Cell Entrance=new Cell(14,8);
        public static bool Walkway(Room r) { return r.Kind==RoomKind.Corridor || r.Kind==RoomKind.Staircase; }
        public static Room At(SchoolState s, Cell c) { return s.Rooms.FirstOrDefault(r=>r.Contains(c)); }
        public static bool IsOwned(SchoolState s, Cell c)
        { return c.Floor>=0 && c.Floor<MaxFloors && c.X>=0 && c.Y>=0 && c.X<Width && c.Y<Height && (s.CampusLevel>0 || c.X>=6 && c.X<26 && c.Y>=5 && c.Y<21); }
        public static bool Supported(SchoolState s, Cell c)
        { if(c.Floor==0) return true; Room below=At(s,new Cell(c.X,c.Y,c.Floor-1)); return below!=null && below.Kind!=RoomKind.Garden; }
        public static bool StairLanding(SchoolState s, Room r)
        { return r.Floor==0 || s.Rooms.Any(b=>b.Kind==RoomKind.Staircase && b.Floor==r.Floor-1 && b.X==r.X && b.Y==r.Y && b.Width==r.Width && b.Height==r.Height); }
        public static HashSet<Cell> Corridors(SchoolState s, Room ignore=null)
        { return new HashSet<Cell>(s.Rooms.Where(r=>r!=ignore && Walkway(r)).SelectMany(r=>r.Cells())); }
        public static HashSet<Cell> Stairs(SchoolState s, Room ignore=null)
        { return new HashSet<Cell>(s.Rooms.Where(r=>r!=ignore && r.Kind==RoomKind.Staircase).SelectMany(r=>r.Cells())); }
        private static IEnumerable<Cell> Neighbours(Cell c, HashSet<Cell> stairs)
        {
            foreach(Cell d in Catalog.Directions) yield return c+d;
            if(stairs!=null && stairs.Contains(c)) for(int f=-1;f<=1;f+=2)
            { var n=new Cell(c.X,c.Y,c.Floor+f); if(stairs.Contains(n)) yield return n; }
        }
        public static HashSet<Cell> Reachable(SchoolState s, Room ignore=null) { return Reachable(Corridors(s,ignore),Stairs(s,ignore)); }
        public static HashSet<Cell> Reachable(HashSet<Cell> corridors, HashSet<Cell> stairs=null)
        {
            var visited=new HashSet<Cell>(); if(!corridors.Contains(Entrance)) return visited;
            var q=new Queue<Cell>();q.Enqueue(Entrance);visited.Add(Entrance);
            while(q.Count>0) foreach(Cell n in Neighbours(q.Dequeue(),stairs)) if(corridors.Contains(n) && visited.Add(n)) q.Enqueue(n);
            return visited;
        }
        public static bool TryDoor(Room r, HashSet<Cell> reachable, out Cell door, out Cell inside)
        {
            foreach(Cell c in r.Cells()) foreach(Cell d in Catalog.Directions)
            { Cell n=c+d; if(!r.Contains(n) && reachable.Contains(n)) { door=n;inside=c;return true; } }
            if(r.Kind==RoomKind.Staircase) foreach(Cell c in r.Cells()) if(reachable.Contains(c)) { door=inside=c;return true; }
            door=inside=default(Cell);return false;
        }
        public static bool IsConnected(SchoolState s,Room r)
        { var reachable=Reachable(s); if(Walkway(r)) return r.Cells().All(reachable.Contains);Cell door,inside;return TryDoor(r,reachable,out door,out inside); }
        public static List<Cell> Path(SchoolState s,Cell start,Cell end)
        {
            var walkable=Corridors(s);var stairs=Stairs(s);var parents=new Dictionary<Cell,Cell>();
            if(!walkable.Contains(start)||!walkable.Contains(end))return new List<Cell>();
            var q=new Queue<Cell>();q.Enqueue(start);parents[start]=start;
            while(q.Count>0)
            {
                Cell c=q.Dequeue(); if(c.Equals(end)) { var p=new List<Cell>{end};while(!c.Equals(start)){c=parents[c];p.Add(c);}p.Reverse();return p; }
                foreach(Cell n in Neighbours(c,stairs)) if(walkable.Contains(n)&&!parents.ContainsKey(n)){parents[n]=c;q.Enqueue(n);}
            }
            return new List<Cell>();
        }
    }
}

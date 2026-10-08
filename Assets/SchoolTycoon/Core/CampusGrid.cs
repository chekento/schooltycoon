using System;
using System.Collections.Generic;
using System.Linq;

namespace KoSch.SchoolTycoon.Core
{
    public static class CampusGrid
    {
        public const int Width = 32, Height = 24;
        public static readonly Cell Entrance = new Cell(14, 8);
        public static Room At(SchoolState s, Cell c) { return s.Rooms.FirstOrDefault(r => r.Contains(c)); }
        public static bool IsOwned(SchoolState s, Cell c)
        {
            if (c.X < 0 || c.Y < 0 || c.X >= Width || c.Y >= Height) return false;
            return s.CampusLevel > 0 || (c.X >= 6 && c.X < 26 && c.Y >= 5 && c.Y < 21);
        }
        public static HashSet<Cell> Corridors(SchoolState s, Room ignore = null)
        { return new HashSet<Cell>(s.Rooms.Where(r => r.Kind == RoomKind.Corridor && r != ignore).SelectMany(r => r.Cells())); }

        public static HashSet<Cell> Reachable(HashSet<Cell> corridors)
        {
            var visited = new HashSet<Cell>();
            if (!corridors.Contains(Entrance)) return visited;
            var queue = new Queue<Cell>();
            queue.Enqueue(Entrance); visited.Add(Entrance);
            while (queue.Count > 0)
            {
                Cell c = queue.Dequeue();
                foreach (Cell d in Catalog.Directions)
                {
                    Cell n = c + d;
                    if (corridors.Contains(n) && visited.Add(n)) queue.Enqueue(n);
                }
            }
            return visited;
        }
        public static bool TryDoor(Room room, HashSet<Cell> reachable, out Cell door, out Cell inside)
        {
            foreach (Cell c in room.Cells()) foreach (Cell d in Catalog.Directions)
            {
                Cell n = c + d;
                if (!room.Contains(n) && reachable.Contains(n)) { door = n; inside = c; return true; }
            }
            door = inside = default(Cell); return false;
        }
        public static bool IsConnected(SchoolState s, Room room)
        {
            var reachable = Reachable(Corridors(s));
            if (room.Kind == RoomKind.Corridor) return room.Cells().All(reachable.Contains);
            Cell door, inside; return TryDoor(room, reachable, out door, out inside);
        }
        public static List<Cell> Path(SchoolState s, Cell start, Cell end)
        {
            var walkable = Corridors(s);
            if (!walkable.Contains(start) || !walkable.Contains(end)) return new List<Cell>();
            var parents = new Dictionary<Cell, Cell>();
            var q = new Queue<Cell>(); q.Enqueue(start); parents[start] = start;
            while (q.Count > 0)
            {
                Cell c = q.Dequeue();
                if (c.Equals(end))
                {
                    var path = new List<Cell> { end };
                    while (!c.Equals(start)) { c = parents[c]; path.Add(c); }
                    path.Reverse(); return path;
                }
                foreach (Cell d in Catalog.Directions)
                {
                    Cell n = c + d;
                    if (walkable.Contains(n) && !parents.ContainsKey(n)) { parents[n] = c; q.Enqueue(n); }
                }
            }
            return new List<Cell>();
        }
    }
}

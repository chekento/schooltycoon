using System;
using System.Collections.Generic;
using System.Linq;

namespace KoSch.SchoolTycoon.Core
{
    public enum RoomKind { Corridor, Classroom, Toilet, Staffroom, Canteen, Library, Garden, ScienceLab, ArtRoom, Gym }
    public enum StaffRole { Teacher, Caretaker, Counselor }
    public enum Subject { Mathematics, Languages, Science, Arts, Sports }

    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public int X, Y;
        public Cell(int x, int y) { X = x; Y = y; }
        public bool Equals(Cell other) { return X == other.X && Y == other.Y; }
        public override bool Equals(object obj) { return obj is Cell && Equals((Cell)obj); }
        public override int GetHashCode() { return X * 397 ^ Y; }
        public static Cell operator +(Cell a, Cell b) { return new Cell(a.X + b.X, a.Y + b.Y); }
    }

    public sealed class RoomSpec
    {
        public RoomKind Kind;
        public int Width, Height, Cost, DailyCost, Capacity, UnlockLevel;
        public string NameDe, NameEn, DescriptionDe, DescriptionEn, Hex;
        public RoomSpec(RoomKind kind, int w, int h, int cost, int daily, int capacity, int unlock,
            string de, string en, string dde, string den, string hex)
        {
            Kind = kind; Width = w; Height = h; Cost = cost; DailyCost = daily; Capacity = capacity;
            UnlockLevel = unlock; NameDe = de; NameEn = en; DescriptionDe = dde; DescriptionEn = den; Hex = hex;
        }
    }

    public static class Catalog
    {
        public static readonly RoomSpec[] Rooms = {
            new RoomSpec(RoomKind.Corridor, 1, 1, 80, 1, 0, 0, "Flur", "Corridor", "Verbindet Räume und Eingang.", "Connects rooms to the entrance.", "B5CBDB"),
            new RoomSpec(RoomKind.Classroom, 4, 3, 3600, 36, 24, 0, "Klassenzimmer", "Classroom", "24 Plätze. Benötigt eine Lehrkraft.", "24 seats. Requires one teacher.", "F5C861"),
            new RoomSpec(RoomKind.Toilet, 2, 2, 1100, 14, 0, 0, "Sanitärraum", "Restroom", "Versorgt bis zu 48 Schüler.", "Serves up to 48 students.", "70D4C5"),
            new RoomSpec(RoomKind.Staffroom, 3, 2, 1800, 18, 0, 0, "Lehrerzimmer", "Staff room", "Stärkt Unterricht und Teamklima.", "Supports teaching and staff wellbeing.", "B6A2E5"),
            new RoomSpec(RoomKind.Canteen, 4, 3, 3000, 30, 0, 1, "Mensa", "Canteen", "Mittagessen für bis zu 72 Schüler.", "Lunch for up to 72 students.", "F6A782"),
            new RoomSpec(RoomKind.Library, 4, 3, 4800, 35, 0, 1, "Bibliothek", "Library", "Verbessert Lernerfolg in allen Fächern.", "Improves learning in every subject.", "86BBE9"),
            new RoomSpec(RoomKind.Garden, 3, 3, 800, 5, 0, 0, "Schulgarten", "School garden", "Erholung, Natur und gute Laune.", "Rest, nature and happy students.", "8CCB7A"),
            new RoomSpec(RoomKind.ScienceLab, 4, 3, 6500, 45, 0, 2, "Labor", "Science lab", "Verstärkt den naturwissenschaftlichen Unterricht.", "Boosts science lessons.", "74C9E4"),
            new RoomSpec(RoomKind.ArtRoom, 4, 3, 5200, 34, 0, 2, "Atelier", "Art studio", "Mehr Kreativität im Kunstunterricht.", "More creativity in arts lessons.", "EA96C0"),
            new RoomSpec(RoomKind.Gym, 5, 4, 9000, 60, 0, 3, "Sporthalle", "Gym", "Gesundheit und besserer Sportunterricht.", "Health and better sports lessons.", "F1AB5C")
        };
        public static RoomSpec Get(RoomKind kind) { return Rooms.First(r => r.Kind == kind); }
        public static readonly string[] StaffNames = { "Mila Weber", "Jonas Fischer", "Amira Yilmaz", "Luca Berger", "Sofia Nguyen", "Noah Schneider", "Lea Wagner", "Emma Hoffmann" };
        public static readonly Cell[] Directions = { new Cell(1, 0), new Cell(-1, 0), new Cell(0, 1), new Cell(0, -1) };
    }

    [Serializable]
    public sealed class Room
    {
        public int Id, X, Y, Width, Height;
        public RoomKind Kind;
        public bool Contains(Cell c) { return c.X >= X && c.Y >= Y && c.X < X + Width && c.Y < Y + Height; }
        public IEnumerable<Cell> Cells()
        {
            for (int x = X; x < X + Width; x++) for (int y = Y; y < Y + Height; y++) yield return new Cell(x, y);
        }
    }

    [Serializable]
    public sealed class Employee
    {
        public int Id, Skill, Salary;
        public string Name;
        public StaffRole Role;
        public Subject Specialty;
    }

    [Serializable]
    public sealed class Avatar
    {
        public string Name = "KoSch", Gender = "Diverse", Hair = "Brown";
    }

    [Serializable]
    public sealed class Ledger
    {
        public int Day, Students, Funding, Meals, Salaries, Maintenance, Supplies, Net, Joined, Left;
        public int Income { get { return Funding + Meals; } }
        public int Expenses { get { return Salaries + Maintenance + Supplies; } }
    }

    [Serializable]
    public sealed class SchoolState
    {
        public int SaveVersion = 2, Day = 1, Cash = 16000, Students = 18, Reputation = 30;
        public int Happiness = 65, Learning = 50, Cleanliness = 90, Level, CampusLevel, NextId = 3;
        public int CompletedGoals, PendingEvent = -1, EnrollmentPolicy = 1, SupplyBudget = 1;
        public int DebtDays, ClockMinute = 480;
        public bool HasDirector, GameOver;
        public string Language = "de";
        public Avatar Director = new Avatar();
        public List<Room> Rooms = new List<Room>();
        public List<Employee> Staff = new List<Employee>();
        public List<Subject> Timetable = new List<Subject> { Subject.Mathematics, Subject.Languages, Subject.Science, Subject.Arts, Subject.Sports, Subject.Mathematics };
        public List<Ledger> History = new List<Ledger>();
        public Ledger LastLedger = new Ledger();
        public List<string> News = new List<string>();
        public static SchoolState Create()
        {
            var state = new SchoolState();
            state.Rooms.Add(new Room { Id = 1, Kind = RoomKind.Corridor, X = 12, Y = 8, Width = 6, Height = 2 });
            state.Rooms.Add(new Room { Id = 2, Kind = RoomKind.Classroom, X = 12, Y = 10, Width = 4, Height = 3 });
            return state;
        }
    }

    public struct Result
    {
        public bool Success;
        public string Code;
        public static Result Ok() { return new Result { Success = true, Code = "ok" }; }
        public static Result Fail(string code) { return new Result { Code = code }; }
    }

    public sealed class Goal
    {
        public string TitleDe, TitleEn, DetailDe, DetailEn;
        public int Reward;
        public Func<SchoolState, bool> IsComplete;
        public Goal(string de, string en, string dde, string den, int reward, Func<SchoolState, bool> complete)
        { TitleDe = de; TitleEn = en; DetailDe = dde; DetailEn = den; Reward = reward; IsComplete = complete; }
    }

    public sealed class SchoolEvent
    {
        public string TitleDe, TitleEn, BodyDe, BodyEn;
        public string[] OptionsDe, OptionsEn;
        public int[] Costs, Happiness, Reputation, Learning;
    }
}

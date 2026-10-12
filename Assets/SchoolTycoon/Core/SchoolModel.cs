using System;
using System.Collections.Generic;
using System.Linq;

namespace KoSch.SchoolTycoon.Core
{
    public enum RoomKind { Corridor, Classroom, Toilet, Staffroom, Canteen, Library, Garden, ScienceLab, ArtRoom, Gym, Staircase, ComputerLab, MusicRoom, Workshop, FoodLab, Theatre, LanguageLab }
    public enum StaffRole { Teacher, Caretaker, Counselor }
    public enum Subject { Mathematics, Languages, Science, Arts, Sports, Algebra, Geometry, Trigonometry, Precalculus, Calculus, Statistics, FurtherMathematics, APCalculusAB, APCalculusBC, APStatistics, German, English, French, Spanish, Latin, AncientGreek, Italian, Russian, Mandarin, Japanese, Arabic, Portuguese, Turkish, Polish, Hebrew, EnglishAdditional, GermanAdditional, EnglishLiterature, GermanLiterature, AmericanLiterature, WorldLiterature, CreativeWriting, Communication, SignLanguage, Journalism, Biology, Chemistry, Physics, EarthScience, EnvironmentalScience, Astronomy, Anatomy, APBiology, APChemistry, APPhysics1, APPhysics2, APPhysicsCMechanics, APPhysicsCElectricity, APEnvironmentalScience, PrimaryStudies, History, Geography, WorldHistory, EuropeanHistory, USHistory, APWorldHistory, APUSHistory, APEuropeanHistory, APHumanGeography, Philosophy, Ethics, ReligiousEducation, ChristianReligion, OtherReligions, GlobalPerspectives, APAfricanAmericanStudies, Politics, Citizenship, USGovernment, ComparativeGovernment, SocialStudies, Economics, BusinessStudies, FinancialLiteracy, Law, Psychology, Sociology, Careers, ChildDevelopment, APUSGovernment, APComparativeGovernment, APMicroeconomics, APMacroeconomics, APPsychology, APSeminar, APResearch, Music, Choir, InstrumentalMusic, MusicTheory, Drama, Dance, FilmStudies, Photography, ArtHistory, GraphicDesign, APArtHistory, APMusicTheory, APArt2D, APArt3D, APDrawing, ComputerScience, Programming, DigitalLiteracy, Cybersecurity, DataScience, APComputerScienceA, APComputerSciencePrinciples, MediaStudies, DesignTechnology, Engineering, Robotics, Woodwork, Textiles, Electronics, Construction, Automotive, Agriculture, FoodNutrition, HomeEconomics, HealthEducation, RelationshipsHealth, PersonalSocialEducation, LifeSkills, HealthSocialCare, SportsScience, OutdoorEducation, APEnglishLanguage, APEnglishLiterature, APFrench, APSpanishLanguage, APSpanishLiterature, APGerman, APItalian, APJapanese, APChinese, APLatin, APPrecalculus, APBusinessFinance, APCybersecurity, APNetworking }

    [Serializable]
    public struct Cell : IEquatable<Cell>
    {
        public int X, Y, Floor;
        public Cell(int x, int y, int floor = 0) { X = x; Y = y; Floor = floor; }
        public bool Equals(Cell other) { return X == other.X && Y == other.Y && Floor == other.Floor; }
        public override bool Equals(object obj) { return obj is Cell && Equals((Cell)obj); }
        public override int GetHashCode() { return (X * 397 ^ Y) * 397 ^ Floor; }
        public static Cell operator +(Cell a, Cell b) { return new Cell(a.X + b.X, a.Y + b.Y, a.Floor + b.Floor); }
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
            new RoomSpec(RoomKind.Gym, 5, 4, 9000, 60, 24, 3, "Sporthalle", "Gym", "Gesundheit und besserer Sportunterricht.", "Health and better sports lessons.", "F1AB5C"),
            new RoomSpec(RoomKind.Staircase, 2, 2, 1800, 18, 0, 1, "Treppenhaus", "Stairwell", "Fachunterricht und Campusentwicklung.", "Specialist teaching and campus development.", "C9BCA8"),
            new RoomSpec(RoomKind.ComputerLab, 4, 3, 5200, 40, 24, 1, "Computerraum", "Computer lab", "Fachunterricht und Campusentwicklung.", "Specialist teaching and campus development.", "899AE1"),
            new RoomSpec(RoomKind.MusicRoom, 4, 3, 4200, 32, 24, 1, "Musikraum", "Music room", "Fachunterricht und Campusentwicklung.", "Specialist teaching and campus development.", "E6ADCA"),
            new RoomSpec(RoomKind.Workshop, 4, 3, 5800, 42, 24, 2, "Werkstatt", "Workshop", "Fachunterricht und Campusentwicklung.", "Specialist teaching and campus development.", "D3AD7D"),
            new RoomSpec(RoomKind.FoodLab, 4, 3, 4800, 36, 24, 1, "Lehrküche", "Food lab", "Fachunterricht und Campusentwicklung.", "Specialist teaching and campus development.", "A5CFB0"),
            new RoomSpec(RoomKind.Theatre, 4, 3, 5500, 38, 24, 2, "Theatersaal", "Theatre", "Fachunterricht und Campusentwicklung.", "Specialist teaching and campus development.", "CE99B6"),
            new RoomSpec(RoomKind.LanguageLab, 4, 3, 4400, 34, 24, 1, "Sprachlabor", "Language lab", "Fachunterricht und Campusentwicklung.", "Specialist teaching and campus development.", "B6C8E8")
        };
        public static RoomSpec Get(RoomKind kind) { return Rooms.First(r => r.Kind == kind); }
        public static readonly string[] StaffNames = { "Mila Weber", "Jonas Fischer", "Amira Yilmaz", "Luca Berger", "Sofia Nguyen", "Noah Schneider", "Lea Wagner", "Emma Hoffmann", "Theo Braun", "Zara Malik", "Paul Richter", "Hana Sato", "Ava Laurent", "Felix Krüger", "Nina Patel", "Milan Varga", "Lena Ortiz", "David Sommer", "Ada Hassan", "Jan Nowak", "Clara Jung", "Rami Haddad", "Eva Fuchs", "Luis Meyer" };
        public static readonly Cell[] Directions = { new Cell(1, 0), new Cell(-1, 0), new Cell(0, 1), new Cell(0, -1) };
    }

    [Serializable]
    public sealed class Room
    {
        public int Id, X, Y, Width, Height, Floor;
        public int UpgradeLevel, Condition = 100;
        public RoomKind Kind;
        public bool Contains(Cell c) { return c.Floor == Floor && c.X >= X && c.Y >= Y && c.X < X + Width && c.Y < Y + Height; }
        public IEnumerable<Cell> Cells()
        {
            for (int x = X; x < X + Width; x++) for (int y = Y; y < Y + Height; y++) yield return new Cell(x, y, Floor);
        }
    }

    [Serializable]
    public sealed class Employee
    {
        public int Id, Skill, Salary;
        public int Energy = 85, Morale = 75, TrainingLevel;
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
        public int Day, Students, Funding, Meals, Salaries, Maintenance, Supplies, Activities, Support, Curriculum, Net, Joined, Left;
        public int Income { get { return Funding + Meals; } }
        public int Expenses { get { return Salaries + Maintenance + Supplies + Activities + Support + Curriculum; } }
    }

    [Serializable]
    public sealed class SchoolState
    {
        public int SaveVersion = 4, Day = 1, Cash = 16000, Students = 18, Reputation = 30;
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
        public int NextPupilId = 1, SupportPolicy = 1, MealQuality = 1;
        public int ActiveResearch = -1, ResearchProgress, ResearchPoints, Graduates, ExamsHeld, AchievementMask;
        public List<Pupil> Pupils = new List<Pupil>();
        public List<SchoolClass> Classes = new List<SchoolClass>();
        public List<ResearchKind> CompletedResearch = new List<ResearchKind>();
        public List<ClubKind> Clubs = new List<ClubKind>();
        public List<ClubKind> ClubLicenses = new List<ClubKind>();
        public List<TermReport> Reports = new List<TermReport>();
        public DailyReport DailyReport = new DailyReport();
        public CurriculumRegion Region;
        public int GraduationGrade = 6, NextCustomSubject = 1000;
        public List<SubjectSpec> CustomSubjects = new List<SubjectSpec>();
        public int ActiveContract = -1, ContractYear, ContractProgress, Prestige;
        public List<CareerRecord> CareerHistory = new List<CareerRecord>();
        public static SchoolState Create()
        {
            var state = new SchoolState();
            state.Rooms.Add(new Room { Id = 1, Kind = RoomKind.Corridor, X = 12, Y = 8, Width = 6, Height = 2 });
            state.Rooms.Add(new Room { Id = 2, Kind = RoomKind.Classroom, X = 12, Y = 10, Width = 4, Height = 3 });
            SchoolSimulation.InitializeAcademy(state);
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

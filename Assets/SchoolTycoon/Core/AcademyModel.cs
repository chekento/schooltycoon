using System;
using System.Collections.Generic;

namespace KoSch.SchoolTycoon.Core
{
    public enum ResearchKind { DigitalLearning, GreenCampus, InclusiveLearning, CreativeCampus }
    public enum ClubKind { Gardening, Reading, Robotics, Theatre, Sports }
    public enum WeatherKind { Sunny, Cloudy, Rainy }

    [Serializable]
    public sealed class Pupil
    {
        public int Id, ClassRoomId, Grade = 1, Ability, Wellbeing = 65, Stress = 20;
        public int DaysEnrolled, DaysAttended, LastExam = 50, LastSupportDay;
        public string Name;
        public Subject Favourite;
        public bool Absent;
        public List<int> Scores = new List<int> { 45, 45, 45, 45, 45 };
    }

    [Serializable]
    public sealed class SchoolClass
    {
        public int RoomId, TeacherId;
        public string Name;
        public List<Subject> Lessons = new List<Subject>();
    }

    [Serializable]
    public sealed class TermReport
    {
        public int Day, Year, Term, Students, Average, Passed, Graduated, Repeated;
        public List<int> SubjectAverages = new List<int>();
    }

    [Serializable]
    public sealed class DailyReport
    {
        public int Day, Attended, Absent, Unstaffed, Joined, Left, ResearchGained;
        public int ResearchFinished = -1, TermFinished, NewAchievements;
        public List<int> StrugglingPupils = new List<int>();
    }

    public sealed class ResearchSpec
    {
        public ResearchKind Kind;
        public string NameDe, NameEn, DetailDe, DetailEn;
        public int Cost, Points, Level;
        public ResearchSpec(ResearchKind kind, string de, string en, string dde, string den, int cost, int points, int level)
        { Kind = kind; NameDe = de; NameEn = en; DetailDe = dde; DetailEn = den; Cost = cost; Points = points; Level = level; }
    }

    public sealed class ClubSpec
    {
        public ClubKind Kind;
        public RoomKind Room;
        public Subject Subject;
        public string NameDe, NameEn;
        public int Setup, Daily;
        public ClubSpec(ClubKind kind, RoomKind room, Subject subject, string de, string en, int setup, int daily)
        { Kind = kind; Room = room; Subject = subject; NameDe = de; NameEn = en; Setup = setup; Daily = daily; }
    }

    public static class AcademyCatalog
    {
        public static readonly ResearchSpec[] Research = {
            new ResearchSpec(ResearchKind.DigitalLearning, "Digitale Lernwerkstatt", "Digital learning", "+6 Unterrichtsqualität, +2 Forschung pro Tag.", "+6 teaching quality, +2 research per day.", 2200, 45, 1),
            new ResearchSpec(ResearchKind.GreenCampus, "Nachhaltiger Campus", "Green campus", "20 % weniger Raumkosten, +4 Zufriedenheit.", "20% lower room running costs, +4 happiness.", 1800, 35, 1),
            new ResearchSpec(ResearchKind.InclusiveLearning, "Individuelle Förderung", "Inclusive learning", "Förderung ist wirksamer; +5 Unterrichtsqualität und weniger Stress.", "Stronger support, +5 teaching quality and less stress.", 3200, 60, 2),
            new ResearchSpec(ResearchKind.CreativeCampus, "Kreativer Campus", "Creative campus", "AGs fördern alle Fächer; +5 Wohlbefinden.", "Clubs improve every subject; +5 wellbeing.", 3800, 70, 2)
        };
        public static readonly ClubSpec[] Clubs = {
            new ClubSpec(ClubKind.Gardening, RoomKind.Garden, Subject.Science, "Garten-AG", "Gardening club", 350, 18),
            new ClubSpec(ClubKind.Reading, RoomKind.Library, Subject.Languages, "Leseclub", "Reading club", 450, 22),
            new ClubSpec(ClubKind.Robotics, RoomKind.ScienceLab, Subject.Mathematics, "Robotik-AG", "Robotics club", 900, 38),
            new ClubSpec(ClubKind.Theatre, RoomKind.ArtRoom, Subject.Arts, "Theater-AG", "Theatre club", 650, 28),
            new ClubSpec(ClubKind.Sports, RoomKind.Gym, Subject.Sports, "Sport-AG", "Sports club", 600, 25)
        };
        public static readonly string[] FirstNames = { "Alex", "Emil", "Nora", "Sam", "Juna", "Ben", "Aylin", "Finn", "Mia", "Elias", "Lina", "Tom", "Yara", "Leon", "Zoe", "Omar", "Ella", "Niko", "Ida", "Mika" };
        public static readonly string[] LastNames = { "Klein", "Chen", "Schmidt", "Kaya", "Koch", "Ali", "Becker", "Novak", "Santos", "Roth", "Kim", "Bauer", "Singh", "Wolf", "Martin", "Schulz" };
        public static readonly string[] AchievementDe = { "Erstes Zeugnis", "Lernen mit Zukunft", "Eine aktive Schule", "Starkes Schuljahr", "Erster Abschluss", "Raum zum Lernen" };
        public static readonly string[] AchievementEn = { "First report card", "Learning for the future", "An active school", "A strong school year", "First graduate", "Room to learn" };
        public static readonly int[] AchievementRewards = { 1200, 1800, 1600, 3000, 3500, 1400 };
    }
}

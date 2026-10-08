using System;
using System.Collections.Generic;
using System.Linq;

namespace KoSch.SchoolTycoon.Core
{
    public sealed partial class SchoolSimulation
    {
        public int SchoolYear { get { return (State.Day - 1) / 60 + 1; } }
        public int YearDay { get { return (State.Day - 1) % 60 + 1; } }
        public int Term { get { return (YearDay - 1) / 20 + 1; } }
        public int Season { get { return (State.Day - 1) / 15 % 4; } }
        public WeatherKind Weather { get { return State.Day % 7 == 2 || State.Day % 7 == 5 ? WeatherKind.Rainy : State.Day % 3 == 0 ? WeatherKind.Cloudy : WeatherKind.Sunny; } }
        public bool Researched(ResearchKind kind) { return State.CompletedResearch.Contains(kind); }
        public int RoomSeats(Room room) { return room.Kind == RoomKind.Classroom ? 24 + room.UpgradeLevel * 4 : 0; }
        public Employee ClassTeacher(SchoolClass group)
        { return group == null ? null : State.Staff.FirstOrDefault(p => p.Id == group.TeacherId && p.Role == StaffRole.Teacher); }
        public SchoolClass PupilClass(Pupil pupil) { return State.Classes.FirstOrDefault(c => c.RoomId == pupil.ClassRoomId); }
        public int ClassSize(SchoolClass group) { return State.Pupils.Count(p => p.ClassRoomId == group.RoomId); }
        public int EffectiveSkill(Employee person) { return Clamp(person.Skill - Math.Max(0, 50 - person.Energy) / 3 - Math.Max(0, 45 - person.Morale) / 3, 1, 100); }
        public int ActiveClubCount { get { return State.Clubs.Count(ClubAvailable); } }
        public bool ClubAvailable(ClubKind kind) { return Count(AcademyCatalog.Clubs[(int)kind].Room) > 0; }
        public static int LessonSlot(int minute)
        { int phase = (minute - 480) / 60; return phase < 0 || phase == 4 || phase >= 7 ? -1 : phase < 4 ? phase : phase - 1; }

        public static void InitializeAcademy(SchoolState state)
        {
            state.Pupils = new List<Pupil>(); state.Classes = new List<SchoolClass>();
            state.NextPupilId = 1;
            var sim = new SchoolSimulation(state);
            sim.ReconcileClasses(true);
            for (int i = 0; i < state.Students; i++) state.Pupils.Add(sim.CreatePupil());
            sim.PlacePupils(false);
        }

        private Pupil CreatePupil()
        {
            int id = State.NextPupilId++, seed = (id * 37 + State.Day * 11) % 101;
            return new Pupil { Id = id, Name = AcademyCatalog.FirstNames[(id - 1) % AcademyCatalog.FirstNames.Length] + " " + AcademyCatalog.LastNames[(id / AcademyCatalog.FirstNames.Length + id * 3) % AcademyCatalog.LastNames.Length],
                Ability = 55 + seed % 36, Favourite = (Subject)(id % 5), Wellbeing = State.Happiness, Scores = Enumerable.Range(0, 5).Select(s => 40 + (seed + s * 7) % 16).ToList() };
        }
        private void ReconcileClasses(bool assignTeachers, bool newClassesOnly = false)
        {
            var added = new HashSet<int>();
            State.Classes.RemoveAll(c => !State.Rooms.Any(r => r.Id == c.RoomId && r.Kind == RoomKind.Classroom));
            foreach (Room room in State.Rooms.Where(r => r.Kind == RoomKind.Classroom))
                if (!State.Classes.Any(c => c.RoomId == room.Id))
                {
                    State.Classes.Add(new SchoolClass { RoomId = room.Id, Name = "K" + room.Id, Lessons = new List<Subject>(State.Timetable) });
                    added.Add(room.Id);
                }
            foreach (SchoolClass c in State.Classes) if (ClassTeacher(c) == null) c.TeacherId = 0;
            if (assignTeachers)
                foreach (SchoolClass c in State.Classes.Where(c => c.TeacherId == 0 && (!newClassesOnly || added.Contains(c.RoomId))))
                {
                    Employee teacher = State.Staff.FirstOrDefault(p => p.Role == StaffRole.Teacher && !State.Classes.Any(g => g.TeacherId == p.Id));
                    if (teacher != null) c.TeacherId = teacher.Id;
                }
            PlacePupils(false);
        }
        private void PlacePupils(bool staffedOnly)
        {
            foreach (Pupil p in State.Pupils)
                if (!State.Classes.Any(c => c.RoomId == p.ClassRoomId)) p.ClassRoomId = 0;
            foreach (Pupil pupil in State.Pupils.Where(p => p.ClassRoomId == 0))
            {
                SchoolClass group = State.Classes.Where(c => (!staffedOnly || ClassTeacher(c) != null) && ClassSize(c) < RoomSeats(State.Rooms.First(r => r.Id == c.RoomId)))
                    .OrderBy(c => ClassTeacher(c) == null ? 1 : 0).ThenBy(ClassSize).FirstOrDefault();
                if (group != null) pupil.ClassRoomId = group.RoomId;
            }
        }
        public Result AssignTeacher(int roomId, int teacherId)
        {
            if (!CanOperate) return Result.Fail("blocked");
            SchoolClass group = State.Classes.FirstOrDefault(c => c.RoomId == roomId);
            if (group == null || teacherId != 0 && !State.Staff.Any(p => p.Id == teacherId && p.Role == StaffRole.Teacher)) return Result.Fail("invalid");
            foreach (SchoolClass c in State.Classes.Where(c => c.TeacherId == teacherId && teacherId != 0)) c.TeacherId = 0;
            group.TeacherId = teacherId; return Result.Ok();
        }
        public Result AssignPupil(int pupilId, int roomId)
        {
            if (!CanOperate) return Result.Fail("blocked");
            Pupil pupil = State.Pupils.FirstOrDefault(p => p.Id == pupilId);
            SchoolClass group = State.Classes.FirstOrDefault(c => c.RoomId == roomId);
            if (pupil == null || group == null) return Result.Fail("invalid");
            if (pupil.ClassRoomId == roomId) return Result.Ok();
            if (ClassSize(group) >= RoomSeats(State.Rooms.First(r => r.Id == roomId))) return Result.Fail("full");
            pupil.ClassRoomId = roomId; return Result.Ok();
        }
        public Result SetClassLesson(int roomId, int slot, Subject subject)
        {
            if (!CanOperate) return Result.Fail("blocked");
            SchoolClass group = State.Classes.FirstOrDefault(c => c.RoomId == roomId);
            if (group == null || slot < 0 || slot >= 6 || !Enum.IsDefined(typeof(Subject), subject)) return Result.Fail("invalid");
            group.Lessons[slot] = subject; return Result.Ok();
        }
        public Result SetClassName(int roomId, string name)
        {
            if (!CanOperate) return Result.Fail("blocked");
            SchoolClass group = State.Classes.FirstOrDefault(c => c.RoomId == roomId);
            if (group == null || string.IsNullOrWhiteSpace(name) || name.Trim().Length > 20) return Result.Fail("invalid");
            group.Name = name.Trim(); return Result.Ok();
        }
        public int UpgradeCost(Room room) { return Catalog.Get(room.Kind).Cost / 3 + room.UpgradeLevel * Catalog.Get(room.Kind).Cost / 4; }
        public Result UpgradeRoom(int id)
        {
            if (!CanOperate) return Result.Fail("blocked");
            Room room = State.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null || room.Kind == RoomKind.Corridor) return Result.Fail("invalid");
            if (room.UpgradeLevel >= 2) return Result.Fail("maxed");
            int cost = UpgradeCost(room);
            if (State.Cash < cost) return Result.Fail("funds");
            State.Cash -= cost; room.UpgradeLevel++; room.Condition = 100;
            PlacePupils(false); CheckAchievements(); return Result.Ok();
        }
        public int RepairCost(Room room) { return (100 - room.Condition) * (room.Kind == RoomKind.Corridor ? room.Width * room.Height : 4); }
        public Result RepairRoom(int id)
        {
            if (!CanOperate) return Result.Fail("blocked");
            Room room = State.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return Result.Fail("invalid");
            int cost = RepairCost(room);
            if (State.Cash < cost && cost > 0) return Result.Fail("funds");
            State.Cash -= cost; room.Condition = 100; return Result.Ok();
        }
        public int TrainingCost(Employee employee) { return 500 + employee.TrainingLevel * 250; }
        public Result TrainStaff(int id)
        {
            if (!CanOperate) return Result.Fail("blocked");
            Employee employee = State.Staff.FirstOrDefault(p => p.Id == id);
            if (employee == null) return Result.Fail("invalid");
            if (employee.TrainingLevel >= 3 || employee.Salary > 495) return Result.Fail("maxed");
            int cost = TrainingCost(employee);
            if (State.Cash < cost) return Result.Fail("funds");
            State.Cash -= cost; employee.TrainingLevel++; employee.Skill = Clamp(employee.Skill + 8, 1, 100);
            employee.Salary += 5; employee.Morale = Clamp(employee.Morale + 8, 0, 100); return Result.Ok();
        }
        public Result SupportPupil(int id, int option)
        {
            if (!CanOperate) return Result.Fail("blocked");
            Pupil pupil = State.Pupils.FirstOrDefault(p => p.Id == id);
            if (pupil == null || option < 0 || option > 2) return Result.Fail("invalid");
            // One intervention per pupil per school day, tracked by a persistent day stamp.
            if (pupil.LastSupportDay == State.Day) return Result.Fail("done");
            int cost = option == 0 ? 90 : option == 1 ? 60 : 0;
            if (State.Cash < cost && cost > 0) return Result.Fail("funds");
            State.Cash -= cost; pupil.LastSupportDay = State.Day;
            if (option == 0) { int weakest = pupil.Scores.IndexOf(pupil.Scores.Min()); pupil.Scores[weakest] = Clamp(pupil.Scores[weakest] + 8, 0, 100); pupil.Stress = Clamp(pupil.Stress + 3, 0, 100); }
            else { pupil.Stress = Clamp(pupil.Stress - (option == 1 ? 18 : 6), 0, 100); pupil.Wellbeing = Clamp(pupil.Wellbeing + (option == 1 ? 10 : 3), 0, 100); }
            return Result.Ok();
        }
        public Result SetWellbeingPolicy(int support, int meals)
        {
            if (!CanOperate) return Result.Fail("blocked");
            if (support < 0 || support > 2 || meals < 0 || meals > 2) return Result.Fail("invalid");
            State.SupportPolicy = support; State.MealQuality = meals; return Result.Ok();
        }
        public Result StartResearch(ResearchKind kind)
        {
            if (!CanOperate) return Result.Fail("blocked");
            if (!Enum.IsDefined(typeof(ResearchKind), kind)) return Result.Fail("invalid");
            if (State.ActiveResearch >= 0) return Result.Fail("busy");
            if (Researched(kind)) return Result.Fail("done");
            ResearchSpec spec = AcademyCatalog.Research[(int)kind];
            if (State.Level < spec.Level || Count(RoomKind.Library) == 0) return Result.Fail("researchlocked");
            if (State.Cash < spec.Cost) return Result.Fail("funds");
            State.Cash -= spec.Cost; State.ActiveResearch = (int)kind; State.ResearchProgress = 0; return Result.Ok();
        }
        public Result SetClub(ClubKind kind, bool active)
        {
            if (!CanOperate) return Result.Fail("blocked");
            if (!Enum.IsDefined(typeof(ClubKind), kind)) return Result.Fail("invalid");
            bool enabled = State.Clubs.Contains(kind);
            if (enabled == active) return Result.Ok();
            if (!active) { State.Clubs.Remove(kind); return Result.Ok(); }
            if (!ClubAvailable(kind)) return Result.Fail("facility");
            int fee = State.ClubLicenses.Contains(kind) ? 0 : AcademyCatalog.Clubs[(int)kind].Setup;
            if (State.Cash < fee) return Result.Fail("funds");
            State.Cash -= fee; State.Clubs.Add(kind);
            if (!State.ClubLicenses.Contains(kind)) State.ClubLicenses.Add(kind);
            CheckAchievements(); return Result.Ok();
        }

        // The renderer uses the same timetable, assignment and facilities as the simulation.
        public Room Destination(Pupil pupil, Employee employee, int minute, int variation)
        {
            SchoolClass group = pupil != null ? PupilClass(pupil) : employee != null ? State.Classes.FirstOrDefault(c => c.TeacherId == employee.Id) : null;
            if (pupil != null && pupil.Absent) return null;
            RoomKind kind; int slot = LessonSlot(minute);
            if (minute >= 900)
            {
                List<ClubKind> clubs = State.Clubs.Where(ClubAvailable).ToList();
                if (pupil != null && clubs.Count > 0) kind = AcademyCatalog.Clubs[(int)clubs[variation % clubs.Count]].Room;
                else kind = Weather == WeatherKind.Rainy ? RoomKind.Library : RoomKind.Garden;
            }
            else if (minute >= 720 && minute < 780) kind = RoomKind.Canteen;
            else if (pupil == null && employee != null && employee.Role != StaffRole.Teacher)
                kind = employee.Role == StaffRole.Caretaker ? RoomKind.Toilet : RoomKind.Staffroom;
            else if (group != null && ClassTeacher(group) != null && slot >= 0)
            {
                Subject subject = group.Lessons[slot];
                kind = subject == Subject.Science ? RoomKind.ScienceLab : subject == Subject.Arts ? RoomKind.ArtRoom : subject == Subject.Sports ? RoomKind.Gym : RoomKind.Classroom;
            }
            else kind = RoomKind.Classroom;
            List<Room> rooms = State.Rooms.Where(r => r.Kind == kind).ToList();
            if (kind == RoomKind.Classroom || rooms.Count == 0)
            {
                Room home = group == null ? null : State.Rooms.FirstOrDefault(r => r.Id == group.RoomId);
                return home ?? State.Rooms.FirstOrDefault(r => r.Kind == RoomKind.Staffroom) ?? State.Rooms.FirstOrDefault(r => r.Kind == RoomKind.Classroom);
            }
            return rooms[Math.Abs(variation) % rooms.Count];
        }

        private int RunAcademyDay()
        {
            var report = new DailyReport { Day = State.Day };
            State.DailyReport = report;
            int counselorSupport = State.Staff.Where(p => p.Role == StaffRole.Counselor).Sum(p => EffectiveSkill(p) / 25 + p.TrainingLevel);
            foreach (Pupil pupil in State.Pupils)
            {
                pupil.DaysEnrolled++;
                SchoolClass group = PupilClass(pupil); Employee teacher = ClassTeacher(group);
                if (pupil.Absent) report.Absent++; else { pupil.DaysAttended++; report.Attended++; }
                if (teacher == null) report.Unstaffed++;
                int support = State.SupportPolicy * 2 + counselorSupport + (Researched(ResearchKind.InclusiveLearning) ? 4 : 0);
                int targetStress = 24 + (teacher == null ? 30 : 0) + Math.Max(0, 60 - State.Happiness) / 2 - support - ActiveClubCount * 2;
                pupil.Stress = Clamp((pupil.Stress * 3 + targetStress) / 4, 0, 100);
                pupil.Wellbeing = Clamp((pupil.Wellbeing * 2 + State.Happiness + ActiveClubCount * 2 + (Researched(ResearchKind.CreativeCampus) ? 5 : 0) - pupil.Stress / 12) / 3, 0, 100);
                if (teacher != null && !pupil.Absent)
                {
                    Room room = State.Rooms.First(r => r.Id == group.RoomId);
                    foreach (Subject subject in group.Lessons.Distinct())
                    {
                        int target = EffectiveSkill(teacher) / 2 + pupil.Ability / 3 + State.SupplyBudget * 3 + room.UpgradeLevel * 3 - Math.Max(0, 65 - room.Condition) / 8;
                        target += teacher.Specialty == subject ? 6 : 0; target += pupil.Favourite == subject ? 5 : 0;
                        target += State.Rooms.Where(r => r.Kind == RoomKind.Staffroom).Sum(r => 2 + r.UpgradeLevel * 2);
                        target += FacilityLearning(subject) + (Researched(ResearchKind.DigitalLearning) ? 6 : 0) + (Researched(ResearchKind.InclusiveLearning) ? 5 : 0);
                        target -= pupil.Stress / 10 + (pupil.Grade - 1) * 3;
                        int score = pupil.Scores[(int)subject], difference = Clamp(target, 0, 100) - score;
                        pupil.Scores[(int)subject] = Clamp(score + (difference == 0 ? 0 : Math.Sign(difference) * Math.Max(1, Math.Abs(difference) / 10)), 0, 100);
                    }
                }
                foreach (Subject subject in Enum.GetValues(typeof(Subject)))
                {
                    if (State.Clubs.Any(k => ClubAvailable(k) && (AcademyCatalog.Clubs[(int)k].Subject == subject || Researched(ResearchKind.CreativeCampus))) && !pupil.Absent)
                        pupil.Scores[(int)subject] = Clamp(pupil.Scores[(int)subject] + 1, 0, 100);
                    if (State.SupportPolicy == 2 && pupil.Scores[(int)subject] < 50 && teacher != null && !pupil.Absent)
                        pupil.Scores[(int)subject] = Clamp(pupil.Scores[(int)subject] + 1, 0, 100);
                }
            }
            report.StrugglingPupils = State.Pupils.Where(p => p.Scores.Min() < 45 || p.Stress > 65 || p.Wellbeing < 40).OrderBy(p => p.Scores.Min()).Take(5).Select(p => p.Id).ToList();
            foreach (Employee employee in State.Staff)
            {
                bool teaching = State.Classes.Any(c => c.TeacherId == employee.Id);
                int staffroomLevel = State.Rooms.Where(r => r.Kind == RoomKind.Staffroom).Select(r => r.UpgradeLevel).DefaultIfEmpty(-1).Max();
                employee.Energy = Clamp(employee.Energy + (staffroomLevel >= 0 ? 6 + staffroomLevel * 2 : 2) - (employee.Role == StaffRole.Teacher && teaching ? 4 : 2), 0, 100);
                employee.Morale = Clamp((employee.Morale * 3 + State.Happiness + employee.TrainingLevel * 3 + (staffroomLevel >= 0 ? 8 + staffroomLevel * 3 : -6)) / 4, 0, 100);
            }
            int care = State.Staff.Where(p => p.Role == StaffRole.Caretaker).Sum(p => 2 + p.TrainingLevel / 2);
            foreach (Room room in State.Rooms) room.Condition = Clamp(room.Condition - 1 - (State.Students > 72 ? 1 : 0) + care, 20, 100);
            if (State.ActiveResearch >= 0 && Count(RoomKind.Library) > 0)
            {
                int points = 2 + State.Rooms.Where(r => r.Kind == RoomKind.Library).Sum(r => 2 + r.UpgradeLevel) + State.Learning / 25 + (Researched(ResearchKind.DigitalLearning) ? 2 : 0);
                report.ResearchGained = points; State.ResearchPoints += points; State.ResearchProgress += points;
                if (State.ResearchProgress >= AcademyCatalog.Research[State.ActiveResearch].Points)
                {
                    report.ResearchFinished = State.ActiveResearch;
                    State.CompletedResearch.Add((ResearchKind)State.ActiveResearch); State.ActiveResearch = -1; State.ResearchProgress = 0;
                }
            }
            int graduated = 0; var graduateIds = new HashSet<int>();
            if (State.Day % 20 == 0)
            {
                var term = new TermReport { Day = State.Day, Year = SchoolYear, Term = Term, Students = State.Pupils.Count };
                foreach (Pupil pupil in State.Pupils)
                {
                    int attendancePenalty = pupil.DaysEnrolled == 0 ? 0 : Math.Max(0, 90 - pupil.DaysAttended * 100 / pupil.DaysEnrolled) / 3;
                    pupil.LastExam = Clamp((int)pupil.Scores.Average() - attendancePenalty - pupil.Stress / 15, 0, 100);
                    if (pupil.LastExam >= 50) term.Passed++;
                    if (State.Day % 60 == 0)
                    {
                        if (pupil.LastExam < 45) term.Repeated++;
                        else if (pupil.Grade == 6) { term.Graduated++; graduated++; graduateIds.Add(pupil.Id); }
                        else pupil.Grade++;
                    }
                }
                term.Average = term.Students == 0 ? 0 : (int)State.Pupils.Average(p => p.LastExam);
                term.SubjectAverages = Enumerable.Range(0, 5).Select(s => term.Students == 0 ? 0 : (int)State.Pupils.Average(p => p.Scores[s])).ToList();
                State.Reports.Add(term); if (State.Reports.Count > 30) State.Reports.RemoveAt(0);
                State.ExamsHeld++; report.TermFinished = term.Term;
                if (graduated > 0) State.Pupils.RemoveAll(p => graduateIds.Contains(p.Id));
                State.Graduates += graduated; State.Reputation = Clamp(State.Reputation + (term.Average >= 65 ? 3 : term.Average < 45 ? -3 : 0), 0, 100);
            }
            State.Students = State.Pupils.Count;
            CheckAchievements();
            return graduated;
        }
        private int FacilityLearning(Subject subject)
        {
            int result = State.Rooms.Where(r => r.Kind == RoomKind.Library).Sum(r => 3 + r.UpgradeLevel);
            RoomKind kind = subject == Subject.Science ? RoomKind.ScienceLab : subject == Subject.Arts ? RoomKind.ArtRoom : subject == Subject.Sports ? RoomKind.Gym : RoomKind.Classroom;
            if (kind != RoomKind.Classroom) result += State.Rooms.Where(r => r.Kind == kind).Sum(r => 5 + r.UpgradeLevel * 2);
            return result;
        }
        private void ChangeEnrollment(int joined, int left)
        {
            var leaving = State.Pupils.OrderBy(p => ClassTeacher(PupilClass(p)) == null ? 0 : 1).ThenBy(p => p.Wellbeing).Take(left).ToList();
            foreach (Pupil pupil in leaving) State.Pupils.Remove(pupil);
            for (int i = 0; i < joined; i++) State.Pupils.Add(CreatePupil());
            PlacePupils(true); State.Students = State.Pupils.Count;
        }
        private void PrepareAttendance()
        {
            foreach (Pupil pupil in State.Pupils)
                pupil.Absent = pupil.Stress >= 70 && (pupil.Id + State.Day) % 5 == 0 || pupil.Wellbeing < 35 && (pupil.Id + State.Day) % 7 == 0;
        }
        private void CheckAchievements()
        {
            bool[] conditions = { State.ExamsHeld > 0, State.CompletedResearch.Count >= 2, ActiveClubCount >= 3,
                State.Reports.Any(t => t.Term == 3 && t.Average >= 70), State.Graduates > 0, State.Rooms.Any(r => r.UpgradeLevel == 2 && r.Kind == RoomKind.Classroom) };
            for (int i = 0; i < conditions.Length; i++)
                if (conditions[i] && (State.AchievementMask & (1 << i)) == 0)
                {
                    State.AchievementMask |= 1 << i; State.Cash += AcademyCatalog.AchievementRewards[i];
                    State.DailyReport.NewAchievements |= 1 << i;
                }
        }

        public static bool MigrateSave(SchoolState state)
        {
            if (state == null) return false;
            if (state.SaveVersion == 3) return ValidateSave(state);
            if (state.SaveVersion != 2 || !ValidateLegacySave(state)) return false;
            // Validate the legacy state before generating anything. Never repair a corrupt campus silently.
            foreach (Room room in state.Rooms) { room.Condition = 100; room.UpgradeLevel = 0; }
            foreach (Employee employee in state.Staff) { employee.Energy = 85; employee.Morale = 75; employee.TrainingLevel = 0; }
            state.SaveVersion = 3; state.SupportPolicy = 1; state.MealQuality = 1;
            state.ActiveResearch = -1; state.ResearchProgress = 0; state.ResearchPoints = 0;
            state.Graduates = 0; state.ExamsHeld = 0; state.AchievementMask = 0;
            state.CompletedResearch = new List<ResearchKind>(); state.Clubs = new List<ClubKind>(); state.ClubLicenses = new List<ClubKind>();
            state.Reports = new List<TermReport>(); state.DailyReport = new DailyReport();
            InitializeAcademy(state); return ValidateSave(state);
        }

        private static bool ValidateAcademy(SchoolState s)
        {
            if (s.Pupils == null || s.Classes == null || s.CompletedResearch == null || s.Clubs == null || s.ClubLicenses == null || s.Reports == null || s.DailyReport == null || s.DailyReport.StrugglingPupils == null) return false;
            if (s.Pupils.Count != s.Students || s.Pupils.Count > 1000 || s.NextPupilId < 1 || s.NextPupilId > 10000000 || s.SupportPolicy < 0 || s.SupportPolicy > 2 || s.MealQuality < 0 || s.MealQuality > 2) return false;
            if (s.ActiveResearch < -1 || s.ActiveResearch >= AcademyCatalog.Research.Length || s.ResearchProgress < 0 || s.ResearchPoints < 0 || s.Graduates < 0 || s.ExamsHeld < 0 || s.AchievementMask < 0 || s.AchievementMask >= 64) return false;
            if (s.CompletedResearch.Any(k => !Enum.IsDefined(typeof(ResearchKind), k)) || s.CompletedResearch.Distinct().Count() != s.CompletedResearch.Count || s.Clubs.Any(k => !Enum.IsDefined(typeof(ClubKind), k)) || s.Clubs.Distinct().Count() != s.Clubs.Count) return false;
            if (s.ClubLicenses.Any(k => !Enum.IsDefined(typeof(ClubKind), k)) || s.ClubLicenses.Distinct().Count() != s.ClubLicenses.Count || s.Clubs.Any(k => !s.ClubLicenses.Contains(k))) return false;
            if (s.ActiveResearch == -1 ? s.ResearchProgress != 0 : s.CompletedResearch.Contains((ResearchKind)s.ActiveResearch) || s.ResearchProgress >= AcademyCatalog.Research[s.ActiveResearch].Points) return false;
            if (s.Classes.Count != s.Rooms.Count(r => r.Kind == RoomKind.Classroom)) return false;
            var classrooms = new HashSet<int>(); var teachers = new HashSet<int>(); var ids = new HashSet<int>();
            foreach (SchoolClass group in s.Classes)
            {
                if (group == null || !classrooms.Add(group.RoomId) || !s.Rooms.Any(r => r.Id == group.RoomId && r.Kind == RoomKind.Classroom) || string.IsNullOrWhiteSpace(group.Name) || group.Name.Length > 20 || group.Lessons == null || group.Lessons.Count != 6 || group.Lessons.Any(t => !Enum.IsDefined(typeof(Subject), t))) return false;
                if (group.TeacherId != 0 && (!teachers.Add(group.TeacherId) || !s.Staff.Any(p => p.Id == group.TeacherId && p.Role == StaffRole.Teacher))) return false;
            }
            foreach (Pupil p in s.Pupils)
            {
                if (p == null || p.Id < 1 || !ids.Add(p.Id) || string.IsNullOrWhiteSpace(p.Name) || p.Name.Length > 64 || p.Grade < 1 || p.Grade > 6 || p.Ability < 1 || p.Ability > 100 || p.Wellbeing < 0 || p.Wellbeing > 100 || p.Stress < 0 || p.Stress > 100 || p.LastExam < 0 || p.LastExam > 100 || p.LastSupportDay < 0 || p.LastSupportDay > s.Day || !Enum.IsDefined(typeof(Subject), p.Favourite)) return false;
                if (p.ClassRoomId != 0 && !classrooms.Contains(p.ClassRoomId) || p.DaysEnrolled < 0 || p.DaysAttended < 0 || p.DaysAttended > p.DaysEnrolled || p.Scores == null || p.Scores.Count != 5 || p.Scores.Any(n => n < 0 || n > 100)) return false;
            }
            foreach (Room room in s.Rooms)
                if (room.UpgradeLevel < 0 || room.UpgradeLevel > 2 || room.Condition < 20 || room.Condition > 100 || room.Kind == RoomKind.Corridor && room.UpgradeLevel != 0 || room.Kind == RoomKind.Classroom && s.Pupils.Count(p => p.ClassRoomId == room.Id) > 24 + room.UpgradeLevel * 4) return false;
            if (s.Staff.Any(p => p.Energy < 0 || p.Energy > 100 || p.Morale < 0 || p.Morale > 100 || p.TrainingLevel < 0 || p.TrainingLevel > 3)) return false;
            if (s.Reports.Count > 30 || s.Reports.Any(t => t == null || t.Day < 1 || t.Day >= s.Day || t.Year != (t.Day - 1) / 60 + 1 || t.Term != ((t.Day - 1) % 60) / 20 + 1 || t.Day % 20 != 0 || t.Students < 0 || t.Average < 0 || t.Average > 100 || t.Passed < 0 || t.Passed > t.Students || t.Graduated < 0 || t.Repeated < 0 || t.Graduated + t.Repeated > t.Students || t.SubjectAverages == null || t.SubjectAverages.Count != 5 || t.SubjectAverages.Any(n => n < 0 || n > 100))) return false;
            DailyReport d = s.DailyReport;
            if (d.Day < 0 || d.Day >= s.Day || d.Attended < 0 || d.Absent < 0 || d.Unstaffed < 0 || d.Joined < 0 || d.Left < 0 || d.ResearchGained < 0 || d.ResearchFinished < -1 || d.ResearchFinished >= AcademyCatalog.Research.Length || d.TermFinished < 0 || d.TermFinished > 3 || d.NewAchievements < 0 || d.NewAchievements >= 64 || d.StrugglingPupils.Count > 5) return false;
            return s.NextPupilId > (ids.Count == 0 ? 0 : ids.Max());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace KoSch.SchoolTycoon.Core
{
    /// <summary>Pure C# domain. All money and unlock decisions are made here, never by the view.</summary>
    public sealed partial class SchoolSimulation
    {
        public SchoolState State { get; private set; }
        public SchoolSimulation(SchoolState state) { State = state ?? throw new ArgumentNullException("state"); }
        private static int Clamp(int n, int lo, int hi) { return Math.Max(lo, Math.Min(hi, n)); }
        public int Count(RoomKind kind) { return State.Rooms.Count(r => r.Kind == kind); }
        public int Teachers { get { return State.Staff.Count(e => e.Role == StaffRole.Teacher); } }
        public int PhysicalCapacity { get { return State.Rooms.Where(r => r.Kind == RoomKind.Classroom).Sum(RoomSeats); } }
        public int Capacity { get { return State.Classes.Where(c => ClassTeacher(c) != null).Sum(c => RoomSeats(State.Rooms.First(r => r.Id == c.RoomId))); } }
        public int ServedStudents { get { return State.Pupils.Count(p => ClassTeacher(PupilClass(p)) != null); } }
        public bool CanOperate { get { return !State.GameOver && State.PendingEvent < 0; } }

        public static readonly Goal[] Goals = {
            new Goal("Das erste Team", "The first team", "Stelle eine Lehrkraft ein.", "Hire your first teacher.", 1400, s => s.Staff.Any(e => e.Role == StaffRole.Teacher)),
            new Goal("Ein guter Start", "A good start", "Baue einen Sanitärraum und ein Lehrerzimmer.", "Build a restroom and a staff room.", 2200, s => s.Rooms.Any(r => r.Kind == RoomKind.Toilet) && s.Rooms.Any(r => r.Kind == RoomKind.Staffroom)),
            new Goal("Platz zum Wachsen", "Room to grow", "Zwei Klassenzimmer und zwei Lehrkräfte.", "Have two classrooms and two teachers.", 3400, s => s.Rooms.Count(r => r.Kind == RoomKind.Classroom) >= 2 && s.Staff.Count(e => e.Role == StaffRole.Teacher) >= 2),
            new Goal("Mittag & Leselust", "Lunch & literature", "Baue eine Mensa und eine Bibliothek.", "Build a canteen and a library.", 5000, s => s.Rooms.Any(r => r.Kind == RoomKind.Canteen) && s.Rooms.Any(r => r.Kind == RoomKind.Library)),
            new Goal("Lebendiger Campus", "A thriving campus", "48 Schüler und mindestens 65 Zufriedenheit.", "48 students and at least 65 happiness.", 5500, s => s.Students >= 48 && s.Happiness >= 65),
            new Goal("Neugier wecken", "Inspire curiosity", "Labor und Atelier, Lernerfolg mindestens 70.", "A lab and art studio, with learning at least 70.", 6500, s => s.Rooms.Any(r => r.Kind == RoomKind.ScienceLab) && s.Rooms.Any(r => r.Kind == RoomKind.ArtRoom) && s.Learning >= 70),
            new Goal("Eine Schule für alle", "A school for everyone", "96 Schüler, Sporthalle und 75 Ansehen.", "96 students, a gym and 75 reputation.", 9000, s => s.Students >= 96 && s.Reputation >= 75 && s.Rooms.Any(r => r.Kind == RoomKind.Gym))
        };

        public static readonly SchoolEvent[] Events = {
            new SchoolEvent { TitleDe="Tag der offenen Tür", TitleEn="Open campus day", BodyDe="Die Nachbarschaft möchte deine Schule kennenlernen. Wie gestaltest du den Besuch?", BodyEn="Your neighbours want to meet the school. How will you welcome them?", OptionsDe=new[]{"Campusfest · 650 €", "Schülerführungen · 180 €", "Digitaler Rundgang · 0 €"}, OptionsEn=new[]{"Campus festival · €650", "Student tours · €180", "Virtual tour · €0"}, Costs=new[]{650,180,0}, Happiness=new[]{8,3,0}, Reputation=new[]{8,4,1}, Learning=new[]{0,1,0} },
            new SchoolEvent { TitleDe="Der Schulgarten ruft", TitleEn="A greener school", BodyDe="Die Schüler wünschen sich mehr Natur und gemeinsame Projekte.", BodyEn="Students would love more nature and shared projects.", OptionsDe=new[]{"Projektwoche · 500 €", "AG am Nachmittag · 140 €", "Ideenwerkstatt · 0 €"}, OptionsEn=new[]{"Project week · €500", "After-school club · €140", "Ideas workshop · €0"}, Costs=new[]{500,140,0}, Happiness=new[]{6,3,1}, Reputation=new[]{3,1,0}, Learning=new[]{5,2,1} },
            new SchoolEvent { TitleDe="Digitale Lernwerkstatt", TitleEn="Digital learning workshop", BodyDe="Ein lokales Team bietet eine kreative Einführung in digitale Medien an.", BodyEn="A local team offers a creative introduction to digital media.", OptionsDe=new[]{"Workshop für alle · 800 €", "Lehrerfortbildung · 250 €", "Freie Lernmaterialien · 0 €"}, OptionsEn=new[]{"School-wide workshop · €800", "Teacher training · €250", "Free learning resources · €0"}, Costs=new[]{800,250,0}, Happiness=new[]{3,1,0}, Reputation=new[]{4,2,0}, Learning=new[]{9,5,2} },
            Event("Schulinspektion", "School inspection", "Der Besuch steht bevor. Wie bereitet ihr die Schule vor?", "An inspection is approaching. How will you prepare?", new[]{"Ausstattung erneuern · 900 €", "Teamworkshop · 240 €", "Schülervertretung einbeziehen · 0 €"}, new[]{"Renew equipment · €900", "Staff workshop · €240", "Involve the student council · €0"}, new[]{900,240,0}, new[]{4,2,1}, new[]{7,3,1}, new[]{5,3,1}),
            Event("Konflikt im Schulhof", "Playground conflict", "Ein Streit belastet eine Klasse. Welche Unterstützung wählst du?", "A dispute is affecting a class. What support will you choose?", new[]{"Mediation mit Fachkräften · 420 €", "Gemeinsames Klassenprojekt · 120 €", "Klassenrat einberufen · 0 €"}, new[]{"Professional mediation · €420", "Shared class project · €120", "Call a class council · €0"}, new[]{420,120,0}, new[]{8,5,3}, new[]{2,1,0}, new[]{2,2,1}),
            Event("Sicher zur Schule", "A safe journey to school", "Eltern und Kinder möchten den Schulweg verbessern.", "Parents and children want to improve the journey to school.", new[]{"Mobilitätstag · 500 €", "Fahrradtraining · 180 €", "Gemeinsam Wege planen · 0 €"}, new[]{"Mobility day · €500", "Cycling practice · €180", "Plan routes together · €0"}, new[]{500,180,0}, new[]{5,3,1}, new[]{5,2,1}, new[]{3,2,1}),
            Event("Wissenschaftsmesse", "Science fair", "Ein regionaler Wettbewerb lädt eure jungen Forschenden ein.", "A regional competition invites your young researchers.", new[]{"Großer Schulstand · 850 €", "Ein Team entsenden · 220 €", "Projekte online zeigen · 0 €"}, new[]{"Full school exhibition · €850", "Send one team · €220", "Share projects online · €0"}, new[]{850,220,0}, new[]{4,2,1}, new[]{7,3,1}, new[]{8,4,2}),
            Event("Ausflug planen", "Plan a school trip", "Die Klassen wünschen sich Lernen außerhalb des Campus.", "Classes would like to learn beyond the campus.", new[]{"Museumstag · 1000 €", "Naturerkundung · 260 €", "Stadtteil erkunden · 0 €"}, new[]{"Museum day · €1,000", "Nature exploration · €260", "Explore the neighbourhood · €0"}, new[]{1000,260,0}, new[]{7,5,2}, new[]{3,2,1}, new[]{7,4,2}),
            Event("Eltern im Gespräch", "Meet the parents", "Familien wollen mehr über Lernfortschritte und Schulalltag wissen.", "Families want to hear about progress and everyday school life.", new[]{"Beratungstag · 600 €", "Klassengespräche · 160 €", "Offene Sprechstunde · 0 €"}, new[]{"Family support day · €600", "Class meetings · €160", "Open office hours · €0"}, new[]{600,160,0}, new[]{6,3,2}, new[]{6,3,1}, new[]{4,3,1}),
            Event("Warme Tage", "Warm weather", "Es wird warm im Campus. Wie sorgt ihr für angenehme Lernbedingungen?", "The campus is getting warm. How will you keep learning comfortable?", new[]{"Schatten und Trinkstationen · 700 €", "Lernen im Garten · 150 €", "Mehr kurze Pausen · 0 €"}, new[]{"Shade and water stations · €700", "Learn in the garden · €150", "More short breaks · €0"}, new[]{700,150,0}, new[]{7,4,2}, new[]{2,1,0}, new[]{3,2,0}),
            Event("Campus-Sportfest", "Campus sports festival", "Nachbarschulen möchten ein gemeinsames Sportfest veranstalten.", "Neighbouring schools would like to organise a sports festival.", new[]{"Fest ausrichten · 800 €", "Mit einem Team teilnehmen · 200 €", "Bewegungstag auf dem Campus · 0 €"}, new[]{"Host the festival · €800", "Enter one team · €200", "Campus exercise day · €0"}, new[]{800,200,0}, new[]{8,5,3}, new[]{6,3,1}, new[]{4,2,1}),
            Event("Kultur verbindet", "Culture connects", "Die Schüler möchten ihre Sprachen, Geschichten und Traditionen teilen.", "Students want to share their languages, stories and traditions.", new[]{"Kulturwoche · 750 €", "Kreativer Nachmittag · 190 €", "Erzählkreis in den Klassen · 0 €"}, new[]{"Culture week · €750", "Creative afternoon · €190", "Class storytelling circles · €0"}, new[]{750,190,0}, new[]{8,5,3}, new[]{5,2,1}, new[]{7,4,2})
        };

        private static SchoolEvent Event(string de, string en, string bodyDe, string bodyEn, string[] optionsDe, string[] optionsEn, int[] costs, int[] happiness, int[] reputation, int[] learning)
        { return new SchoolEvent { TitleDe = de, TitleEn = en, BodyDe = bodyDe, BodyEn = bodyEn, OptionsDe = optionsDe, OptionsEn = optionsEn, Costs = costs, Happiness = happiness, Reputation = reputation, Learning = learning }; }

        public Result CanBuild(RoomKind kind, int x, int y, bool rotated)
        {
            if (!Enum.IsDefined(typeof(RoomKind), kind)) return Result.Fail("invalid");
            if (!CanOperate) return Result.Fail("blocked");
            var spec = Catalog.Get(kind);
            if (State.Level < spec.UnlockLevel) return Result.Fail("locked");
            if (State.Cash < spec.Cost) return Result.Fail("funds");
            var room = new Room { Kind = kind, X = x, Y = y, Width = rotated ? spec.Height : spec.Width, Height = rotated ? spec.Width : spec.Height };
            foreach (Cell c in room.Cells())
            {
                if (!CampusGrid.IsOwned(State, c)) return Result.Fail("land");
                if (CampusGrid.At(State, c) != null) return Result.Fail("occupied");
            }
            var connected = CampusGrid.Reachable(CampusGrid.Corridors(State));
            Cell door, inside;
            if (!CampusGrid.TryDoor(room, connected, out door, out inside)) return Result.Fail("connection");
            return Result.Ok();
        }
        public Result Build(RoomKind kind, int x, int y, bool rotated = false)
        {
            Result check = CanBuild(kind, x, y, rotated);
            if (!check.Success) return check;
            var spec = Catalog.Get(kind);
            State.Cash -= spec.Cost;
            State.Rooms.Add(new Room { Id = State.NextId++, Kind = kind, X = x, Y = y, Width = rotated ? spec.Height : spec.Width, Height = rotated ? spec.Width : spec.Height });
            ReconcileClasses(kind == RoomKind.Classroom, true); CheckGoals(); return Result.Ok();
        }
        public Result Demolish(int id)
        {
            if (!CanOperate) return Result.Fail("blocked");
            Room room = State.Rooms.FirstOrDefault(r => r.Id == id);
            if (room == null) return Result.Fail("invalid");
            if (id == 1 || id == 2) return Result.Fail("foundation");
            if (room.Kind == RoomKind.Corridor)
            {
                var reachable = CampusGrid.Reachable(CampusGrid.Corridors(State, room));
                foreach (Room remaining in State.Rooms.Where(r => r != room))
                {
                    Cell door, inside;
                    if (remaining.Kind == RoomKind.Corridor ? !remaining.Cells().All(reachable.Contains) : !CampusGrid.TryDoor(remaining, reachable, out door, out inside))
                        return Result.Fail("disconnect");
                }
            }
            State.Rooms.Remove(room); ReconcileClasses(false);
            State.Cash += Catalog.Get(room.Kind).Cost / 2;
            return Result.Ok();
        }

        public List<Employee> Candidates()
        {
            var list = new List<Employee>();
            for (int i = 0; i < 5; i++)
            {
                int seed = (State.Day * 17 + i * 23) % 100;
                list.Add(new Employee { Id = i, Name = Catalog.StaffNames[(State.Day + i) % Catalog.StaffNames.Length], Role = i < 3 ? StaffRole.Teacher : i == 3 ? StaffRole.Caretaker : StaffRole.Counselor, Specialty = (Subject)((State.Day + i) % 5), Skill = 55 + seed % 36, Salary = i < 3 ? 70 + seed / 4 : i == 3 ? 48 : 62 });
            }
            return list;
        }
        public Result Hire(int candidate)
        {
            if (!CanOperate) return Result.Fail("blocked");
            if (candidate < 0 || candidate >= 5) return Result.Fail("invalid");
            Employee person = Candidates()[candidate];
            if (State.Staff.Any(e => e.Name == person.Name)) return Result.Fail("hired");
            int fee = person.Role == StaffRole.Teacher ? 700 : 400;
            if (State.Cash < fee) return Result.Fail("funds");
            State.Cash -= fee; person.Id = State.NextId++; State.Staff.Add(person);
            ReconcileClasses(false);
            if (person.Role == StaffRole.Teacher)
            {
                SchoolClass vacancy = State.Classes.FirstOrDefault(c => c.TeacherId == 0);
                if (vacancy != null) vacancy.TeacherId = person.Id;
            }
            CheckGoals(); return Result.Ok();
        }
        public Result Fire(int id)
        {
            if (!CanOperate) return Result.Fail("blocked");
            var person = State.Staff.FirstOrDefault(e => e.Id == id);
            if (person == null) return Result.Fail("invalid");
            State.Staff.Remove(person); ReconcileClasses(false); return Result.Ok();
        }
        public Result SetLesson(int slot, Subject subject)
        {
            if (!CanOperate) return Result.Fail("blocked");
            if (slot < 0 || slot >= 6 || !Enum.IsDefined(typeof(Subject), subject)) return Result.Fail("invalid");
            State.Timetable[slot] = subject; foreach (SchoolClass group in State.Classes) group.Lessons[slot] = subject; return Result.Ok();
        }
        public Result SetPolicy(int admission, int supplies)
        {
            if (!CanOperate) return Result.Fail("blocked");
            if (admission < 0 || admission > 2 || supplies < 0 || supplies > 2) return Result.Fail("invalid");
            State.EnrollmentPolicy = admission; State.SupplyBudget = supplies; return Result.Ok();
        }
        public Result BuyLand()
        {
            if (!CanOperate) return Result.Fail("blocked");
            if (State.CampusLevel > 0) return Result.Fail("owned");
            if (State.Level < 1) return Result.Fail("locked");
            if (State.Cash < 9000) return Result.Fail("funds");
            State.Cash -= 9000; State.CampusLevel = 1; return Result.Ok();
        }

        public Ledger Forecast()
        {
            return new Ledger {
                Day = State.Day, Students = State.Students,
                Funding = ServedStudents * 28,
                Meals = Math.Min(State.Students, State.Rooms.Where(r => r.Kind == RoomKind.Canteen).Sum(r => 72 + r.UpgradeLevel * 12)) * 3,
                Salaries = State.Staff.Sum(e => e.Salary),
                Maintenance = State.Rooms.Sum(r => (Catalog.Get(r.Kind).DailyCost * (r.Kind == RoomKind.Corridor ? r.Width * r.Height : 1) + r.UpgradeLevel * 8)) * (Researched(ResearchKind.GreenCampus) ? 80 : 100) / 100,
                Supplies = State.Students * (2 + State.SupplyBudget * 2),
                Activities = State.Clubs.Where(ClubAvailable).Sum(k => AcademyCatalog.Clubs[(int)k].Daily),
                Support = State.Students * State.SupportPolicy + (State.MealQuality == 2 ? Math.Min(State.Students, State.Rooms.Where(r => r.Kind == RoomKind.Canteen).Sum(r => 72 + r.UpgradeLevel * 12)) : 0)
            };
        }
        public Result AdvanceDay()
        {
            if (!CanOperate) return Result.Fail("blocked");
            Ledger ledger = Forecast();
            ledger.Net = ledger.Income - ledger.Expenses; State.Cash += ledger.Net;
            int toilet = State.Rooms.Where(r => r.Kind == RoomKind.Toilet).Sum(r => 48 + r.UpgradeLevel * 12);
            int meal = State.Rooms.Where(r => r.Kind == RoomKind.Canteen).Sum(r => 72 + r.UpgradeLevel * 12);
            int cleaning = State.Staff.Where(e => e.Role == StaffRole.Caretaker).Sum(e => 6 + EffectiveSkill(e) / 10 + e.TrainingLevel);
            State.Cleanliness = Clamp(State.Cleanliness - 5 - State.Students / 16 + cleaning, 0, 100);
            int coverage = State.Students == 0 ? 100 : ServedStudents * 100 / State.Students;
            int quality = Teachers == 0 ? 20 : (int)State.Staff.Where(e => e.Role == StaffRole.Teacher).Average(e => EffectiveSkill(e));
            var lessons = State.Classes.SelectMany(c => c.Lessons).ToList();
            int diverse = lessons.Distinct().Count();
            int specialty = State.Classes.Count == 0 ? 0 : (int)State.Classes.Average(c => c.Lessons.Count(sub => ClassTeacher(c) != null && ClassTeacher(c).Specialty == sub));
            int facilities = Count(RoomKind.Library) * 5 + (lessons.Contains(Subject.Science) ? Count(RoomKind.ScienceLab) * 8 : 0) + (lessons.Contains(Subject.Arts) ? Count(RoomKind.ArtRoom) * 6 : 0) + (lessons.Contains(Subject.Sports) ? Count(RoomKind.Gym) * 5 : 0);
            int targetLearning = Clamp(quality / 2 + coverage / 3 + diverse * 2 + specialty + facilities + State.SupplyBudget * 3 + (Count(RoomKind.Staffroom) > 0 ? 4 : 0), 0, 100);
            State.Learning = Clamp((State.Learning * 2 + targetLearning) / 3, 0, 100);
            int targetHappy = 47 + State.Cleanliness / 5 + State.Rooms.Where(r => r.Kind == RoomKind.Garden).Sum(r => 5 + r.UpgradeLevel * 2) + (toilet >= State.Students ? 8 : -18) + (meal >= State.Students ? 8 : -5) + (coverage == 100 ? 8 : -16) + State.Staff.Where(e => e.Role == StaffRole.Counselor).Sum(e => 3 + EffectiveSkill(e) / 20) + ActiveClubCount * 2 + (Researched(ResearchKind.GreenCampus) ? 4 : 0) + (State.MealQuality - 1) * 4;
            State.Happiness = Clamp((State.Happiness * 2 + Clamp(targetHappy, 0, 100)) / 3, 0, 100);
            State.Reputation = Clamp(State.Reputation + (State.Learning >= 65 && State.Happiness >= 60 ? 2 : State.Happiness < 40 ? -2 : 0), 0, 100);
            int graduated = RunAcademyDay();
            int available = Math.Max(0, Capacity - State.Students);
            int applicants = State.Reputation / 15 + (State.Happiness >= 65 ? 2 : 0) + State.EnrollmentPolicy;
            ledger.Joined = State.Happiness >= 45 ? Math.Min(available, applicants) : 0;
            int left = Math.Min(State.Students, State.Happiness < 35 ? 3 : State.Students > ServedStudents ? Math.Min(4, State.Students - ServedStudents) : 0);
            ChangeEnrollment(ledger.Joined, left);
            ledger.Left = left + graduated; State.DailyReport.Joined = ledger.Joined; State.DailyReport.Left = ledger.Left;
            State.LastLedger = ledger; State.History.Add(ledger);
            if (State.History.Count > 60) State.History.RemoveAt(0);
            State.Day++; State.ClockMinute = 480; PrepareAttendance();
            CheckGoals();
            State.DebtDays = State.Cash < 0 ? State.DebtDays + 1 : 0;
            State.GameOver = State.Cash < -5000 || State.DebtDays >= 5;
            if (!State.GameOver && State.Day % 4 == 0) State.PendingEvent = (State.Day / 4 - 1) % Events.Length;
            return Result.Ok();
        }
        public Result ChooseEvent(int option)
        {
            if (State.PendingEvent < 0 || State.PendingEvent >= Events.Length || option < 0 || option >= 3) return Result.Fail("invalid");
            SchoolEvent e = Events[State.PendingEvent];
            if (State.Cash < e.Costs[option] && e.Costs[option] > 0) return Result.Fail("funds");
            State.Cash -= e.Costs[option];
            State.Happiness = Clamp(State.Happiness + e.Happiness[option], 0, 100);
            State.Reputation = Clamp(State.Reputation + e.Reputation[option], 0, 100);
            State.Learning = Clamp(State.Learning + e.Learning[option], 0, 100);
            foreach (Pupil pupil in State.Pupils)
            {
                pupil.Wellbeing = Clamp(pupil.Wellbeing + e.Happiness[option], 0, 100);
                pupil.Stress = Clamp(pupil.Stress - e.Happiness[option] / 2, 0, 100);
                for (int subject = 0; subject < 5; subject++) pupil.Scores[subject] = Clamp(pupil.Scores[subject] + e.Learning[option] / 2, 0, 100);
            }
            State.PendingEvent = -1; PrepareAttendance(); CheckGoals(); return Result.Ok();
        }
        public void CheckGoals()
        {
            while (State.CompletedGoals < Goals.Length && Goals[State.CompletedGoals].IsComplete(State))
            {
                int index = State.CompletedGoals++;
                State.Cash += Goals[index].Reward;
                State.News.Insert(0, "goal:" + index);
                if (State.News.Count > 10) State.News.RemoveAt(State.News.Count - 1);
            }
            State.Level = Math.Min(3, State.CompletedGoals / 2);
        }

        public static bool ValidateSave(SchoolState s) { return ValidateBase(s, 3) && ValidateAcademy(s); }
        private static bool ValidateLegacySave(SchoolState s) { return ValidateBase(s, 2) && s.PendingEvent < 3; }
        private static bool ValidateBase(SchoolState s, int version)
        {
            if (s == null || s.SaveVersion != version || s.Rooms == null || s.Staff == null || s.Timetable == null || s.Director == null || s.History == null || s.LastLedger == null || s.News == null) return false;
            if (s.Day < 1 || s.Day > 100000 || s.Cash < -1000000 || s.Cash > 100000000 || s.Students < 0 || s.Students > 1000 || s.Level < 0 || s.Level > 3 || s.CampusLevel < 0 || s.CampusLevel > 1) return false;
            if (s.NextId < 3 || s.NextId > 1000000) return false;
            if (s.CompletedGoals < 0 || s.CompletedGoals > Goals.Length || s.Level != Math.Min(3, s.CompletedGoals / 2) || s.PendingEvent < -1 || s.PendingEvent >= Events.Length || s.EnrollmentPolicy < 0 || s.EnrollmentPolicy > 2 || s.SupplyBudget < 0 || s.SupplyBudget > 2 || s.ClockMinute < 480 || s.ClockMinute >= 1020) return false;
            if (s.Happiness < 0 || s.Happiness > 100 || s.Learning < 0 || s.Learning > 100 || s.Cleanliness < 0 || s.Cleanliness > 100 || s.Reputation < 0 || s.Reputation > 100 || s.DebtDays < 0) return false;
            if (s.Timetable.Count != 6 || s.Timetable.Any(t => !Enum.IsDefined(typeof(Subject), t)) || s.Rooms.Count > WidthLimit || s.Staff.Count > 200 || s.History.Count > 60) return false;
            if (s.HasDirector && (string.IsNullOrWhiteSpace(s.Director.Name) || s.Director.Name.Length > 24)) return false;
            if (!new[] { "Female", "Male", "Diverse" }.Contains(s.Director.Gender) || !new[] { "Brown", "Black", "Blonde", "Red", "Silver" }.Contains(s.Director.Hair)) return false;
            if (s.History.Any(l => l == null || l.Day < 1 || l.Students < 0) || s.News.Any(n => n == null)) return false;
            var cells = new HashSet<Cell>(); var ids = new HashSet<int>();
            foreach (Room r in s.Rooms)
            {
                if (r == null || r.Id < 1 || !ids.Add(r.Id) || !Enum.IsDefined(typeof(RoomKind), r.Kind)) return false;
                if (r.Id == 1)
                { if (r.Kind != RoomKind.Corridor || r.X != 12 || r.Y != 8 || r.Width != 6 || r.Height != 2) return false; }
                else
                {
                    RoomSpec spec = Catalog.Get(r.Kind);
                    if (!((r.Width == spec.Width && r.Height == spec.Height) || (r.Width == spec.Height && r.Height == spec.Width))) return false;
                }
                foreach (Cell c in r.Cells()) if (!CampusGrid.IsOwned(s, c) || !cells.Add(c)) return false;
            }
            if (!s.Rooms.Any(r => r.Id == 1) || !s.Rooms.Any(r => r.Id == 2 && r.Kind == RoomKind.Classroom)) return false;
            foreach (Employee p in s.Staff)
                if (p == null || p.Id < 3 || !ids.Add(p.Id) || p.Skill < 1 || p.Skill > 100 || p.Salary < 1 || p.Salary > 500 || string.IsNullOrWhiteSpace(p.Name) || !Enum.IsDefined(typeof(StaffRole), p.Role) || !Enum.IsDefined(typeof(Subject), p.Specialty)) return false;
            return s.NextId > ids.Max() && s.Rooms.All(r => CampusGrid.IsConnected(s, r));
        }
        private const int WidthLimit = CampusGrid.Width * CampusGrid.Height;
    }
}

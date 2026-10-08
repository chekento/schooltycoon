using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using KoSch.SchoolTycoon.Core;

namespace KoSch.SchoolTycoon
{
    public sealed class SchoolApp : MonoBehaviour
    {
        public SchoolSimulation Simulation { get; private set; }
        public SchoolState State { get { return Simulation.State; } }
        public CampusRenderer Campus { get; private set; }
        public CampusCamera CameraRig { get; private set; }
        public SchoolUI UI { get; private set; }
        public RoomKind? BuildKind { get; private set; }
        public bool Rotated { get; private set; }
        public bool Demolition { get; private set; }
        public bool Paused = true;
        public int Speed = 1, SelectedRoomId = -1;
        private float minute, uiTick;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (FindFirstObjectByType<SchoolApp>() == null) new GameObject("The School Simulation").AddComponent<SchoolApp>();
        }
        private void Awake()
        {
            Application.targetFrameRate = 60;
            string problem;
            SchoolState state = SchoolSave.Load(out problem) ?? SchoolState.Create();
            if (!state.HasDirector) state.Language = Application.systemLanguage == SystemLanguage.German ? "de" : "en";
            Simulation = new SchoolSimulation(state); minute = state.ClockMinute;
            Campus = new GameObject("Campus").AddComponent<CampusRenderer>(); Campus.Initialize(this);
            CameraRig = new GameObject("Campus Camera").AddComponent<CampusCamera>(); CameraRig.Initialize(this);
            UI = new GameObject("School interface").AddComponent<SchoolUI>(); UI.Initialize(this);
            if (!string.IsNullOrEmpty(problem)) UI.Notify(T("Speicherstand beschädigt. Sicherung oder neue Schule geladen.", "Save damaged. Loaded backup or started a new school."));
        }
        private void Update()
        {
            if (State.HasDirector && !Paused && Simulation.CanOperate && !UI.HasModal)
            {
                minute += Time.deltaTime * 6 * Speed;
                if (minute >= 1020) EndDay();
                else State.ClockMinute = (int)minute;
            }
            uiTick += Time.unscaledDeltaTime;
            if (uiTick > .25f) { UI.RefreshStats(); uiTick = 0; }
        }
        public string T(string de, string en) { return State.Language == "de" ? de : en; }
        public string RoomName(RoomKind kind) { var s = Catalog.Get(kind); return T(s.NameDe, s.NameEn); }
        public string SubjectName(Subject s)
        {
            string[] de = { "Mathematik", "Sprachen", "Naturwissenschaft", "Kunst", "Sport" };
            string[] en = { "Mathematics", "Languages", "Science", "Arts", "Sports" };
            return T(de[(int)s], en[(int)s]);
        }
        public string RoleName(StaffRole r)
        { return r == StaffRole.Teacher ? T("Lehrkraft", "Teacher") : r == StaffRole.Caretaker ? T("Hausdienst", "Caretaker") : T("Schulberatung", "Counselor"); }
        public string ResearchName(ResearchKind kind) { var spec = AcademyCatalog.Research[(int)kind]; return T(spec.NameDe, spec.NameEn); }
        public string ClubName(ClubKind kind) { var spec = AcademyCatalog.Clubs[(int)kind]; return T(spec.NameDe, spec.NameEn); }
        public string SeasonName() { return T(new[] { "Frühling", "Sommer", "Herbst", "Winter" }[Simulation.Season], new[] { "Spring", "Summer", "Autumn", "Winter" }[Simulation.Season]); }
        public string WeatherName() { return T(new[] { "Sonnig", "Bewölkt", "Regen" }[(int)Simulation.Weather], new[] { "Sunny", "Cloudy", "Rainy" }[(int)Simulation.Weather]); }
        public void ChooseBuild(RoomKind? kind, bool demolish = false)
        {
            BuildKind = kind; Demolition = demolish; Rotated = false;
            if (kind.HasValue || demolish) Paused = true;
            Campus.ClearGhost(); UI.Refresh();
        }
        public void Rotate() { Rotated = !Rotated; UI.RefreshStats(); }
        public void TogglePause() { Paused = !Paused; UI.RefreshStats(); }
        public void SetSpeed(int n) { Speed = n; UI.RefreshStats(); }
        public void SelectRoom(Room r) { SelectedRoomId = r == null ? -1 : r.Id; UI.SelectOverview(); }
        public void Act(Result result, bool rebuild = true)
        {
            if (!result.Success) { UI.Notify(Error(result.Code)); return; }
            if (rebuild) Campus.Rebuild();
            UI.Refresh(); Save(false);
        }
        public void BuildAt(Cell cell)
        {
            if (!State.HasDirector || UI.HasModal) return;
            if (BuildKind.HasValue) Act(Simulation.Build(BuildKind.Value, cell.X, cell.Y, Rotated));
            else if (Demolition)
            {
                Room room = CampusGrid.At(State, cell);
                if (room != null) UI.Confirm(T("Raum zurückbauen?", "Demolish room?"), T("Du erhältst 50 % der Baukosten zurück.", "You receive 50% of the construction cost."), () => { Act(Simulation.Demolish(room.Id)); });
            }
            else SelectRoom(CampusGrid.At(State, cell));
        }
        public void EndDay()
        {
            Result r = Simulation.AdvanceDay();
            if (!r.Success) { UI.Notify(Error(r.Code)); return; }
            minute = 480; Campus.Rebuild(); UI.Refresh();
            Ledger l = State.LastLedger;
            UI.Notify(T("Tag ", "Day ") + l.Day + "  ·  " + (l.Net >= 0 ? "+" : "") + l.Net + " €  ·  " + l.Joined + T(" neue Schüler", " new students"));
            Save(false); UI.ShowPending();
            if (!UI.HasModal && (State.DailyReport.TermFinished > 0 || State.DailyReport.ResearchFinished >= 0 || State.DailyReport.NewAchievements > 0)) UI.ShowDayReport();
        }
        public void CompleteDirector(Avatar avatar)
        {
            State.Director = avatar; State.HasDirector = true; Paused = true;
            Campus.Rebuild(); UI.Refresh(); Save(false);
            UI.Notify(T("Willkommen! Stelle zuerst eine Lehrkraft ein. Bau- und Personalmenü sind links und rechts.", "Welcome! Hire a teacher first. Building and staff menus are on the left and right."));
        }
        public void NewSchool()
        {
            SchoolState fresh = SchoolState.Create(); fresh.Language = State.Language;
            Simulation = new SchoolSimulation(fresh); minute = 480; Paused = true;
            BuildKind = null; Demolition = false; SelectedRoomId = -1;
            Campus.Rebuild(); UI.Refresh(); UI.ShowDirector();
        }
        public void ToggleLanguage() { State.Language = State.Language == "de" ? "en" : "de"; UI.Refresh(); Campus.Rebuild(); Save(false); }
        public void Save(bool notify)
        {
            string error;
            if (!State.HasDirector) return;
            if (SchoolSave.Write(State, out error)) { if (notify) UI.Notify(T("Schule gespeichert.", "School saved.")); }
            else UI.Notify(T("Speichern fehlgeschlagen: ", "Save failed: ") + error);
        }
        private void OnApplicationPause(bool pause) { if (pause && Simulation != null && UI != null) Save(false); }
        private void OnApplicationQuit() { if (Simulation != null && UI != null) Save(false); }
        public string Error(string code)
        {
            switch (code)
            {
                case "funds": return T("Dafür reicht das Geld noch nicht.", "You don't have enough funds yet.");
                case "occupied": return T("Hier steht bereits ein Raum.", "This plot is already occupied.");
                case "connection": return T("Der Raum muss an einen verbundenen Flur grenzen.", "Place the room next to a connected corridor.");
                case "land": return T("Dieses Grundstück gehört dir noch nicht.", "You don't own this land yet.");
                case "locked": return T("Erfülle weitere Förderziele, um dies freizuschalten.", "Complete more grant goals to unlock this.");
                case "hired": return T("Diese Person arbeitet schon bei dir.", "This person already works here.");
                case "foundation": return T("Startflur und erstes Klassenzimmer bleiben erhalten.", "The starting corridor and classroom are permanent.");
                case "disconnect": return T("Damit würdest du einen anderen Raum vom Eingang trennen.", "This would disconnect another room from the entrance.");
                case "blocked": return T("Beantworte zuerst das Ereignis oder beginne eine neue Schule.", "Resolve the event or start a new school first.");
                case "owned": return T("Du besitzt bereits den gesamten Campus.", "You already own the whole campus.");
                case "full": return T("Diese Klasse hat keine freien Plätze.", "This class has no free seats.");
                case "maxed": return T("Die höchste Entwicklungsstufe ist erreicht.", "The highest upgrade level has been reached.");
                case "done": return T("Das ist bereits erledigt. Förderung ist einmal pro Schüler und Tag möglich.", "Already completed. Each pupil can receive one intervention per day.");
                case "busy": return T("Schließe zuerst das laufende Forschungsprojekt ab.", "Finish the current research project first.");
                case "researchlocked": return T("Forschung benötigt eine Bibliothek und die angegebene Schulstufe.", "Research needs a library and the stated school level.");
                case "facility": return T("Für diese AG fehlt noch der passende Raum.", "This club needs its matching room first.");
                default: return T("Diese Aktion ist nicht verfügbar.", "This action is unavailable.");
            }
        }
    }

    public static class SchoolSave
    {
        private static string PathName { get { return Path.Combine(Application.persistentDataPath, "school-v3.json"); } }
        public static bool Write(SchoolState s, out string error)
        {
            error = null;
            try
            {
                if (!SchoolSimulation.ValidateSave(s)) throw new InvalidDataException("State validation failed");
                Directory.CreateDirectory(Application.persistentDataPath);
                string temp = PathName + ".tmp";
                File.WriteAllText(temp, JsonUtility.ToJson(s, true));
                if (File.Exists(PathName)) File.Copy(PathName, PathName + ".bak", true);
                // File.Replace is not supported on every Unity target. Rename within the same directory.
                if (File.Exists(PathName)) File.Delete(PathName);
                File.Move(temp, PathName);
                return true;
            }
            catch (Exception e) { error = e.Message; Debug.LogWarning("School save: " + e.Message); return false; }
        }
        public static SchoolState Load(out string error)
        {
            error = null;
            string legacy = Path.Combine(Application.persistentDataPath, "school-v2.json");
            foreach (string path in new[] { PathName, PathName + ".bak", legacy, legacy + ".bak" })
            {
                if (!File.Exists(path)) continue;
                try
                {
                    var info = new FileInfo(path);
                    if (info.Length > 2 * 1024 * 1024) throw new InvalidDataException("Oversized save");
                    SchoolState s = JsonUtility.FromJson<SchoolState>(File.ReadAllText(path));
                    if (SchoolSimulation.MigrateSave(s)) return s;
                    throw new InvalidDataException("Invalid save");
                }
                catch (Exception e) { error = e.Message; Debug.LogWarning("School load: " + e.Message); }
            }
            return null;
        }
    }
}

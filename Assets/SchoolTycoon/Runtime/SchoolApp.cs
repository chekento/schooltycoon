using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using KoSch.SchoolTycoon.Core;
using Avatar = KoSch.SchoolTycoon.Core.Avatar;

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
        public int ViewFloor;
        public bool WorldLabels = true;
        public bool InMainMenu = true;
        public int CurrentSlot;
        public GameSettings Settings { get; private set; }
        public CampusAudio Sound { get; private set; }
        public bool Paused = true;
        public int Speed = 1, SelectedRoomId = -1;
        private float minute, uiTick;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (FindFirstObjectByType<SchoolApp>() == null) new GameObject("Rektor").AddComponent<SchoolApp>();
        }
        private void Awake()
        {
            Settings=GameSettings.Load();string problem;SchoolSave.ImportLegacy(out problem);
            SchoolState state=SchoolState.Create();state.Language=Settings.Language;Simulation=new SchoolSimulation(state);minute=480;
            Campus=new GameObject("Campus").AddComponent<CampusRenderer>();Campus.Initialize(this);
            CameraRig=new GameObject("Campus Camera").AddComponent<CampusCamera>();CameraRig.Initialize(this);
            Sound=new GameObject("Campus sound").AddComponent<CampusAudio>();Sound.Initialize(this);
            Settings.Apply(this);UI=new GameObject("Rektor interface").AddComponent<SchoolUI>();UI.Initialize(this);
            if(!string.IsNullOrEmpty(problem))UI.Notify(T("Ein alter Spielstand konnte nicht importiert werden.","A legacy save could not be imported."));
        }
        private void Update()
        {
            if (!InMainMenu && State.HasDirector && !Paused && Simulation.CanOperate && !UI.HasModal)
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
            SubjectSpec spec=CurriculumCatalog.Get(s,State);return spec==null?"?":T(spec.NameDe,spec.NameEn);
        }
        public void SetFloor(int floor) { ViewFloor=Mathf.Clamp(floor,0,CampusGrid.MaxFloors-1);Campus.ClearGhost();Campus.SetViewFloor();UI.Refresh(); }
        public void ToggleLabels() { WorldLabels=!WorldLabels;Campus.SetViewFloor();UI.Refresh(); }
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
            if (InMainMenu || !State.HasDirector || UI.HasModal) return;
            if (BuildKind.HasValue) Act(Simulation.Build(BuildKind.Value, cell.X, cell.Y, Rotated, cell.Floor));
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
        public void StartNew(int slot,Difficulty difficulty,GameMode mode,string schoolName)
        {
            SchoolState fresh=SchoolSimulation.CreateCampaign(difficulty,mode);fresh.Language=Settings.Language;fresh.SchoolName=string.IsNullOrWhiteSpace(schoolName)?"Campus "+slot:schoolName.Trim().Substring(0,Math.Min(64,schoolName.Trim().Length));
            CurrentSlot=slot;ReplaceState(fresh);UI.ShowDirector();
        }
        public bool LoadSlot(int slot)
        {
            string error;SchoolState state=SchoolSave.Archive.Load(slot,out error);if(state==null){UI.Notify(T("Spielstand konnte nicht geladen werden: ","Could not load save: ")+error);return false;}
            CurrentSlot=slot;state.Language=Settings.Language;ReplaceState(state);if(!state.HasDirector)UI.ShowDirector();else UI.ShowPending();if(error=="backup")UI.Notify(T("Sicherung wiederhergestellt.","Recovered the backup."));return true;
        }
        private void ReplaceState(SchoolState state)
        {
            Simulation=new SchoolSimulation(state);minute=state.ClockMinute;Paused=true;InMainMenu=false;ViewFloor=0;BuildKind=null;Demolition=false;SelectedRoomId=-1;Campus.Rebuild();CameraRig.Home();Settings.Apply(this);UI.Dismiss();UI.Refresh();
        }
        public void NewSchool(){ReturnToMenu();}
        public void ReturnToMenu(){Save(false,true);InMainMenu=true;Paused=true;BuildKind=null;Demolition=false;Campus.ClearGhost();UI.ShowMainMenu();}
        public void ToggleLanguage(){Settings.Language=Settings.Language=="de"?"en":"de";State.Language=Settings.Language;Settings.Save();UI.Refresh();Campus.Rebuild();Save(false);}
        public void Save(bool notify,bool force=false)
        {
            if(CurrentSlot<1||!State.HasDirector||!force&&!notify&&!Settings.Autosave)return;string error;
            if(SchoolSave.Archive.Write(CurrentSlot,State,out error)){if(notify)UI.Notify(T("Campus gespeichert.","Campus saved."));}else UI.Notify(T("Speichern fehlgeschlagen: ","Save failed: ")+error);
        }
        private void OnApplicationPause(bool pause) { if (pause && Simulation != null && UI != null) Save(false,true); }
        private void OnApplicationQuit() { if (Simulation != null && UI != null) Save(false,true); }
        public string Error(string code)
        {
            switch (code)
            {
                case "floor": return T("Sechs Geschosse sind möglich.","Up to six floors are supported.");
                case "outdoors": return T("Gärten benötigen den Erdboden.","Gardens need ground level.");
                case "support": return T("Alle Felder benötigen einen Raum darunter.","Every tile needs a supporting room below.");
                case "supports": return T("Dieser Raum trägt ein Obergeschoss.","This room supports an upper floor.");
                case "stairs": return T("Treppenhäuser müssen exakt übereinander liegen.","Stairwells must align exactly above each other.");
                case "lateyear": return T("Starte das Jahresprojekt spätestens an Tag 51.","Start the annual project by day 51.");
                case "grade": return T("Abschlussjahrgang muss zwischen 6 und 13 liegen und alle Schüler umfassen.","Graduation grade must be 6–13 and include every pupil.");
                case "decorationlast": return T("Kaufe zuerst die nützliche Ausstattung dieses Raums.","Buy the functional equipment in this room first.");
                case "prerequisite": return T("Kaufe zuerst die vorherige Ausbaustufe.","Purchase the previous upgrade first.");
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
        private static SaveArchive archive;
        public static SaveArchive Archive {get{return archive??(archive=new SaveArchive(Application.persistentDataPath,s=>JsonUtility.ToJson(s,true),json=>JsonUtility.FromJson<SchoolState>(json)));}}
        public static void ImportLegacy(out string problem)
        {
            problem=null;if(Archive.HasData(1))return;
            foreach(string file in new[]{"school-v4.json","school-v4.json.bak","school-v3.json","school-v3.json.bak","school-v2.json","school-v2.json.bak"})
            {string path=Path.Combine(Application.persistentDataPath,file);if(!File.Exists(path))continue;try{if(new FileInfo(path).Length>SaveArchive.MaxBytes)throw new InvalidDataException("Oversized save");SchoolState s=JsonUtility.FromJson<SchoolState>(File.ReadAllText(path));if(!SchoolSimulation.MigrateSave(s))throw new InvalidDataException("Invalid school");if(s.HasDirector&&Archive.Write(1,s,out problem))return;}catch(Exception e){problem=e.Message;}}
        }
    }
}

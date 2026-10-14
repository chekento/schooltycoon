using Godot;
using KoSch.SchoolTycoon.Core;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avatar = KoSch.SchoolTycoon.Core.Avatar;

public sealed class Preferences
{
    public string Language = "de";
    public int Graphics = OS.HasFeature("android") ? 0 : 1, Fps = OS.HasFeature("android") ? 30 : 60;
    public int LastSlot;
    public bool ResumeSession;
    public float CameraSpeed = 1, Music = .25f, Effects = .6f;
    public bool ReducedMotion, Labels = true, Autosave = true;
}

[JsonSourceGenerationOptions(IncludeFields = true, IgnoreReadOnlyProperties = true, WriteIndented = true)]
[JsonSerializable(typeof(SchoolState))]
[JsonSerializable(typeof(Preferences))]
internal partial class SaveJson : JsonSerializerContext { }

public partial class RektorApp : Node3D
{
    public SchoolSimulation Sim { get; private set; }
    public SchoolState State => Sim.State;
    public Preferences Settings { get; private set; } = new();
    public SaveArchive Saves { get; private set; }
    public CampusView Campus { get; private set; }
    public RektorUI UI { get; private set; }
    public CampusAudio Audio { get; private set; }
    public RuntimeDiagnostics Diagnostics { get; private set; }
    public bool Menu = true, Paused = true, Rotated, Demolition;
    public int Slot, Floor, Selected = -1, Speed = 1;
    public RoomKind? BuildKind;
    private double minute = 480, uiTick;
    private string saveFolder, settingsPath;
    private bool smoke;

    public override void _Ready()
    {
        smoke = OS.GetCmdlineUserArgs().Contains("--smoke-test");
        bool preview = OS.GetCmdlineUserArgs().Contains("--capture-preview");
        bool touch = OS.GetCmdlineUserArgs().Contains("--touch-check");
        saveFolder = ProjectSettings.GlobalizePath(smoke || preview || touch ? "user://checks/" + Guid.NewGuid().ToString("N") : "user://saves");
        settingsPath = Path.Combine(saveFolder, "settings.json");
        Diagnostics = new RuntimeDiagnostics(Path.Combine(saveFolder, "diagnostics"));
        try { if (File.Exists(settingsPath)) Settings = JsonSerializer.Deserialize(File.ReadAllText(settingsPath), SaveJson.Default.Preferences) ?? new(); }
        catch (Exception e) { GD.PushWarning("Settings recovered to defaults: " + e.Message); }
        NormalizeSettings();
        if (Diagnostics.Interrupted && OS.HasFeature("android")) { Settings.Graphics = 0; Settings.Fps = 30; }
        Saves = new SaveArchive(saveFolder, Encode, Decode);
        Sim = new(SchoolSimulation.CreateCampaign(Difficulty.Standard));
        State.Language = Settings.Language;
        Campus = new CampusView { Name = "Campus" }; AddChild(Campus); Campus.Initialize(this);
        Audio = new CampusAudio { Name = "CampusAudio" }; AddChild(Audio); Audio.Initialize(this);
        UI = new RektorUI { Name = "Interface" }; AddChild(UI); UI.Initialize(this);
        ApplySettings();
        GetTree().AutoAcceptQuit = false;
        Diagnostics.Stage("menu-ready"); GD.Print("REKTOR_READY menu");
        if (smoke) Callable.From(() => FrontendSmoke.Run(this)).CallDeferred();
        else if (preview) Callable.From(() => CapturePreview.Run(this)).CallDeferred();
        else if (touch) Callable.From(() => TouchLifecycleCheck.Run(this)).CallDeferred();
        else if (Settings.ResumeSession && Settings.LastSlot > 0 && !Diagnostics.Interrupted) Callable.From(() => Load(Settings.LastSlot)).CallDeferred();
    }
    public static string Encode(SchoolState s) => JsonSerializer.Serialize(s, SaveJson.Default.SchoolState);
    public static SchoolState Decode(string s) => JsonSerializer.Deserialize(s, SaveJson.Default.SchoolState);
    public string T(string de, string en) => Settings.Language == "de" ? de : en;
    public string RoomName(RoomKind k) { var x = Catalog.Get(k); return T(x.NameDe, x.NameEn); }
    public string SubjectName(Subject k) { var x = CurriculumCatalog.Get(k, State); return x == null ? "?" : T(x.NameDe, x.NameEn); }
    public string RoleName(StaffRole k) => k == StaffRole.Teacher ? T("Lehrkraft", "Teacher") : k == StaffRole.Caretaker ? T("Hausdienst", "Caretaker") : T("Schulberatung", "Counselor");
    public override void _Process(double delta)
    {
        if (!Menu && State.HasDirector && !Paused && Sim.CanOperate && !UI.ModalOpen)
        {
            minute += delta * 6 * Speed;
            if (minute >= 1020) EndDay(); else State.ClockMinute = (int)minute;
        }
        uiTick += delta;
        if (uiTick > .3) { uiTick = 0; UI.RefreshStats(); }
    }
    public void NewCampaign(int slot, Difficulty difficulty, GameMode mode, string name)
    {
        Diagnostics.Stage("new-school");
        Slot = slot;
        var s = SchoolSimulation.CreateCampaign(difficulty, mode);
        s.SchoolName = string.IsNullOrWhiteSpace(name) ? "Campus " + slot : name.Trim()[..Math.Min(64, name.Trim().Length)];
        Replace(s); UI.Director();
    }
    public void CompleteDirector(Avatar director)
    {
        State.Director = director; State.HasDirector = true; Campus.Rebuild(); UI.Hud(); Save(false, true);
        Settings.LastSlot = Slot; Settings.ResumeSession = true; ApplySettings();
        if (!Sim.Sandbox) UI.Story();
    }
    public bool Load(int slot)
    {
        var s = Saves.Load(slot, out string error);
        if (s == null) { UI.Toast(T("Spielstand konnte nicht geladen werden: ", "Could not load save: ") + error); return false; }
        Diagnostics.Stage("load-school");
        Slot = slot; Replace(s);
        Settings.LastSlot = Slot; Settings.ResumeSession = s.HasDirector; ApplySettings();
        if (!s.HasDirector) UI.Director(); else UI.Pending();
        if (error == "backup") UI.Toast(T("Sicherung wiederhergestellt.", "Backup recovered."));
        return true;
    }
    private void Replace(SchoolState s)
    {
        s.Language = Settings.Language; Sim = new(s); Menu = false; Paused = true; Floor = 0;
        minute = s.ClockMinute; BuildKind = null; Demolition = Rotated = false; Selected = -1;
        UI.CloseModal(); Campus.Rebuild(); Campus.Home(); UI.Hud();
    }
    public bool Save(bool notify = false, bool force = false)
    {
        if (Slot < 1 || !State.HasDirector || !force && !notify && !Settings.Autosave) return true;
        bool ok = Saves.Write(Slot, State, out string error);
        if (!ok) GD.PushWarning("SAVE_WRITE_FAIL " + error);
        if (!ok || notify) UI.Toast(ok ? T("Campus gespeichert.", "Campus saved.") : T("Speichern fehlgeschlagen: ", "Save failed: ") + error);
        return ok;
    }
    public void ReturnToMenu()
    {
        if (!Save(false, true)) return;
        Menu = Paused = true; Settings.ResumeSession = false; ApplySettings(); BuildKind = null; Demolition = false; Campus.HideCampus(); UI.MainMenu(); Diagnostics.Stage("menu-ready");
    }
    public void ChooseBuild(RoomKind? kind, bool demolish = false)
    {
        BuildKind = kind; Demolition = demolish; Rotated = false; Paused = true; Campus.ClearGhost(); UI.Hud();
    }
    public void SetFloor(int n)
    {
        Floor = Math.Clamp(n, 0, CampusGrid.MaxFloors - 1); Campus.ClearGhost(); Campus.UpdateVisibility(); UI.Hud();
    }
    public void BuildAt(Cell cell)
    {
        if (Menu || !State.HasDirector || UI.ModalOpen) return;
        if (BuildKind.HasValue) Act(Sim.Build(BuildKind.Value, cell.X, cell.Y, Rotated, Floor));
        else
        {
            Room r = CampusGrid.At(State, cell);
            if (r == null) return;
            if (Demolition) UI.Confirm(T("Raum zurückbauen?", "Demolish room?"), T("50 % Baukosten-Erstattung. Tragende Räume bleiben geschützt.", "50% refund. Supporting rooms remain protected."), () => Act(Sim.Demolish(r.Id)));
            else { Selected = r.Id; UI.ShowManagement(0); }
        }
    }
    public void Act(Result result, bool rebuild = true)
    {
        if (!result.Success) { UI.Toast(Error(result.Code)); return; }
        if (rebuild) Campus.Rebuild(); UI.RefreshCurrent(); Save();
    }
    public void EndDay()
    {
        if (!State.HasDirector) { UI.Director(); return; }
        var result = Sim.AdvanceDay();
        if (!result.Success) { UI.Pending(); UI.Toast(Error(result.Code)); return; }
        minute = 480; Campus.Rebuild(); UI.RefreshCurrent(); Save(); UI.Pending();
        var l = State.LastLedger;
        UI.Toast(T("Tag ", "Day ") + l.Day + " · " + (l.Net >= 0 ? "+" : "") + l.Net + " € · " + l.Joined + T(" neue Schüler", " new pupils"));
    }
    public void ApplySettings()
    {
        NormalizeSettings();
        Engine.MaxFps = Settings.Fps == 30 ? 30 : 60;
        Audio?.Apply(); Campus?.UpdateVisibility();
        try { Directory.CreateDirectory(saveFolder); File.WriteAllText(settingsPath, JsonSerializer.Serialize(Settings, SaveJson.Default.Preferences)); }
        catch (Exception e) { GD.PushWarning("Settings could not be saved: " + e.Message); }
    }
    private void NormalizeSettings()
    {
        Settings.Language = Settings.Language == "en" ? "en" : "de";
        Settings.Graphics = Math.Clamp(Settings.Graphics, 0, 2);
        Settings.Fps = Settings.Fps == 60 ? 60 : 30;
        Settings.LastSlot = Math.Clamp(Settings.LastSlot, 0, SaveArchive.Slots);
        Settings.CameraSpeed = float.IsFinite(Settings.CameraSpeed) ? Math.Clamp(Settings.CameraSpeed, .5f, 2) : 1;
        Settings.Music = float.IsFinite(Settings.Music) ? Math.Clamp(Settings.Music, 0, 1) : .25f;
        Settings.Effects = float.IsFinite(Settings.Effects) ? Math.Clamp(Settings.Effects, 0, 1) : .6f;
    }
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationPaused) { Paused = true; Campus?.HideCampus(); Save(false, true); if (Diagnostics != null) { ApplySettings(); Diagnostics.Clean(); } }
        if (what == NotificationApplicationResumed) { Diagnostics?.Stage(Menu ? "menu-resumed" : "campus-resumed"); if (!Menu && State.HasDirector) Campus?.Rebuild(); }
        if (what == NotificationWMCloseRequest || what == NotificationWMGoBackRequest)
        {
            if (UI == null) return;
            if (UI.ModalOpen) UI.Back();
            else if (!Menu) ReturnToMenu();
            else UI.Confirm(T("Rektor schließen?", "Close Rektor?"), T("Deine Spielstände bleiben erhalten.", "Your saves are preserved."), () => { if (Save(false, true)) { Diagnostics.Clean(); GetTree().Quit(); } });
        }
    }
    public string Error(string code)
    {
        return code switch
        {
            "funds" => T("Noch nicht genug Geld. Prüfe dein Budget.", "Not enough funds. Check your budget."),
            "connection" or "disconnect" => T("Alle Räume benötigen einen Flurweg zum Eingang.", "Every room needs a corridor route to the entrance."),
            "occupied" => T("Hier steht bereits ein Raum.", "A room already occupies this tile."),
            "land" => T("Dieses Grundstück gehört dir noch nicht.", "You do not own this land yet."),
            "support" or "supports" => T("Obergeschosse benötigen tragende Räume darunter.", "Upper floors require supporting rooms below."),
            "stairs" => T("Treppenhäuser müssen exakt übereinander liegen.", "Stairwells must align exactly above each other."),
            "decorationlast" => T("Kaufe zuerst die funktionale Ausstattung.", "Buy the functional equipment first."),
            "prerequisite" => T("Die vorherige Ausbaustufe fehlt noch.", "The previous upgrade is still missing."),
            "locked" or "researchlocked" => T("Noch gesperrt: Voraussetzungen im Menü prüfen.", "Still locked: check the requirements in the menu."),
            "blocked" => T("Beantworte das Ereignis oder starte eine neue Schule.", "Resolve the event or start a new school."),
            "hired" => T("Diese Person arbeitet bereits hier.", "This person already works here."),
            "done" => T("Bereits erledigt. Förderung einmal pro Schüler und Tag.", "Already completed. Support once per pupil and day."),
            "foundation" => T("Das erste Klassenzimmer und der Startflur bleiben erhalten.", "The first classroom and entrance corridor are permanent."),
            "outdoors" => T("Ein Garten braucht den Erdboden.", "Gardens need ground level."),
            "busy" => T("Schließe zuerst das laufende Projekt ab.", "Finish the active project first."),
            "facility" => T("Der passende Fachraum fehlt.", "The matching specialist room is missing."),
            "grade" => T("Abschlussjahrgang 6–13 muss alle Schüler umfassen.", "Graduation grade 6–13 must include every pupil."),
            _ => T("Aktion nicht verfügbar: ", "Action unavailable: ") + code
        };
    }
}

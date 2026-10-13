using Godot;
using KoSch.SchoolTycoon.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avatar = KoSch.SchoolTycoon.Core.Avatar;

public partial class RektorUI : CanvasLayer
{
    private RektorApp app;
    private Control root, safe, screen, modal;
    private Label stats, modeLabel, toast;
    private VBoxContainer modalBody, content;
    private readonly Dictionary<string, Button> buttons = new();
    private int tab, day, directorStep;
    private bool buildOpen, manageOpen;
    private readonly Avatar director = new();
    private double toastTime;
    private static readonly Color Ink = new("31574e"), Muted = new("668278"), Teal = new("598c7b");
    private float W => GetViewport().GetVisibleRect().Size.X;
    private float H => GetViewport().GetVisibleRect().Size.Y;
    private float Tap => OS.HasFeature("android") ? 84 : 60;
    public bool ModalOpen => modal != null;
    public void Initialize(RektorApp owner)
    {
        app = owner;
        var theme = new Theme { DefaultFontSize = OS.HasFeature("android") ? 25 : 22 };
        theme.SetColor("font_color", "Label", Ink);
        theme.SetColor("font_color", "Button", Ink);
        theme.SetStylebox("normal", "Button", Box("e9eee2", 14));
        theme.SetStylebox("hover", "Button", Box("d4e4d6", 14));
        theme.SetStylebox("pressed", "Button", Box("b7d2bf", 14));
        theme.SetStylebox("disabled", "Button", Box("e4e7df", 14));
        theme.SetStylebox("focus", "Button", new StyleBoxFlat { BorderColor = Teal, BorderWidthLeft = 2, BorderWidthTop = 2, BorderWidthRight = 2, BorderWidthBottom = 2, BgColor = new Color(0, 0, 0, 0), CornerRadiusTopLeft = 14, CornerRadiusTopRight = 14, CornerRadiusBottomLeft = 14, CornerRadiusBottomRight = 14 });
        theme.SetStylebox("normal", "LineEdit", Box("eff2e8", 10));
        theme.SetColor("font_color", "LineEdit", Ink);
        root = new Control { Name = "Root", Theme = theme, MouseFilter = Control.MouseFilterEnum.Ignore };
        AddChild(root); Fill(root);
        safe = new Control { Name = "SafeArea", MouseFilter = Control.MouseFilterEnum.Ignore }; root.AddChild(safe); Fill(safe);
        GetViewport().SizeChanged += SafeArea; SafeArea(); MainMenu();
        toast = new Label { Name = "Toast", HorizontalAlignment = HorizontalAlignment.Center, AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = Control.MouseFilterEnum.Ignore, Visible = false };
        root.AddChild(toast); toast.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide);
        toast.OffsetLeft = 32; toast.OffsetRight = -32; toast.OffsetTop = -130; toast.OffsetBottom = -85;
        toast.AddThemeColorOverride("font_color", new Color("fff7e3")); toast.AddThemeColorOverride("font_shadow_color", Ink); toast.AddThemeConstantOverride("shadow_offset_x", 2); toast.AddThemeConstantOverride("shadow_offset_y", 2);
    }
    private void SafeArea()
    {
        if (!OS.HasFeature("android")) return;
        Rect2I area = DisplayServer.GetDisplaySafeArea(); Vector2 window = GetWindow().Size;
        if (window.X <= 0 || window.Y <= 0 || area.Size.X <= 0) return;
        Vector2 scale = GetViewport().GetVisibleRect().Size / window;
        safe.OffsetLeft = area.Position.X * scale.X; safe.OffsetTop = area.Position.Y * scale.Y;
        safe.OffsetRight = -(window.X - area.End.X) * scale.X; safe.OffsetBottom = -(window.Y - area.End.Y) * scale.Y;
    }
    private static void Fill(Control c) => c.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
    private static StyleBoxFlat Box(string hex, int radius = 22) => new()
    {
        BgColor = new Color(hex), CornerRadiusTopLeft = radius, CornerRadiusTopRight = radius, CornerRadiusBottomLeft = radius, CornerRadiusBottomRight = radius,
        ContentMarginLeft = 18, ContentMarginRight = 18, ContentMarginTop = 14, ContentMarginBottom = 14,
        ShadowColor = new Color(.15f, .25f, .2f, .12f), ShadowSize = 8, ShadowOffset = new Vector2(0, 4)
    };
    private PanelContainer Panel(Control parent, string hex = "faf4e6")
    {
        var p = new PanelContainer(); p.AddThemeStyleboxOverride("panel", Box(hex)); parent.AddChild(p); return p;
    }
    private static VBoxContainer Column(Control parent)
    {
        var box = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; box.AddThemeConstantOverride("separation", 12); parent.AddChild(box); return box;
    }
    private static HBoxContainer Row(Control parent)
    {
        var box = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; box.AddThemeConstantOverride("separation", 10); parent.AddChild(box); return box;
    }
    private Label Text(Control parent, string text, int size = 22, bool heading = false)
    {
        var label = new Label { Text = text, AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size); label.AddThemeColorOverride("font_color", heading ? Ink : Muted); parent.AddChild(label); return label;
    }
    private Button Btn(Control parent, string label, Action action, bool primary = false, string id = null)
    {
        var b = new Button { Text = label, CustomMinimumSize = new Vector2(0, Tap), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, AutowrapMode = TextServer.AutowrapMode.WordSmart };
        if (primary) { b.AddThemeStyleboxOverride("normal", Box("598c7b", 14)); b.AddThemeColorOverride("font_color", new Color("fff9ed")); }
        b.Pressed += () => { app.Audio.Click(); action(); }; parent.AddChild(b);
        if (id != null) buttons[id] = b;
        return b;
    }
    private VBoxContainer Scroll(Control parent)
    {
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, SizeFlagsVertical = Control.SizeFlags.ExpandFill, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        parent.AddChild(scroll); return Column(scroll);
    }
    private void Card(Control parent, string title, string body)
    { var v = Column(Panel(parent, "eef0e3")); Text(v, title, 25, true); if (!string.IsNullOrEmpty(body)) Text(v, body, 20); }
    public void TestClick(string id)
    { if (!buttons.TryGetValue(id, out var b) || b.Disabled) throw new InvalidOperationException("Unavailable UI action: " + id); b.EmitSignal(Button.SignalName.Pressed); }
    private void ClearScreen()
    {
        if (screen != null) { safe.RemoveChild(screen); screen.QueueFree(); }
        screen = new Control { Name = "Screen", MouseFilter = Control.MouseFilterEnum.Ignore }; safe.AddChild(screen); Fill(screen);
        stats = modeLabel = null; buttons.Clear();
    }
    public void MainMenu()
    {
        CloseModal(); ClearScreen();
        if (ResourceLoader.Exists("res://Art/CampusWelcome.png"))
        {
            var image = new TextureRect { Texture = GD.Load<Texture2D>("res://Art/CampusWelcome.png"), ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered, MouseFilter = Control.MouseFilterEnum.Ignore };
            screen.AddChild(image); Fill(image);
            var wash = new ColorRect { Color = new Color(.87f, .92f, .84f, .28f), MouseFilter = Control.MouseFilterEnum.Ignore }; screen.AddChild(wash); Fill(wash);
        }
        var p = Panel(screen); p.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.CenterRight);
        float width = Math.Min(620, W - 64), height = Math.Min(800, H - 48);
        p.OffsetLeft = -width - 32; p.OffsetRight = -32; p.OffsetTop = -height / 2; p.OffsetBottom = height / 2;
        var v = Scroll(p);
        Text(v, "REKTOR", 72, true);
        Text(v, app.T("Ein Campus. Tausend Möglichkeiten.", "One campus. A thousand possibilities."), 26);
        Text(v, app.T("Vom ersten Klassenzimmer zur Schule deiner Träume.", "From your first classroom to the school of your dreams."), 22);
        Btn(v, app.T("Kampagne spielen", "Play campaign"), () => Slots(GameMode.Campaign), true, "campaign");
        Btn(v, app.T("Sandbox · unendlich Geld", "Sandbox · unlimited funds"), () => Slots(GameMode.Sandbox), false, "sandbox");
        Btn(v, app.T("Einstellungen", "Settings"), Settings, false, "settings");
        Btn(v, app.T("Spielstand importieren", "Import a save"), ImportDialog);
        Btn(v, app.T("Über Rektor / Engines", "About Rektor / engines"), About);
        Text(v, app.T("Godot 4 · Version 0.6.0\nDas Unity-Projekt bleibt im Repository erhalten.", "Godot 4 · Version 0.6.0\nThe Unity project remains in the repository."), 18);
    }
    private void About()
    {
        OpenModal("Rektor", app.T("Eine liebevolle Schulaufbau-Simulation von KoSch.", "A lovingly crafted school tycoon by KoSch."));
        Text(modalBody, app.T("Godot und Unity nutzen denselben Simulationskern: 146 Fächer, drei Schwierigkeiten, sechs Geschosse, 57 Item-Typen und zwölf Story-Kapitel. Die Godot-Oberfläche ist eine neue Implementierung; die Unity-Oberfläche bleibt separat erhalten.", "Godot and Unity use the same simulation core: 146 subjects, three difficulties, six floors, 57 item types and twelve story chapters. Godot has a new frontend; the Unity frontend remains available."));
        Text(modalBody, "Godot Engine · MIT license\nCopyright (c) 2014-present Godot Engine contributors\nCopyright (c) 2007-2014 Juan Linietsky, Ariel Manzur\nhttps://godotengine.org/license/", 18);
        Btn(modalBody, app.T("Zurück", "Back"), CloseModal);
    }
    public void Slots(GameMode mode)
    {
        OpenModal(app.T("Deine Spielstände", "Your saves"), mode == GameMode.Sandbox ? app.T("Sandbox: Unendlich Geld, aber echte Schulorganisation.", "Sandbox: Unlimited funds, real school organisation.") : app.T("Sechs unabhängige Schulen mit automatischen Sicherungen.", "Six independent schools with automatic backups."));
        for (int slot = 1; slot <= SaveArchive.Slots; slot++)
        {
            int n = slot; var saved = app.Saves.Load(n, out string error);
            string title = n + " · " + (saved == null ? app.T("Freier Platz", "Empty slot") : saved.SchoolName + " · " + app.T("Tag ", "Day ") + saved.Day);
            var v = Column(Panel(modalBody, "eef0e3")); Text(v, title, 23, true);
            if (saved != null)
            {
                Text(v, saved.Mode + " · " + saved.Difficulty + " · " + saved.Students + app.T(" Schüler", " pupils"), 18);
                Btn(v, app.T("Fortsetzen", "Continue"), () => app.Load(n), true, "continue" + n);
                Btn(v, app.T("Neue Schule in diesem Platz", "New school in this slot"), () => Confirm(app.T("Spielstand ersetzen?", "Replace this save?"), title, () => Difficulty(n, mode)));
            }
            else
            {
                if (app.Saves.HasData(n)) Text(v, app.T("Beschädigter Spielstand: ", "Damaged save: ") + error, 18);
                Btn(v, app.T("Neue Schule", "New school"), () => { if (app.Saves.HasData(n)) Confirm(app.T("Beschädigten Spielstand ersetzen?", "Replace damaged save?"), title, () => Difficulty(n, mode)); else Difficulty(n, mode); }, true, "new" + n);
            }
        }
    }
    private void Difficulty(int slot, GameMode mode)
    {
        OpenModal(app.T("Deine neue Schule", "Your new school"), app.T("Wähle die Herausforderung. Bestehende Schulen behalten ihre Schwierigkeit.", "Choose your challenge. Existing schools keep their difficulty."));
        var name = new LineEdit { Text = "Campus " + slot, PlaceholderText = app.T("Name der Schule", "School name"), MaxLength = 64, CustomMinimumSize = new Vector2(0, Tap) }; modalBody.AddChild(name);
        foreach (var difficulty in RektorCatalog.Difficulties)
        {
            var d = difficulty;
            Card(modalBody, app.T(d.NameDe, d.NameEn), mode == GameMode.Sandbox ? app.T("Unendlich Geld. Wege, Klassenplätze und Lehrkräfte bleiben wichtig.", "Unlimited funds. Paths, seats and teachers still matter.") : d.StartCash + app.T(" € Startkapital · ", " starting funds · ") + d.PricePercent + app.T(" % Kaufpreise\n", "% purchase prices\n") + d.UpkeepPercent + app.T(" % Betriebskosten · ", "% running costs · ") + d.DebtGrace + app.T(" Tage Schuldentoleranz", " days of debt tolerance"));
            Btn(modalBody, app.T(d.NameDe, d.NameEn), () => { buildOpen = manageOpen = false; app.NewCampaign(slot, d.Id, mode, name.Text); }, true, "difficulty" + (int)d.Id);
        }
    }
    public void Director(int step = 0)
    {
        directorStep = step;
        OpenModal(app.T("Deine Schulleitung", "Your school director"), app.T("Schritt ", "Step ") + (step + 1) + "/3");
        if (step == 0)
        {
            string[] ids = { "Female", "Male", "Diverse" }, labels = { app.T("Weiblich", "Female"), app.T("Männlich", "Male"), app.T("Divers", "Diverse") };
            for (int i = 0; i < 3; i++) { int n = i; Btn(modalBody, labels[i], () => { director.Gender = ids[n]; Director(1); }, true, "gender" + i); }
        }
        else if (step == 1)
        {
            string[] ids = { "Brown", "Black", "Blonde", "Red", "Silver" }, labels = { app.T("Braun", "Brown"), app.T("Schwarz", "Black"), app.T("Blond", "Blonde"), app.T("Rot", "Red"), app.T("Silber", "Silver") };
            for (int i = 0; i < ids.Length; i++) { int n = i; Btn(modalBody, labels[i], () => { director.Hair = ids[n]; Director(2); }, true, "hair" + i); }
        }
        else
        {
            Text(modalBody, app.T("Wie soll deine Schulleitung heißen?", "What is your director's name?"), 27, true);
            var name = new LineEdit { Text = director.Name, MaxLength = 24, CustomMinimumSize = new Vector2(0, Tap) }; modalBody.AddChild(name);
            Btn(modalBody, app.T("Willkommen auf dem Campus", "Welcome to your campus"), () => { if (string.IsNullOrWhiteSpace(name.Text)) { Toast(app.T("Bitte einen Namen eingeben.", "Please enter a name.")); return; } CloseModal(); app.CompleteDirector(new Avatar { Name = name.Text.Trim(), Gender = director.Gender, Hair = director.Hair }); }, true, "directorDone");
        }
        if (step > 0) Btn(modalBody, app.T("Zurück", "Back"), () => Director(step - 1));
    }
    public void Hud()
    {
        ClearScreen();
        var header = Panel(screen); header.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.TopWide); header.OffsetLeft = 18; header.OffsetRight = -18; header.OffsetTop = 12; header.OffsetBottom = 106;
        var row = Row(header); stats = Text(row, "", OS.HasFeature("android") ? 22 : 21, true);
        var menu = Btn(row, app.T("Menü", "Menu"), app.ReturnToMenu); menu.SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        var bottom = Panel(screen); bottom.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomWide); bottom.OffsetLeft = 18; bottom.OffsetRight = -18; bottom.OffsetTop = -110; bottom.OffsetBottom = -12;
        var controls = Row(bottom);
        Btn(controls, app.T("Bauen", "Build"), () => { buildOpen = !buildOpen; manageOpen = false; Hud(); }, buildOpen, "build");
        Btn(controls, app.T("Verwalten", "Manage"), () => ShowManagement(tab), manageOpen, "manage");
        Btn(controls, app.Paused ? app.T("Start", "Play") : app.T("Pause", "Pause"), () => { app.Paused = !app.Paused; Hud(); }, true, "pause");
        Btn(controls, app.T("Tag beenden", "End day"), app.EndDay, false, "endDay");
        Btn(controls, app.T("Ansicht", "View"), CameraOptions);
        Btn(controls, app.T("Speichern", "Save"), () => app.Save(true));
        if (buildOpen || manageOpen)
        {
            var panel = Panel(screen); panel.AnchorLeft = 0; panel.AnchorRight = 0; panel.AnchorTop = 0; panel.AnchorBottom = 1;
            panel.OffsetLeft = 18; panel.OffsetRight = Math.Min(650, W * .48f); panel.OffsetTop = 120; panel.OffsetBottom = -124;
            var v = Column(panel);
            var top = Row(v); Text(top, buildOpen ? app.T("DEINE SCHULE", "YOUR SCHOOL") : app.T("CAMPUS VERWALTEN", "MANAGE CAMPUS"), 24, true);
            Btn(top, "×", () => { buildOpen = manageOpen = false; Hud(); }).SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
            if (manageOpen)
            {
                var tabs = new OptionButton { CustomMinimumSize = new Vector2(0, Tap), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
                string[] names = { app.T("Schule & Räume", "School & rooms"), app.T("Team", "Staff"), app.T("Klassen & Stundenpläne", "Classes & timetables"), app.T("Budget", "Budget"), app.T("Schüler", "Pupils"), app.T("Forschung", "Research"), app.T("Arbeitsgemeinschaften", "Clubs"), app.T("Schuljahr & Projekte", "School year & projects") };
                foreach (string n in names) tabs.AddItem(n); tabs.Selected = tab; tabs.ItemSelected += n => { tab = (int)n; Hud(); }; v.AddChild(tabs);
            }
            content = Scroll(v);
            if (buildOpen) BuildPalette(); else switch (tab) { case 0: Overview(); break; case 1: Staff(); break; case 2: Classes(); break; case 3: Budget(); break; case 4: Pupils(); break; case 5: Research(); break; case 6: Clubs(); break; default: Year(); break; }
        }
        modeLabel = Text(screen, "", 20, true); modeLabel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.BottomRight);
        modeLabel.OffsetLeft = -440; modeLabel.OffsetRight = -28; modeLabel.OffsetTop = -164; modeLabel.OffsetBottom = -118; modeLabel.HorizontalAlignment = HorizontalAlignment.Right;
        RefreshStats(); if (modal != null) safe.MoveChild(modal, -1);
    }
    public void ShowManagement(int n) { tab = n; manageOpen = true; buildOpen = false; Hud(); }
    public void RefreshCurrent() { if (!app.Menu) Hud(); }
    public void RefreshStats()
    {
        if (stats == null) return;
        var s = app.State; var sim = app.Sim;
        stats.Text = "REKTOR  ·  " + (sim.Sandbox ? "∞" : s.Cash.ToString("N0")) + " €   ·   " + s.Students + "/" + sim.Capacity + app.T(" Schüler", " pupils") + "\n" + app.T("Lernen ", "Learning ") + s.Learning + "%  ·  " + app.T("Freude ", "Joy ") + s.Happiness + "%  ·  " + app.T("Jahr ", "Year ") + sim.SchoolYear + " / " + app.T("Tag ", "Day ") + sim.YearDay + "  ·  " + s.ClockMinute / 60 + ":" + (s.ClockMinute % 60).ToString("00");
        modeLabel.Text = app.BuildKind.HasValue ? app.RoomName(app.BuildKind.Value) + (app.Rotated ? " ↻" : "") : app.Demolition ? app.T("Rückbau: Raum antippen", "Demolish: tap a room") : app.T("Ziehen: Kamera · Tippen: Raum · Pinch: Zoom", "Drag: camera · Tap: room · Pinch: zoom");
    }
    private void CameraOptions()
    {
        OpenModal(app.T("Campus-Ansicht", "Campus view"), app.T("Sechs Geschosse. Treppen und tragende Räume verbinden deinen Campus.", "Six floors. Stairs and supporting rooms connect your campus."));
        var floor = Row(modalBody); Btn(floor, "−", () => { app.SetFloor(app.Floor - 1); CloseModal(); }); Text(floor, app.Floor == 0 ? app.T("Erdgeschoss", "Ground floor") : "OG " + app.Floor); Btn(floor, "+", () => { app.SetFloor(app.Floor + 1); CloseModal(); });
        var zoom = Row(modalBody); Btn(zoom, "Zoom −", () => app.Campus.Zoom(3)); Btn(zoom, "Zoom +", () => app.Campus.Zoom(-3));
        Btn(modalBody, app.T("Ansicht drehen", "Rotate view"), app.Campus.Orbit);
        Btn(modalBody, app.T("Kamera zurücksetzen", "Reset camera"), app.Campus.Home);
        Btn(modalBody, app.T("Geschwindigkeit: ", "Speed: ") + app.Speed + "×", () => { app.Speed = app.Speed == 1 ? 3 : 1; CloseModal(); Hud(); });
        Btn(modalBody, app.T("Zurück", "Back"), CloseModal);
    }
    private void BuildPalette()
    {
        Text(content, app.T("Tippe auf eine freie Fläche. Neue Räume brauchen einen verbundenen Flur; obere Etagen einen Raum darunter.", "Tap an empty plot. New rooms need corridor access; upper floors need support below."), 20);
        var row = Row(content); Btn(row, app.T("Erkunden", "Explore"), () => app.ChooseBuild(null)); Btn(row, app.T("Bauplan ↻", "Footprint ↻"), () => { app.Rotated = !app.Rotated; app.Campus.ClearGhost(); RefreshStats(); });
        var floors = Row(content); Btn(floors, "−", () => app.SetFloor(app.Floor - 1)); Text(floors, app.Floor == 0 ? "EG" : "OG " + app.Floor, 26, true); Btn(floors, "+", () => app.SetFloor(app.Floor + 1));
        foreach (var spec in Catalog.Rooms)
        {
            RoomKind kind = spec.Kind; bool locked = !app.Sim.RoomUnlocked(kind, app.Floor);
            var b = Btn(content, app.RoomName(kind) + "\n" + app.Sim.BuildCost(kind, app.Floor) + " € · " + spec.Width + "×" + spec.Height + (locked ? app.T(" · gesperrt", " · locked") : ""), () => app.ChooseBuild(kind), app.BuildKind == kind, "room" + (int)kind);
            b.Disabled = locked || !app.Sim.CanOperate;
            if (locked) Text(content, app.Sim.RoomRequirement(kind, app.Floor), 18);
        }
        Btn(content, app.T("Rückbau · 50 % Erstattung", "Demolish · 50% refund"), () => app.ChooseBuild(null, true));
        Btn(content, app.T("Grundstück erweitern · ", "Expand land · ") + app.Sim.Price(9000) + " €", () => app.Act(app.Sim.BuyLand()));
    }
    private void Guide(Control parent)
    {
        if (app.Sim.Sandbox) { Card(parent, "SANDBOX", app.T("Gestalte frei. Unendlich Geld ersetzt weder Plätze noch Unterricht.", "Build freely. Unlimited money does not replace seats or teaching.")); return; }
        int chapter = Math.Min(11, app.State.StoryChapter);
        Card(parent, "MARA · " + (app.State.StoryChapter < 12 ? (chapter + 1) + "/12" : "✓"), app.T(SchoolSimulation.StoryDe[chapter], SchoolSimulation.StoryEn[chapter]) + "\n" + app.Sim.StoryAdvice());
    }
    private void Overview()
    {
        Guide(content);
        Text(content, app.T("Ansehen ", "Reputation ") + app.State.Reputation + "% · " + app.T("Sauberkeit ", "Cleanliness ") + app.State.Cleanliness + "%");
        if (app.State.CompletedGoals < SchoolSimulation.Goals.Length) { var goal = SchoolSimulation.Goals[app.State.CompletedGoals]; Card(content, app.T(goal.TitleDe, goal.TitleEn), app.T(goal.DetailDe, goal.DetailEn) + "\n" + goal.Reward + " €"); }
        var room = app.State.Rooms.FirstOrDefault(r => r.Id == app.Selected);
        if (room == null) Text(content, app.T("Tippe einen Raum an, um Ausstattung nachzukaufen.", "Tap a room to buy additional equipment."));
        else
        {
            Card(content, app.RoomName(room.Kind) + " · " + app.T("Stufe ", "Tier ") + app.Sim.RoomTier(room) + "/5", app.T("Zustand ", "Condition ") + room.Condition + "%\n" + app.T("Lernbonus ", "Learning bonus ") + app.Sim.RoomLearning(room) + " · " + app.T("Komfort ", "Comfort ") + app.Sim.RoomComfort(room));
            Btn(content, app.T("Reparieren · ", "Repair · ") + app.Sim.RepairCost(room) + " €", () => app.Act(app.Sim.RepairRoom(room.Id)));
            if (room.FurnishingVersion == 0) Btn(content, app.T("Legacy-Raum ausbauen · ", "Upgrade legacy room · ") + app.Sim.UpgradeCost(room) + " €", () => app.Act(app.Sim.UpgradeRoom(room.Id))).Disabled = room.UpgradeLevel >= 3;
            foreach (var spec in RektorCatalog.For(room.Kind))
            {
                var item = spec; bool owned = room.Items.Contains(item.Id); var check = app.Sim.CanBuyItem(room, item.Id);
                var b = Btn(content, (owned ? "✓ " : "") + app.T(item.NameDe, item.NameEn) + "\n" + app.Sim.ItemCost(item.Id) + " € · " + item.Daily + app.T(" €/Tag", " €/day"), () => app.Act(app.Sim.BuyRoomItem(room.Id, item.Id)), !item.Decoration);
                b.Disabled = owned || !check.Success;
                if (!owned && !check.Success) Text(content, check.Code == "locked" ? app.T("Ab Tag ", "From day ") + item.Day + (item.Research.HasValue ? " · " + app.T("Forschung erforderlich", "research required") : "") : app.Error(check.Code), 18);
            }
        }
        foreach (var r in app.State.Rooms.Where(r => !CampusGrid.Walkway(r))) { int id = r.Id; Btn(content, app.RoomName(r.Kind) + " · " + (r.Floor == 0 ? "EG" : "OG " + r.Floor) + " · #" + r.Id, () => { app.Selected = id; Hud(); }); }
    }
    private void Staff()
    {
        Card(content, app.T("Ein Team für deinen Campus", "A team for your campus"), app.T("Lehrkräfte betreuen je eine Klasse. Hausdienst und Beratung stärken die gesamte Schule.", "Teachers each staff one class. Caretakers and counselors support the entire school."));
        foreach (var employee in app.State.Staff)
        {
            var e = employee; Card(content, e.Name, app.RoleName(e.Role) + " · " + app.Sim.StaffDailySalary(e) + app.T(" €/Tag\n", " €/day\n") + app.T("Kompetenz ", "Skill ") + app.Sim.EffectiveSkill(e) + "% · " + app.T("Energie ", "Energy ") + e.Energy + "% · " + app.T("Teamgefühl ", "Morale ") + e.Morale + "%");
            Btn(content, app.T("Fortbildung · ", "Training · ") + app.Sim.TrainingCost(e) + " €", () => app.Act(app.Sim.TrainStaff(e.Id), false)).Disabled = e.TrainingLevel >= 3;
            Btn(content, app.T("Freistellen", "Release"), () => Confirm(app.T("Teammitglied freistellen?", "Release staff?"), e.Name, () => app.Act(app.Sim.Fire(e.Id))));
        }
        Text(content, app.T("BEWERBUNGEN DES TAGES", "TODAY'S APPLICANTS"), 24, true);
        foreach (var candidate in app.Sim.Candidates())
        {
            var c = candidate; Card(content, c.Name, app.RoleName(c.Role) + " · " + app.Sim.StaffDailySalary(c) + app.T(" €/Tag · ", " €/day · ") + c.Skill + "%\n" + app.SubjectName(c.Specialty));
            Btn(content, app.T("Einstellen · ", "Hire · ") + app.Sim.Price(c.Role == StaffRole.Teacher ? 700 : 400) + " €", () => app.Act(app.Sim.Hire(c.Id)), true, "hire" + c.Id).Disabled = app.State.Staff.Any(e => e.Name == c.Name);
        }
    }
    private void Budget()
    {
        var f = app.Sim.Forecast();
        Card(content, app.T("Tagesprognose", "Daily forecast"), app.T("Bauen und Einstellen: einmalig. Die folgenden Kosten fallen jeden Schultag an.", "Construction and hiring: one-off. The costs below recur each school day."));
        Text(content, app.T("Förderung +", "Funding +") + f.Funding + " €\n" + app.T("Mensa +", "Canteen +") + f.Meals + " €\n" + app.T("Gehälter −", "Salaries −") + f.Salaries + " €\n" + app.T("Betrieb −", "Maintenance −") + f.Maintenance + " €\n" + app.T("Lernmaterial −", "Supplies −") + f.Supplies + " €\n" + app.T("Kursmaterial −", "Course materials −") + f.Curriculum + " €\n" + app.T("AGs −", "Clubs −") + f.Activities + " €\n" + app.T("Förderung / Essen −", "Support / meals −") + f.Support + " €");
        Text(content, app.T("Bilanz: ", "Balance: ") + (f.Income - f.Expenses) + " €", 30, true);
        string[] levels = { app.T("Sparsam", "Basic"), app.T("Ausgewogen", "Balanced"), app.T("Großzügig", "Generous") };
        Btn(content, app.T("Lernmaterial: ", "Supplies: ") + levels[app.State.SupplyBudget], () => app.Act(app.Sim.SetPolicy(app.State.EnrollmentPolicy, (app.State.SupplyBudget + 1) % 3), false));
        Btn(content, app.T("Aufnahmetempo: ", "Admissions: ") + app.State.EnrollmentPolicy + "/2", () => app.Act(app.Sim.SetPolicy((app.State.EnrollmentPolicy + 1) % 3, app.State.SupplyBudget), false));
        Btn(content, app.T("Förderung: ", "Support: ") + levels[app.State.SupportPolicy], () => app.Act(app.Sim.SetWellbeingPolicy((app.State.SupportPolicy + 1) % 3, app.State.MealQuality), false));
        Btn(content, app.T("Mittagessen: ", "Lunch: ") + levels[app.State.MealQuality], () => app.Act(app.Sim.SetWellbeingPolicy(app.State.SupportPolicy, (app.State.MealQuality + 1) % 3), false));
        Text(content, app.T("Schuldentoleranz: ", "Debt tolerance: ") + app.Sim.Rules.DebtGrace + app.T(" Tage · Grenze −", " days · limit −") + app.Sim.Rules.DebtLimit + " €", 20);
        if (app.State.Cash < 0) Btn(content, app.T("Überbrückungshilfe prüfen", "Check emergency funding"), () => app.Act(app.Sim.FinancialBridge(), false));
        foreach (var l in app.State.History.AsEnumerable().Reverse().Take(10)) Text(content, app.T("Tag ", "Day ") + l.Day + " · " + l.Net + " € · " + l.Students + app.T(" Schüler", " pupils"), 18);
    }
    private void Classes()
    {
        var regions = Row(content);
        foreach (CurriculumRegion region in new[] { CurriculumRegion.Germany, CurriculumRegion.England, CurriculumRegion.USA }) { var r = region; Btn(regions, region == CurriculumRegion.Germany ? "DE" : region == CurriculumRegion.England ? "UK" : "US", () => app.Act(app.Sim.ApplyCurriculum(r), false)); }
        Text(content, app.T("146 Fächer. Jede Klasse besitzt 30 Wochenstunden. Fachräume können nicht gleichzeitig zwei Klassen bedienen.", "146 subjects. Every class has 30 weekly lessons. A specialist room cannot serve two classes simultaneously."), 20);
        Btn(content, app.T("Eigenes Wahlfach erstellen", "Create an elective"), CustomSubject);
        foreach (var group in app.State.Classes)
        {
            var c = group; var room = app.State.Rooms.First(r => r.Id == c.RoomId); var teacher = app.Sim.ClassTeacher(c);
            Card(content, c.Name, app.Sim.ClassSize(c) + "/" + app.Sim.RoomSeats(room) + " · " + (teacher?.Name ?? app.T("Lehrkraft fehlt", "No teacher")));
            Btn(content, app.T("Lehrkraft zuordnen", "Assign a teacher"), () => ChooseTeacher(c));
            Btn(content, app.T("Wochenplan bearbeiten", "Edit weekly timetable"), () => Timetable(c));
            Btn(content, app.T("Klassenname ändern", "Rename class"), () => { OpenModal(app.T("Klassenname", "Class name"), ""); var name = Entry(modalBody, c.Name, 28); Btn(modalBody, app.T("Übernehmen", "Apply"), () => { var r = app.Sim.SetClassName(c.RoomId, name.Text); if (r.Success) CloseModal(); app.Act(r, false); }); });
            Btn(content, app.T("Kursniveau: ", "Course level: ") + c.CourseLevel, () => app.Act(app.Sim.SetCourseLevel(c.RoomId, c.CourseLevel % 3 + 1), false));
        }
    }
    private LineEdit Entry(Control parent, string text, int max = 60)
    { var e = new LineEdit { Text = text, MaxLength = max, CustomMinimumSize = new Vector2(0, Tap), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill }; parent.AddChild(e); return e; }
    private void ChooseTeacher(SchoolClass c)
    {
        OpenModal(c.Name, app.T("Eine Lehrkraft kann nur eine Klasse betreuen. Zuordnung verschiebt die Lehrkraft aus der bisherigen Klasse.", "A teacher can staff only one class. Assignment moves them from their previous class."));
        foreach (var teacher in app.State.Staff.Where(t => t.Role == StaffRole.Teacher)) { var t = teacher; Btn(modalBody, t.Name + " · " + app.Sim.EffectiveSkill(t) + "%", () => { CloseModal(); app.Act(app.Sim.AssignTeacher(c.RoomId, t.Id)); }, true); }
        Btn(modalBody, app.T("Ohne Lehrkraft", "No teacher"), () => { CloseModal(); app.Act(app.Sim.AssignTeacher(c.RoomId, 0)); });
    }
    private void Timetable(SchoolClass c)
    {
        OpenModal(c.Name + " · " + app.T("Wochenplan", "Weekly timetable"), app.T("Die Freischaltung eines Fachs hängt von deiner Schulentwicklung ab.", "Subject unlocks depend on your school's development."));
        var days = Row(modalBody); string[] names = app.Settings.Language == "de" ? new[] { "Mo", "Di", "Mi", "Do", "Fr" } : new[] { "Mon", "Tue", "Wed", "Thu", "Fri" };
        for (int i = 0; i < 5; i++) { int d = i; Btn(days, names[i], () => { day = d; Timetable(c); }, i == day); }
        for (int slot = 0; slot < 6; slot++) { int n = slot; Btn(modalBody, (n + 1) + " · " + app.SubjectName(c.Week[day * 6 + n]), () => SubjectPicker(c, n)); }
        Btn(modalBody, app.T("Fertig", "Done"), CloseModal);
    }
    private void SubjectPicker(SchoolClass c, int slot)
    {
        OpenModal(app.T("Fach auswählen", "Choose a subject"), app.T("Alle integrierten und eigenen Fächer sind durchsuchbar.", "Search every built-in and custom subject."));
        var search = Entry(modalBody, ""); search.PlaceholderText = app.T("Fach suchen …", "Search subjects …");
        var results = Column(modalBody);
        void Filter(string query)
        {
            foreach (var child in results.GetChildren()) { results.RemoveChild(child); child.QueueFree(); }
            foreach (var subject in CurriculumCatalog.All(app.State).Where(s => app.T(s.NameDe, s.NameEn).Contains(query, StringComparison.OrdinalIgnoreCase)).OrderBy(s => app.T(s.NameDe, s.NameEn)))
            {
                var s = subject; bool unlocked = app.Sim.SubjectUnlocked(s.Id);
                var b = Btn(results, app.T(s.NameDe, s.NameEn) + "\n" + app.RoomName(s.Facility) + " · " + app.T("Niveau ", "Level ") + s.Difficulty + (unlocked ? "" : app.T(" · gesperrt", " · locked")), () => { var r = app.Sim.SetWeeklyLesson(c.RoomId, day, slot, s.Id); if (r.Success) { app.Act(r, false); Timetable(c); } else Toast(app.Error(r.Code)); }, false);
                b.Disabled = !unlocked;
            }
        }
        search.TextChanged += Filter; Filter("");
    }
    private void CustomSubject()
    {
        OpenModal(app.T("Neues Wahlfach", "New elective"), app.T("Bis zu 200 eigene Fächer ergänzen die integrierten Angebote.", "Add up to 200 electives to the built-in subjects."));
        Text(modalBody, app.T("Deutscher Name", "German name")); var de = Entry(modalBody, "");
        Text(modalBody, app.T("Englischer Name", "English name")); var en = Entry(modalBody, "");
        var groups = new OptionButton { CustomMinimumSize = new Vector2(0, Tap) }; foreach (SubjectGroup g in Enum.GetValues<SubjectGroup>()) groups.AddItem(g.ToString()); modalBody.AddChild(groups);
        var rooms = new OptionButton { CustomMinimumSize = new Vector2(0, Tap) }; var kinds = Catalog.Rooms.Where(r => r.Kind != RoomKind.Corridor && r.Kind != RoomKind.Staircase).Select(r => r.Kind).ToArray(); foreach (var r in kinds) rooms.AddItem(app.RoomName(r)); modalBody.AddChild(rooms);
        var levels = new OptionButton { CustomMinimumSize = new Vector2(0, Tap) }; foreach (string n in new[] { "1", "2", "3" }) levels.AddItem(n); modalBody.AddChild(levels);
        Btn(modalBody, app.T("Wahlfach hinzufügen", "Add elective"), () => { var r = app.Sim.AddSubject(de.Text, en.Text, (SubjectGroup)groups.Selected, kinds[rooms.Selected], levels.Selected + 1); if (r.Success) CloseModal(); app.Act(r, false); }, true);
    }
    private void Pupils()
    {
        Text(content, app.T("Individuelle Lernfortschritte, Wohlbefinden und Förderung.", "Individual learning progress, wellbeing and support."));
        foreach (var p in app.State.Pupils.OrderByDescending(app.Sim.NeedsSupport).ThenBy(p => p.Name)) { int id = p.Id; Btn(content, p.Name + " · " + app.T("Jg. ", "Grade ") + p.Grade + "\n" + app.Sim.PupilAverage(p) + "% · " + app.T("Stress ", "Stress ") + p.Stress + "%" + (p.Absent ? app.T(" · abwesend", " · absent") : ""), () => Pupil(id)); }
    }
    private void Pupil(int id)
    {
        var p = app.State.Pupils.FirstOrDefault(p => p.Id == id); if (p == null) return;
        OpenModal(p.Name, app.T("Wohlbefinden ", "Wellbeing ") + p.Wellbeing + "% · " + app.T("Stress ", "Stress ") + p.Stress + "%\n" + app.T("Anwesenheit ", "Attendance ") + p.DaysAttended + "/" + p.DaysEnrolled);
        foreach (var s in app.Sim.PupilCourses(p)) Text(modalBody, app.SubjectName(s) + " · " + app.Sim.Score(p, s) + "%");
        string[] labels = { app.T("Förderunterricht · ", "Tutoring · ") + app.Sim.Price(90) + " €", app.T("Beratung · ", "Counselling · ") + app.Sim.Price(60) + " €", app.T("Persönliches Gespräch · 0 €", "Personal conversation · €0") };
        for (int i = 0; i < 3; i++) { int n = i; Btn(modalBody, labels[i], () => { var r = app.Sim.SupportPupil(id, n); if (r.Success) { app.Act(r, false); Pupil(id); } else Toast(app.Error(r.Code)); }, true).Disabled = p.LastSupportDay == app.State.Day; }
        foreach (var group in app.State.Classes) { var c = group; Btn(modalBody, app.T("Zuordnen: ", "Assign: ") + c.Name, () => { var r = app.Sim.AssignPupil(id, c.RoomId); if (r.Success) { app.Act(r, false); Pupil(id); } else Toast(app.Error(r.Code)); }); }
    }
    private void Research()
    {
        Text(content, app.T("Forschung benötigt eine Bibliothek und Fortschritt über mehrere Schultage.", "Research needs a library and progresses over multiple school days."));
        foreach (var spec in AcademyCatalog.Research)
        {
            var r = spec; bool done = app.Sim.Researched(r.Kind);
            Card(content, (done ? "✓ " : "") + app.T(r.NameDe, r.NameEn), app.T(r.DetailDe, r.DetailEn) + "\n" + app.T("Ab Tag ", "From day ") + app.Sim.ResearchStartDay(r.Kind) + " · " + app.T("Schulstufe ", "School level ") + r.Level + " · " + app.Sim.ResearchRequired(r.Kind) + app.T(" Punkte", " points"));
            if (app.State.ActiveResearch == (int)r.Kind) Text(content, app.State.ResearchProgress + "/" + app.Sim.ResearchRequired(r.Kind), 28, true);
            Btn(content, done ? app.T("Abgeschlossen", "Completed") : app.T("Projekt starten · ", "Start project · ") + app.Sim.Price(r.Cost) + " €", () => app.Act(app.Sim.StartResearch(r.Kind), false), true).Disabled = done || app.State.ActiveResearch >= 0;
        }
    }
    private void Clubs()
    {
        foreach (var spec in AcademyCatalog.Clubs)
        {
            var c = spec; bool active = app.State.Clubs.Contains(c.Kind); int cost = app.State.ClubLicenses.Contains(c.Kind) ? 0 : app.Sim.Price(c.Setup);
            Card(content, app.T(c.NameDe, c.NameEn), app.RoomName(c.Room) + " · " + c.Daily + app.T(" €/Tag", " €/day"));
            Btn(content, active ? app.T("AG pausieren", "Pause club") : app.T("AG starten · ", "Start club · ") + cost + " €", () => app.Act(app.Sim.SetClub(c.Kind, !active)), active).Disabled = !active && !app.Sim.ClubAvailable(c.Kind);
        }
    }
    private void Year()
    {
        Card(content, app.T("Schuljahr ", "School year ") + app.Sim.SchoolYear, app.T("Tag ", "Day ") + app.Sim.YearDay + "/60 · " + app.T("Abschnitt ", "Term ") + app.Sim.Term + "/3\n" + app.State.ExamsHeld + app.T(" Prüfungen · ", " exams · ") + app.State.Graduates + app.T(" Abschlüsse", " graduates"));
        Text(content, app.T("Prüfungen an Tag 20, 40 und 60. Versetzung ab 45 %. Abschlussjahrgang frei von 6 bis 13.", "Exams on days 20, 40 and 60. Pass mark: 45%. Choose graduation grade 6–13."));
        Btn(content, app.T("Abschlussjahrgang: ", "Graduation grade: ") + app.State.GraduationGrade, () => app.Act(app.Sim.SetSchoolPath(app.State.GraduationGrade == 13 ? 6 : app.State.GraduationGrade + 1), false));
        if (app.State.ActiveContract >= 0) Text(content, app.T(SchoolSimulation.CareerDe[app.State.ActiveContract], SchoolSimulation.CareerEn[app.State.ActiveContract]) + " · " + app.State.ContractProgress + "/10 " + app.T("stabile Tage", "stable days"), 24, true);
        foreach (int n in app.Sim.CareerChoices()) { int k = n; Btn(content, app.T(SchoolSimulation.CareerDe[k], SchoolSimulation.CareerEn[k]) + "\n" + app.Sim.CareerCost + " € → " + app.Sim.CareerReward + " €", () => app.Act(app.Sim.StartCareer(k), false)).Disabled = app.State.ActiveContract >= 0 || app.State.ContractYear == app.Sim.SchoolYear; }
        Text(content, app.T("Prestige: ", "Prestige: ") + app.State.Prestige, 26, true);
        foreach (var report in app.State.Reports.AsEnumerable().Reverse().Take(5)) Card(content, app.T("Zeugnis · Tag ", "Report card · day ") + report.Day, report.Average + "% · " + report.Passed + app.T(" bestanden · ", " passed · ") + report.Graduated + app.T(" Abschlüsse", " graduated"));
        for (int i = 0; i < AcademyCatalog.AchievementDe.Length; i++) Text(content, ((app.State.AchievementMask & 1 << i) != 0 ? "✓ " : "○ ") + app.T(AcademyCatalog.AchievementDe[i], AcademyCatalog.AchievementEn[i]), 20);
    }
    public void Story()
    {
        OpenModal(app.T("Mara begleitet deinen Campus", "Mara guides your campus"), app.T("Ein guter Anfang braucht nicht alles auf einmal.", "A good beginning does not need everything at once.")); Guide(modalBody);
        Btn(modalBody, app.T("Mein Team aufbauen", "Build my team"), () => { CloseModal(); ShowManagement(1); }, true, "storyTeam");
        Btn(modalBody, app.T("Campus erkunden", "Explore campus"), CloseModal);
    }
    public void Pending()
    {
        if (app.State.GameOver)
        {
            OpenModal(app.T("Diese Schule ist zahlungsunfähig", "This school is insolvent"), app.T("Andere Spielstände bleiben erhalten. Starte eine neue Schule oder versuche einen anderen Schwierigkeitsgrad.", "Other saves remain intact. Start a new school or choose another difficulty.")); Btn(modalBody, app.T("Hauptmenü", "Main menu"), app.ReturnToMenu); return;
        }
        if (app.State.PendingEvent < 0) return;
        var e = SchoolSimulation.Events[app.State.PendingEvent]; OpenModal(app.T(e.TitleDe, e.TitleEn), app.T(e.BodyDe, e.BodyEn));
        for (int i = 0; i < 3; i++) { int n = i; Btn(modalBody, app.T(e.OptionsDe[i], e.OptionsEn[i]) + "\n" + app.T("Preis nach Schwierigkeit: ", "Difficulty-adjusted price: ") + app.Sim.Price(e.Costs[i]) + " €", () => { var r = app.Sim.ChooseEvent(n); if (r.Success) { CloseModal(); app.Act(r); } else Toast(app.Error(r.Code)); }, true, "event" + i).Disabled = !app.Sim.CanAfford(app.Sim.Price(e.Costs[i])); }
    }
    public void OpenModal(string title, string body)
    {
        CloseModal(); modal = new Control { Name = "Modal" }; safe.AddChild(modal); Fill(modal);
        var shade = new ColorRect { Color = new Color(.12f, .23f, .19f, .65f) }; modal.AddChild(shade); Fill(shade);
        var p = Panel(modal); p.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        float w = Math.Min(760, W - 72), h = Math.Min(810, H - 72);
        p.OffsetLeft = -w / 2; p.OffsetRight = w / 2; p.OffsetTop = -h / 2; p.OffsetBottom = h / 2;
        var v = Column(p); var row = Row(v); Text(row, title, 31, true); Btn(row, "×", Back).SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd;
        modalBody = Scroll(v); if (!string.IsNullOrEmpty(body)) Text(modalBody, body, 22);
    }
    public void CloseModal()
    {
        if (modal == null) return; safe.RemoveChild(modal); modal.QueueFree(); modal = null; modalBody = null;
    }
    public void Back()
    {
        if (!app.Menu && !app.State.HasDirector) { app.ReturnToMenu(); return; }
        CloseModal();
    }
    public void Confirm(string title, string body, Action yes)
    {
        OpenModal(title, body); Btn(modalBody, app.T("Bestätigen", "Confirm"), () => { CloseModal(); yes(); }, true); Btn(modalBody, app.T("Abbrechen", "Cancel"), CloseModal);
    }
    private void Settings()
    {
        OpenModal(app.T("Einstellungen", "Settings"), app.T("Deine Einstellungen gelten für alle Spielstände.", "Your preferences apply to all saves."));
        Btn(modalBody, "Deutsch / English", () => { app.Settings.Language = app.Settings.Language == "de" ? "en" : "de"; app.State.Language = app.Settings.Language; app.ApplySettings(); if (app.Menu) MainMenu(); else Hud(); Settings(); });
        Slider(modalBody, app.T("Musik", "Music"), app.Settings.Music, 0, 1, .05, x => app.Settings.Music = (float)x);
        Slider(modalBody, app.T("Effekte", "Effects"), app.Settings.Effects, 0, 1, .05, x => app.Settings.Effects = (float)x);
        Slider(modalBody, app.T("Kamerageschwindigkeit", "Camera speed"), app.Settings.CameraSpeed, .5, 2, .1, x => app.Settings.CameraSpeed = (float)x);
        Btn(modalBody, app.T("Grafik: ", "Graphics: ") + app.T(new[] { "Sparsam", "Ausgewogen", "Hoch" }[app.Settings.Graphics], new[] { "Low", "Balanced", "High" }[app.Settings.Graphics]), () => { app.Settings.Graphics = (app.Settings.Graphics + 1) % 3; app.Campus.Rebuild(); app.ApplySettings(); Settings(); });
        Btn(modalBody, "FPS: " + app.Settings.Fps, () => { app.Settings.Fps = app.Settings.Fps == 30 ? 60 : 30; app.ApplySettings(); Settings(); });
        Toggle(modalBody, app.T("Weniger Bewegung", "Reduced motion"), app.Settings.ReducedMotion, x => app.Settings.ReducedMotion = x);
        Toggle(modalBody, app.T("Raumnamen", "Room labels"), app.Settings.Labels, x => app.Settings.Labels = x);
        Toggle(modalBody, app.T("Automatisch speichern", "Autosave"), app.Settings.Autosave, x => app.Settings.Autosave = x);
        Btn(modalBody, app.T("Fertig", "Done"), () => { app.ApplySettings(); CloseModal(); });
    }
    private void Slider(Control parent, string name, double value, double min, double max, double step, Action<double> change)
    {
        Text(parent, name, 24, true); var slider = new HSlider { MinValue = min, MaxValue = max, Step = step, Value = value, CustomMinimumSize = new Vector2(0, Tap), SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        slider.ValueChanged += n => { change(n); app.ApplySettings(); }; parent.AddChild(slider);
    }
    private void Toggle(Control parent, string name, bool value, Action<bool> change)
    { var toggle = new CheckButton { Text = name, ButtonPressed = value, CustomMinimumSize = new Vector2(0, Tap) }; toggle.Toggled += n => { change(n); app.ApplySettings(); }; parent.AddChild(toggle); }
    private void ImportDialog()
    {
        var dialog = new FileDialog { FileMode = FileDialog.FileModeEnum.OpenFile, Access = FileDialog.AccessEnum.Filesystem, UseNativeDialog = true, Filters = new[] { "*.save,*.json;Rektor / Unity saves" } };
        root.AddChild(dialog);
        dialog.FileSelected += path =>
        {
            try
            {
                string data = Godot.FileAccess.GetFileAsString(path);
                if (data.Length > SaveArchive.MaxBytes) throw new InvalidDataException("Save is too large");
                SchoolState state;
                if (data.StartsWith("REKTOR 1\n"))
                {
                    int first = data.IndexOf('\n'), second = data.IndexOf('\n', first + 1); if (second < 0) throw new InvalidDataException("Invalid save header");
                    string payload = data[(second + 1)..]; string hash = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
                    if (hash != data[(first + 1)..second]) throw new InvalidDataException("Checksum mismatch"); state = RektorApp.Decode(payload);
                }
                else state = RektorApp.Decode(data);
                if (!SchoolSimulation.MigrateSave(state)) throw new InvalidDataException("Invalid school state");
                OpenModal(app.T("Importieren in …", "Import into …"), state.SchoolName);
                for (int i = 1; i <= SaveArchive.Slots; i++)
                {
                    int slot = i; Btn(modalBody, app.T("Spielstand ", "Save slot ") + slot, () => { void Write() { if (app.Saves.Write(slot, state, out string error)) { CloseModal(); Toast(app.T("Importiert. Öffne den Spielstand im Hauptmenü.", "Imported. Open the save from the main menu.")); } else Toast(error); } if (app.Saves.HasData(slot)) Confirm(app.T("Spielstand ersetzen?", "Replace save?"), "#" + slot, Write); else Write(); });
                }
            }
            catch (Exception e) { Toast(app.T("Import fehlgeschlagen: ", "Import failed: ") + e.Message); }
            dialog.QueueFree();
        };
        dialog.Canceled += dialog.QueueFree; dialog.PopupCentered(new Vector2I(900, 650));
    }
    public void Toast(string message) { if (toast == null) { GD.Print(message); return; } toast.Text = message; toast.Visible = true; toastTime = 4; }
    public override void _Process(double delta) { if (toastTime > 0 && (toastTime -= delta) <= 0) toast.Visible = false; }
}

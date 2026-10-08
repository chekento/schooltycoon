using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using KoSch.SchoolTycoon.Core;

namespace KoSch.SchoolTycoon
{
    public sealed partial class SchoolUI : MonoBehaviour
    {
        private SchoolApp app;
        private RectTransform safe, leftContent, rightContent, bottom, modal, modalBody;
        private GameObject screen, toastObject;
        private Font font;
        private Sprite round;
        private Text money, students, quality, calendar, mode, pauseLabel, toast;
        private int tab;
        private float toastUntil;
        private Avatar draft;
        public bool HasModal { get { return modal != null; } }
        private readonly Color ink = CampusRenderer.ColorHex("274D5C"), muted = CampusRenderer.ColorHex("66838E"), paper = CampusRenderer.ColorHex("FFFDF5"), teal = CampusRenderer.ColorHex("347E83");

        public void Initialize(SchoolApp owner)
        {
            app = owner;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            round = RoundedSprite();
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight; scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>();
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
            }
            safe = Rect("Safe area", transform); Fit(safe);
            Refresh();
            if (!app.State.HasDirector) ShowDirector(); else ShowPending();
        }
        private void Update()
        {
            Rect area = Screen.safeArea;
            safe.anchorMin = new Vector2(area.xMin / Screen.width, area.yMin / Screen.height);
            safe.anchorMax = new Vector2(area.xMax / Screen.width, area.yMax / Screen.height);
            safe.offsetMin = safe.offsetMax = Vector2.zero;
            if (toastObject != null && Time.unscaledTime > toastUntil) toastObject.SetActive(false);
        }
        private Sprite RoundedSprite()
        {
            var texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
            {
                float cx = Mathf.Clamp(x, 8, 23), cy = Mathf.Clamp(y, 8, 23);
                float distance = new Vector2(x - cx, y - cy).magnitude;
                texture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(8.5f - distance)));
            }
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(9, 9, 9, 9));
        }
        private RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform)); var r = go.GetComponent<RectTransform>(); r.SetParent(parent, false); return r;
        }
        private void Fit(RectTransform r, float inset = 0)
        { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.one * inset; r.offsetMax = Vector2.one * -inset; }
        private RectTransform Panel(string name, Transform parent, Color color)
        {
            RectTransform r = Rect(name, parent); Image image = r.gameObject.AddComponent<Image>(); image.sprite = round; image.type = Image.Type.Sliced; image.color = color; return r;
        }
        private void Place(RectTransform r, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        { r.anchorMin = r.anchorMax = anchor; r.pivot = pivot; r.anchoredPosition = position; r.sizeDelta = size; }
        private LayoutElement Height(Transform p, float h)
        { var e = p.gameObject.AddComponent<LayoutElement>(); e.minHeight = e.preferredHeight = h; return e; }
        private Text Text(Transform parent, string value, int size = 20, float h = 34, Color? color = null, bool bold = false)
        {
            var r = Rect("Text", parent); var t = r.gameObject.AddComponent<Text>();
            t.font = font; t.fontSize = size; t.text = value; t.color = color ?? ink;
            t.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal; t.supportRichText = false;
            t.alignment = TextAnchor.MiddleLeft; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate; t.raycastTarget = false;
            Height(r, h); return t;
        }
        private void VStack(RectTransform r, int padding = 16, int spacing = 9)
        {
            var layout = r.gameObject.AddComponent<VerticalLayoutGroup>(); layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing; layout.childControlWidth = true; layout.childControlHeight = true;
            layout.childForceExpandWidth = true; layout.childForceExpandHeight = false;
        }
        private RectTransform Row(Transform p, float h = 42)
        {
            RectTransform r = Rect("Row", p); Height(r, h);
            var group = r.gameObject.AddComponent<HorizontalLayoutGroup>(); group.spacing = 7;
            group.childControlWidth = group.childControlHeight = true; group.childForceExpandWidth = true; group.childForceExpandHeight = true;
            return r;
        }
        private Button Button(Transform p, string label, Action click, float h = 44, bool primary = false)
        {
            RectTransform r = Panel(label, p, primary ? teal : CampusRenderer.ColorHex("E5EEEB")); Height(r, h);
            var b = r.gameObject.AddComponent<Button>(); b.onClick.AddListener(() => click());
            ColorBlock colors = b.colors; colors.highlightedColor = new Color(.92f, .99f, .98f); colors.pressedColor = new Color(.72f, .87f, .85f); colors.disabledColor = new Color(.65f, .68f, .67f); b.colors = colors;
            Text t = Text(r, label, 17, h, primary ? Color.white : ink, true);
            Fit(t.rectTransform, 7); t.alignment = TextAnchor.MiddleCenter;
            return b;
        }
        private RectTransform Scroll(RectTransform parent, float top, float bottomInset)
        {
            RectTransform container = Rect("Scroll", parent); Fit(container);
            container.offsetMin = new Vector2(0, bottomInset); container.offsetMax = new Vector2(0, -top);
            var scroll = container.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            RectTransform viewport = Rect("Viewport", container); Fit(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            // A transparent image allows wheel/drag input in the empty portions of the viewport.
            var image = viewport.gameObject.AddComponent<Image>(); image.color = Color.clear;
            RectTransform content = Rect("Content", viewport); content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f, 1);
            content.sizeDelta = new Vector2(0, 0); VStack(content);
            var fit = content.gameObject.AddComponent<ContentSizeFitter>(); fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = viewport; scroll.content = content;
            return content;
        }
        private void CardText(Transform p, string title, string body, float bodyHeight = 52)
        { Text(p, title, 20, 32, ink, true); Text(p, body, 16, bodyHeight, muted); }
        public void Refresh()
        {
            if (screen != null) { screen.SetActive(false); Destroy(screen); }
            screen = Rect("Game HUD", safe).gameObject; Fit((RectTransform)screen.transform);
            RectTransform top = Panel("Status", screen.transform, paper);
            top.anchorMin = new Vector2(0, 1); top.anchorMax = Vector2.one; top.pivot = new Vector2(.5f, 1); top.anchoredPosition = new Vector2(0, -16); top.sizeDelta = new Vector2(-32, 80);
            var layout = top.gameObject.AddComponent<HorizontalLayoutGroup>(); layout.padding = new RectOffset(22, 20, 10, 10); layout.spacing = 22;
            layout.childControlWidth = layout.childControlHeight = true; layout.childForceExpandWidth = true;
            Text brand = Text(top, "THE SCHOOL\nSIMULATION 0.3", 21, 58, ink, true); brand.GetComponent<LayoutElement>().preferredWidth = 235;
            money = Text(top, "", 21, 58, ink, true); students = Text(top, "", 19, 58); quality = Text(top, "", 18, 58); calendar = Text(top, "", 19, 58);
            Button(top, app.State.Language == "de" ? "DE / EN" : "EN / DE", () => app.ToggleLanguage(), 50).GetComponent<LayoutElement>().preferredWidth = 100;

            RectTransform left = Panel("Build palette", screen.transform, paper);
            Place(left, new Vector2(0, 1), new Vector2(0, 1), new Vector2(16, -110), new Vector2(242, 650));
            Text title = Text(left, app.T("DEINE SCHULE", "YOUR SCHOOL"), 20, 45, ink, true);
            Place(title.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(18, -10), new Vector2(215, 40));
            leftContent = Scroll(left, 58, 6); BuildPalette();

            RectTransform right = Panel("Management", screen.transform, paper);
            Place(right, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-16, -110), new Vector2(354, 650));
            string[] labels = { app.T("Schule", "School"), app.T("Team", "Staff"), app.T("Klassen", "Classes"), app.T("Budget", "Budget"), app.T("Schüler", "Pupils"), app.T("Forschung", "Research"), app.T("AGs", "Clubs"), app.T("Schuljahr", "Year") };
            for (int row = 0; row < 2; row++)
            {
                RectTransform tabs = Row(right, 42); Place(tabs, new Vector2(0, 1), new Vector2(0, 1), new Vector2(12, -12 - row * 47), new Vector2(330, 42));
                for (int i = row * 4; i < row * 4 + 4; i++) { int n = i; Button(tabs, labels[i], () => { tab = n; Refresh(); }, 42, tab == i); }
            }
            rightContent = Scroll(right, 111, 6);
            if (tab == 0) Overview(); else if (tab == 1) Staff(); else if (tab == 2) Classes(); else if (tab == 3) Budget();
            else if (tab == 4) Pupils(); else if (tab == 5) Research(); else if (tab == 6) Activities(); else AcademicYear();

            bottom = Panel("Controls", screen.transform, paper);
            bottom.anchorMin = Vector2.zero; bottom.anchorMax = new Vector2(1, 0); bottom.pivot = new Vector2(.5f, 0); bottom.anchoredPosition = new Vector2(0, 16); bottom.sizeDelta = new Vector2(-32, 88);
            var controls = bottom.gameObject.AddComponent<HorizontalLayoutGroup>(); controls.padding = new RectOffset(14, 14, 12, 12); controls.spacing = 7;
            controls.childControlHeight = controls.childControlWidth = true; controls.childForceExpandWidth = true;
            mode = Text(bottom, "", 16, 58); mode.GetComponent<LayoutElement>().preferredWidth = 235;
            pauseLabel = Button(bottom, "", () => app.TogglePause(), 58, true).GetComponentInChildren<Text>();
            Button(bottom, "1× / 3×", () => app.SetSpeed(app.Speed == 1 ? 3 : 1), 58);
            Button(bottom, app.T("Tag beenden", "End day"), () => { if (!HasModal) app.EndDay(); }, 58);
            Button(bottom, app.T("Drehen", "Rotate"), () => app.Rotate(), 58);
            Button(bottom, "−", () => app.CameraRig.Zoom(1.5f), 58);
            Button(bottom, "+", () => app.CameraRig.Zoom(-1.5f), 58);
            Button(bottom, app.T("Ansicht", "View"), () => app.CameraRig.Orbit(1), 58);
            Button(bottom, app.T("Speichern", "Save"), () => app.Save(true), 58);
            RectTransform help = Panel("World hint", screen.transform, new Color(.97f, .98f, .93f, .94f));
            Place(help, new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(-55, 123), new Vector2(760, 42));
            Text hint = Text(help, app.T("Klicken: bauen / auswählen   ·   Ziehen: Kamera   ·   Mausrad / Pinch: Zoom", "Click: build / select   ·   Drag: camera   ·   Wheel / pinch: zoom"), 15, 36, muted);
            Fit(hint.rectTransform, 6); hint.alignment = TextAnchor.MiddleCenter;
            RefreshStats();
            if (modal != null) modal.SetAsLastSibling();
            if (toastObject != null) toastObject.transform.SetAsLastSibling();
        }
        public void RefreshStats()
        {
            if (money == null) return;
            SchoolState s = app.State; SchoolSimulation sim = app.Simulation;
            money.text = s.Cash.ToString("N0") + " €\n" + app.T("Kassenbestand", "School funds");
            students.text = s.Students + " / " + sim.Capacity + "\n" + app.T("Schüler / betreute Plätze", "Students / staffed seats");
            quality.text = app.T("Lernen ", "Learning ") + s.Learning + "%  ·  " + app.T("Freude ", "Joy ") + s.Happiness + "%\n" + app.T("Ansehen ", "Reputation ") + s.Reputation + "%";
            calendar.text = app.T("Jahr ", "Year ") + app.Simulation.SchoolYear + " · " + app.T("Tag ", "Day ") + app.Simulation.YearDay + "  ·  " + (s.ClockMinute / 60).ToString("00") + ":" + (s.ClockMinute % 60).ToString("00") + "\n" + (app.Paused ? app.T("Planungspause", "Planning paused") : app.Speed + "×");
            mode.text = app.BuildKind.HasValue ? app.RoomName(app.BuildKind.Value) + (app.Rotated ? " ↻" : "") : app.Demolition ? app.T("Rückbau: Raum auswählen", "Demolish: select room") : app.T("Campus erkunden", "Explore campus");
            pauseLabel.text = app.Paused ? app.T("Start", "Play") : app.T("Pause", "Pause");
        }
        private void BuildPalette()
        {
            Button(leftContent, app.T("Erkunden / Auswählen", "Explore / select"), () => app.ChooseBuild(null), 46, !app.BuildKind.HasValue && !app.Demolition);
            Text(leftContent, app.T("RÄUME BAUEN", "BUILD ROOMS"), 15, 30, muted, true);
            foreach (RoomSpec spec in Catalog.Rooms)
            {
                RoomKind kind = spec.Kind; bool locked = app.State.Level < spec.UnlockLevel;
                string name = app.RoomName(kind) + (locked ? app.T(" · gesperrt", " · locked") : "");
                Button b = Button(leftContent, name + "\n" + spec.Cost + " €  ·  " + spec.Width + "×" + spec.Height, () => app.ChooseBuild(kind), 65, app.BuildKind == kind);
                b.interactable = !locked && app.Simulation.CanOperate;
            }
            Button(leftContent, app.T("Rückbau · 50 % Erstattung", "Demolish · 50% refund"), () => app.ChooseBuild(null, true), 52, app.Demolition);
            Text(leftContent, app.T("Räume brauchen einen Fluranschluss. Jeder Flur muss zum Eingang führen.", "Rooms need corridor access. Every corridor must lead to the entrance."), 15, 70, muted);
        }
        public void SelectOverview() { tab = 0; Refresh(); }
        private void Overview()
        {
            var s = app.State; var sim = app.Simulation;
            CardText(rightContent, s.HasDirector ? app.T("Leitung: ", "Director: ") + s.Director.Name : app.T("Willkommen auf dem Campus", "Welcome to the campus"),
                sim.Count(RoomKind.Classroom) + app.T(" Klassenräume · ", " classrooms · ") + sim.Teachers + app.T(" Lehrkräfte\n", " teachers\n") + app.T("Sauberkeit ", "Cleanliness ") + s.Cleanliness + "%");
            if (sim.Capacity < s.Students) Text(rightContent, app.T("Es fehlen betreute Unterrichtsplätze. Stelle eine Lehrkraft ein!", "Staffed seats are missing. Hire a teacher!"), 17, 66, CampusRenderer.ColorHex("B7674F"), true);
            if (s.CompletedGoals < SchoolSimulation.Goals.Length)
            {
                Goal goal = SchoolSimulation.Goals[s.CompletedGoals];
                CardText(rightContent, app.T("FÖRDERZIEL · ", "GRANT GOAL · ") + (s.CompletedGoals + 1), app.T(goal.TitleDe, goal.TitleEn), 36);
                Text(rightContent, app.T(goal.DetailDe, goal.DetailEn), 17, 66);
                Text(rightContent, app.T("Fördergeld: ", "Grant: ") + goal.Reward + " €", 18, 35, teal, true);
            }
            else CardText(rightContent, app.T("Alle Förderziele erreicht!", "All grant goals completed!"), app.T("Deine Schule kann weiter wachsen. Gestalte deinen Wunschcampus.", "Your school can keep growing. Create your dream campus."), 66);
            Room selected = s.Rooms.FirstOrDefault(r => r.Id == app.SelectedRoomId);
            if (selected != null)
            {
                RoomSpec spec = Catalog.Get(selected.Kind);
                CardText(rightContent, app.RoomName(selected.Kind), app.T(spec.DescriptionDe, spec.DescriptionEn), 66);
                Text(rightContent, selected.Width + "×" + selected.Height + app.T(" Felder · ", " tiles · ") + spec.DailyCost * (selected.Kind == RoomKind.Corridor ? selected.Width * selected.Height : 1) + app.T(" €/Tag", " €/day"), 16, 40);
                RoomDevelopment(selected);
                if (selected.Id > 2) Button(rightContent, app.T("Diesen Raum zurückbauen", "Demolish this room"), () => Confirm(app.T("Raum zurückbauen?", "Demolish room?"), app.T("Erstattung: ", "Refund: ") + spec.Cost / 2 + " €", () => { app.SelectedRoomId = -1; app.Act(sim.Demolish(selected.Id)); }), 45);
            }
            CardText(rightContent, app.T("CAMPUS AUSBAUEN", "EXPAND THE CAMPUS"), app.T("Zunächst 20×16 Baufelder. Mit zusätzlichem Land stehen dir 32×24 Felder zur Verfügung.", "Start with 20×16 building tiles. Additional land opens the full 32×24 campus."), 88);
            Button land = Button(rightContent, s.CampusLevel > 0 ? app.T("Gesamter Campus gekauft", "Full campus owned") : app.T("Grundstück kaufen · 9.000 €", "Buy land · €9,000"), () => app.Act(sim.BuyLand()), 50);
            land.interactable = s.CampusLevel == 0 && sim.CanOperate && s.Level >= 1;
            Button(rightContent, app.T("Tagesbericht öffnen", "Open daily report"), () => ShowDayReport(), 46);
            Button(rightContent, app.T("Kamera zurücksetzen", "Reset camera"), () => app.CameraRig.Home(), 45);
            Button(rightContent, app.T("Neue Schule beginnen", "Start a new school"), () => Confirm(app.T("Neue Schule?", "New school?"), app.T("Der aktuelle Spielstand wird durch eine neue Schule ersetzt.", "The current save will be replaced by a new school."), () => app.NewSchool()), 45);
            foreach (string news in s.News.Take(3))
            {
                int n; if (news.StartsWith("goal:") && int.TryParse(news.Substring(5), out n) && n < SchoolSimulation.Goals.Length)
                    Text(rightContent, "✓ " + app.T(SchoolSimulation.Goals[n].TitleDe, SchoolSimulation.Goals[n].TitleEn), 15, 38, teal);
            }
        }
        private void Staff()
        {
            CardText(rightContent, app.T("DEIN TEAM", "YOUR TEAM"), app.T("Jede Lehrkraft betreut einen Klassenraum. Hausdienst hält die Schule sauber; Beratung steigert Zufriedenheit.", "Each teacher staffs one classroom. Caretakers keep the school clean; counselors support happiness."), 104);
            foreach (Employee employee in app.State.Staff)
            {
                Employee p = employee;
                CardText(rightContent, p.Name, app.RoleName(p.Role) + " · " + p.Salary + app.T(" €/Tag", " €/day"), 35);
                if (p.Role == StaffRole.Teacher) Text(rightContent, app.SubjectName(p.Specialty) + " · " + p.Skill + app.T(" % Kompetenz", "% skill"), 16, 40, muted);
                Text(rightContent, app.T("Energie ", "Energy ") + p.Energy + "% · " + app.T("Teamgefühl ", "Morale ") + p.Morale + "%", 16, 40, muted);
                Button training = Button(rightContent, app.T("Fortbildung · ", "Training · ") + app.Simulation.TrainingCost(p) + " €", () => app.Act(app.Simulation.TrainStaff(p.Id), false), 43);
                training.interactable = p.TrainingLevel < 3 && app.Simulation.CanOperate;
                Button(rightContent, app.T("Freistellen", "Release"), () => Confirm(app.T("Teammitglied freistellen?", "Release staff member?"), p.Name, () => app.Act(app.Simulation.Fire(p.Id))), 40);
            }
            Text(rightContent, app.T("BEWERBUNGEN DES TAGES", "TODAY'S APPLICANTS"), 16, 34, teal, true);
            foreach (Employee candidate in app.Simulation.Candidates())
            {
                int id = candidate.Id;
                CardText(rightContent, candidate.Name, app.RoleName(candidate.Role) + " · " + candidate.Salary + app.T(" €/Tag\n", " €/day\n") + (candidate.Role == StaffRole.Teacher ? app.SubjectName(candidate.Specialty) + " · " + candidate.Skill + "%" : app.T("Für eine lebendige Schule", "For a thriving school")), 60);
                Button hire = Button(rightContent, app.T("Einstellen · ", "Hire · ") + (candidate.Role == StaffRole.Teacher ? 700 : 400) + " €", () => app.Act(app.Simulation.Hire(id)), 43, true);
                hire.interactable = app.Simulation.CanOperate && !app.State.Staff.Any(e => e.Name == candidate.Name);
            }
        }
        private void Budget()
        {
            Ledger f = app.Simulation.Forecast(); int net = f.Income - f.Expenses;
            CardText(rightContent, app.T("TAGESPROGNOSE", "DAILY FORECAST"), app.T("Baukosten und Einstellungsgebühren sind einmalig. Diese Positionen fallen jeden Schultag an.", "Construction and hiring fees are one-time costs. These items recur each school day."), 88);
            Text(rightContent, app.T("Bildungsförderung   +", "Education funding   +") + f.Funding + " €", 17, 32, teal);
            Text(rightContent, app.T("Mensaerlös   +", "Canteen income   +") + f.Meals + " €", 17, 32, teal);
            Text(rightContent, app.T("Gehälter   −", "Salaries   −") + f.Salaries + " €", 17, 32);
            Text(rightContent, app.T("Betriebskosten   −", "Maintenance   −") + f.Maintenance + " €", 17, 32);
            Text(rightContent, app.T("Lernmaterial   −", "Learning supplies   −") + f.Supplies + " €", 17, 32);
            Text(rightContent, app.T("Arbeitsgemeinschaften   −", "Clubs   −") + f.Activities + " €", 17, 32);
            Text(rightContent, app.T("Förderung / Essen   −", "Support / meals   −") + f.Support + " €", 17, 32);
            Text(rightContent, app.T("Tagesbilanz   ", "Daily balance   ") + (net >= 0 ? "+" : "") + net + " €", 22, 44, net >= 0 ? teal : CampusRenderer.ColorHex("BA725C"), true);
            string[] budget = { app.T("Sparsam", "Basic"), app.T("Ausgewogen", "Balanced"), app.T("Großzügig", "Generous") };
            Button(rightContent, app.T("Lernmaterial: ", "Supplies: ") + budget[app.State.SupplyBudget], () => app.Act(app.Simulation.SetPolicy(app.State.EnrollmentPolicy, (app.State.SupplyBudget + 1) % 3), false), 46);
            string[] admissions = { app.T("Behutsam", "Gradual"), app.T("Normal", "Normal"), app.T("Aktiv", "Active") };
            Button(rightContent, app.T("Aufnahme: ", "Admissions: ") + admissions[app.State.EnrollmentPolicy], () => app.Act(app.Simulation.SetPolicy((app.State.EnrollmentPolicy + 1) % 3, app.State.SupplyBudget), false), 46);
            string[] support = { app.T("Klassenintern", "Class support"), app.T("Begleitung", "Guidance"), app.T("Intensive Förderung", "Intensive support") };
            Button(rightContent, app.T("Förderung: ", "Support: ") + support[app.State.SupportPolicy], () => app.Act(app.Simulation.SetWellbeingPolicy((app.State.SupportPolicy + 1) % 3, app.State.MealQuality), false), 54);
            Button(rightContent, app.T("Mittagessen: ", "Lunch: ") + budget[app.State.MealQuality], () => app.Act(app.Simulation.SetWellbeingPolicy(app.State.SupportPolicy, (app.State.MealQuality + 1) % 3), false), 46);
            Text(rightContent, app.T("LETZTE SCHULTAGE", "RECENT SCHOOL DAYS"), 16, 35, muted, true);
            foreach (Ledger l in app.State.History.AsEnumerable().Reverse().Take(8))
                Text(rightContent, app.T("Tag ", "Day ") + l.Day + "  ·  " + (l.Net >= 0 ? "+" : "") + l.Net + " €  ·  " + l.Students + app.T(" Schüler", " students"), 16, 30);
            if (app.State.Cash < 0) Text(rightContent, app.T("Deine Schule ist verschuldet. Nach fünf Tagen Schulden oder unter −5.000 € endet diese Schule.", "Your school is in debt. Five days in debt or funds below −€5,000 end this school."), 16, 88, CampusRenderer.ColorHex("BA725C"));
        }
        private void OpenModal(string title, string body)
        {
            CloseModal();
            modal = Panel("Modal blocker", safe, new Color(.1f, .24f, .3f, .86f)); Fit(modal);
            modal.offsetMax = new Vector2(0, -100);
            RectTransform card = Panel("Modal card", modal, paper);
            Place(card, new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(670, 600));
            modalBody = Scroll(card, 0, 0);
            Text(modalBody, "THE SCHOOL SIMULATION", 17, 38, teal, true);
            Text(modalBody, title, 30, 64, ink, true);
            Text(modalBody, body, 20, 112, muted);
        }
        private void CloseModal()
        {
            if (modal != null) { modal.gameObject.SetActive(false); Destroy(modal.gameObject); modal = null; }
        }
        public void Confirm(string title, string body, Action action)
        {
            OpenModal(title, body);
            Button(modalBody, app.T("Bestätigen", "Confirm"), () => { CloseModal(); action(); }, 56, true);
            Button(modalBody, app.T("Abbrechen", "Cancel"), () => CloseModal(), 52);
        }
        public void ShowDirector()
        {
            draft = new Avatar();
            DirectorStep(0);
        }
        private void DirectorStep(int step)
        {
            string[] titles = { app.T("Wer leitet deine Schule?", "Who leads your school?"), app.T("Dein persönlicher Stil", "Your personal style"), app.T("Wie heißt die Schulleitung?", "What's your director's name?") };
            string[] copy = { app.T("Schritt 1 von 3 · Wähle deinen Charakter. Die Auswahl verändert das Aussehen, alle Spielmöglichkeiten sind gleich.", "Step 1 of 3 · Choose your character. Appearance changes; all gameplay options are equal."), app.T("Schritt 2 von 3 · Wähle eine Haarfarbe für deinen Charakter.", "Step 2 of 3 · Pick your character's hair colour."), app.T("Schritt 3 von 3 · Gib einen Namen ein. Danach beginnt dein Campus mit einem Flur und einem Klassenzimmer.", "Step 3 of 3 · Enter a name. Your campus begins with a corridor and one classroom.") };
            OpenModal(titles[step], copy[step]);
            Portrait(modalBody, draft);
            if (step == 0)
            {
                string[] values = { "Female", "Male", "Diverse" }, de = { "Weiblich", "Männlich", "Divers" }, en = { "Female", "Male", "Non-binary" };
                for (int i = 0; i < 3; i++) { int n = i; Button(modalBody, app.T(de[i], en[i]), () => { draft.Gender = values[n]; DirectorStep(1); }, 56, true); }
            }
            else if (step == 1)
            {
                string[] values = { "Brown", "Black", "Blonde", "Red", "Silver" }, de = { "Braun", "Schwarz", "Blond", "Rot", "Silber" };
                for (int i = 0; i < values.Length; i++) { int n = i; Button(modalBody, app.T(de[i], values[i]), () => { draft.Hair = values[n]; DirectorStep(2); }, 49, true); }
                Button(modalBody, app.T("Zurück", "Back"), () => DirectorStep(0), 45);
            }
            else
            {
                RectTransform inputRect = Panel("Director name", modalBody, CampusRenderer.ColorHex("E4EFEB")); Height(inputRect, 64);
                var field = inputRect.gameObject.AddComponent<InputField>(); field.characterLimit = 24;
                Text value = Text(inputRect, "", 25, 60); Fit(value.rectTransform, 12); field.textComponent = value;
                Text placeholder = Text(inputRect, app.T("Dein Name", "Your name"), 23, 60, muted); Fit(placeholder.rectTransform, 12); field.placeholder = placeholder;
                field.text = "KoSch";
                RectTransform suggested = Row(modalBody, 43);
                foreach (string suggestedName in new[] { "Alex", "Sam", "Robin" })
                { string nameChoice = suggestedName; Button(suggested, nameChoice, () => field.text = nameChoice, 43); }
                Button(modalBody, app.T("Meine Schule eröffnen", "Open my school"), () =>
                {
                    string name = (field.text ?? "").Trim();
                    if (name.Length == 0) { Notify(app.T("Bitte gib einen Namen ein.", "Please enter a name.")); return; }
                    draft.Name = name; CloseModal(); app.CompleteDirector(draft);
                }, 58, true);
                Button(modalBody, app.T("Zurück", "Back"), () => DirectorStep(1), 45);
            }
        }
        private void Portrait(Transform parent, Avatar avatar)
        {
            RectTransform holder = Rect("Director portrait", parent); Height(holder, 95);
            RectTransform body = Panel("Jacket", holder, teal);
            Place(body, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, -23), new Vector2(90, 43));
            RectTransform hair = Panel("Hair", holder, CampusRenderer.ColorHex(CampusRenderer.HairColor(avatar.Hair)));
            Place(hair, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 18), new Vector2(62, avatar.Gender == "Male" ? 50 : 64));
            RectTransform face = Panel("Face", holder, CampusRenderer.ColorHex("EAC49B"));
            Place(face, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 12), new Vector2(48, 45));
            for (int i = -1; i <= 1; i += 2)
            {
                RectTransform eye = Panel("Eye", holder, ink);
                Place(eye, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(i * 10, 15), new Vector2(4, 6));
            }
            RectTransform fringe = Panel("Fringe", holder, CampusRenderer.ColorHex(CampusRenderer.HairColor(avatar.Hair)));
            Place(fringe, new Vector2(.5f, .5f), new Vector2(.5f, .5f), new Vector2(0, 35), new Vector2(58, 15));
        }
        public void ShowPending()
        {
            if (app.State.GameOver)
            {
                OpenModal(app.T("Ein neuer Anfang", "A new beginning"), app.T("Die Schule ist zahlungsunfähig. Mit deiner Erfahrung kannst du eine neue Schule planen.", "The school is insolvent. Use what you learned to plan a new school."));
                Button(modalBody, app.T("Neue Schule", "New school"), () => { CloseModal(); app.NewSchool(); }, 58, true);
                return;
            }
            if (app.State.PendingEvent < 0) return;
            SchoolEvent e = SchoolSimulation.Events[app.State.PendingEvent];
            OpenModal(app.T(e.TitleDe, e.TitleEn), app.T(e.BodyDe, e.BodyEn));
            for (int i = 0; i < 3; i++)
            {
                int choice = i;
                Button b = Button(modalBody, app.State.Language == "de" ? e.OptionsDe[i] : e.OptionsEn[i], () =>
                {
                    Result r = app.Simulation.ChooseEvent(choice);
                    if (r.Success) { CloseModal(); app.Act(r, false); }
                    else Notify(app.Error(r.Code));
                }, 62, true);
                Text(modalBody, app.T("Freude ", "Joy ") + Signed(e.Happiness[i]) + " · " + app.T("Ansehen ", "Reputation ") + Signed(e.Reputation[i]) + " · " + app.T("Lernen ", "Learning ") + Signed(e.Learning[i]), 16, 32, muted);
                b.interactable = e.Costs[i] == 0 || app.State.Cash >= e.Costs[i];
            }
        }
        public void Notify(string message)
        {
            if (toastObject == null)
            {
                RectTransform r = Panel("Notification", safe, CampusRenderer.ColorHex("2D5A66"));
                Place(r, new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -108), new Vector2(790, 82));
                toastObject = r.gameObject; toast = Text(r, "", 20, 74, Color.white); Fit(toast.rectTransform, 12); toast.alignment = TextAnchor.MiddleCenter;
            }
            toastObject.SetActive(true); toastObject.transform.SetAsLastSibling(); toast.text = message; toastUntil = Time.unscaledTime + 5;
        }
    }
}

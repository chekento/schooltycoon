using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoSch.SchoolTycoon.Core;

namespace KoSch.SchoolTycoon
{
    public sealed partial class SchoolUI
    {
        private int pupilPage, pupilFilter;
        private string Signed(int n) { return (n >= 0 ? "+" : "") + n; }
        private void OpenTab(int target) { CloseModal(); tab = target; Refresh(); }
        private void Progress(Transform parent, int value, int total)
        {
            RectTransform track = Panel("Progress", parent, CampusRenderer.ColorHex("DCE8E4")); Height(track, 18);
            RectTransform fill = Panel("Progress fill", track, teal); Fit(fill);
            fill.anchorMax = new Vector2(total == 0 ? 0 : Mathf.Clamp01((float)value / total), 1);
        }
        private void RoomDevelopment(Room room)
        {
            Text(rightContent, app.T("Zustand ", "Condition ") + room.Condition + "% · " + app.T("Ausbau ", "Upgrade ") + room.UpgradeLevel + "/2", 16, 38, muted);
            if (room.Kind != RoomKind.Corridor)
            {
                Button upgrade = Button(rightContent, app.T("Raum verbessern · ", "Upgrade room · ") + app.Simulation.UpgradeCost(room) + " €", () => app.Act(app.Simulation.UpgradeRoom(room.Id)), 47, true);
                upgrade.interactable = room.UpgradeLevel < 2 && app.Simulation.CanOperate;
                Text(rightContent, room.Kind == RoomKind.Classroom ? app.T("Je Stufe +4 Plätze und bessere Lernbedingungen.", "Each level adds 4 seats and better learning conditions.") : app.T("Bessere Ausstattung; Mensa und Sanitärraum versorgen mehr Schüler. +8 €/Tag je Stufe.", "Better equipment; canteens and restrooms serve more pupils. +€8/day per level."), 15, 76, muted);
            }
            Button repair = Button(rightContent, app.T("Instandsetzen · ", "Repair · ") + app.Simulation.RepairCost(room) + " €", () => app.Act(app.Simulation.RepairRoom(room.Id)), 43);
            repair.interactable = room.Condition < 100 && app.Simulation.CanOperate;
        }
        private void Classes()
        {
            CardText(rightContent, app.T("KLASSEN & UNTERRICHT", "CLASSES & LESSONS"), app.T("Jede Klasse hat feste Schüler, eine Lehrkraft und ihren eigenen Tagesplan. Lehrkräfte können jeweils nur eine Klasse betreuen.", "Each class has assigned pupils, one teacher and its own timetable. A teacher can lead only one class."), 118);
            foreach (SchoolClass group in app.State.Classes)
            {
                SchoolClass c = group; Employee teacher = app.Simulation.ClassTeacher(c);
                Room room = app.State.Rooms.First(r => r.Id == c.RoomId);
                var pupilsInClass = app.State.Pupils.Where(p => p.ClassRoomId == c.RoomId).ToList();
                int average = pupilsInClass.Count == 0 ? 0 : (int)pupilsInClass.Average(p => p.Scores.Average());
                CardText(rightContent, c.Name + " · " + pupilsInClass.Count + "/" + app.Simulation.RoomSeats(room), (teacher == null ? app.T("Keine Lehrkraft", "No teacher") : teacher.Name) + "\n" + app.T("Lernstand ", "Learning progress ") + average + "%", 63);
                Button(rightContent, app.T("Klasse verwalten", "Manage class"), () => ShowClass(c.RoomId), 45, true);
            }
            Text(rightContent, app.T("VORLAGE FÜR ALLE KLASSEN", "TEMPLATE FOR ALL CLASSES"), 16, 42, teal, true);
            Text(rightContent, app.T("Änderungen hier übernehmen alle Klassen. Individuelle Pläne findest du bei der jeweiligen Klasse.", "Changes here apply to every class. Individual plans are available in class management."), 15, 80, muted);
            for (int i = 0; i < 6; i++)
            {
                int slot = i;
                Button(rightContent, (i < 4 ? 8 + i : 9 + i) + ":00 · " + app.SubjectName(app.State.Timetable[i]) + " ›", () => app.Act(app.Simulation.SetLesson(slot, (Subject)(((int)app.State.Timetable[slot] + 1) % 5)), false), 46);
            }
        }
        private void ShowClass(int roomId)
        {
            SchoolClass group = app.State.Classes.FirstOrDefault(c => c.RoomId == roomId); if (group == null) return;
            OpenModal(group.Name, app.T("Feste Klassenzuordnung, eigene Fächer und eine verantwortliche Lehrkraft. Um 12 Uhr ist Mittagspause; ab 15 Uhr beginnen AGs.", "Assigned pupils, individual subjects and a responsible teacher. Lunch is at noon; clubs begin at 15:00."));
            Button(modalBody, app.T("Lehrkraft zuordnen", "Assign teacher"), () => ChooseTeacher(roomId), 48, true);
            Button(modalBody, app.T("Klasse umbenennen", "Rename class"), () => RenameClass(roomId), 43);
            for (int i = 0; i < 6; i++)
            {
                int slot = i;
                Button(modalBody, (i < 4 ? 8 + i : 9 + i) + ":00 · " + app.SubjectName(group.Lessons[i]) + " ›", () =>
                { app.Act(app.Simulation.SetClassLesson(roomId, slot, (Subject)(((int)group.Lessons[slot] + 1) % 5)), false); ShowClass(roomId); }, 45);
            }
            Text(modalBody, app.T("SCHÜLER DIESER KLASSE", "PUPILS IN THIS CLASS"), 16, 38, teal, true);
            foreach (Pupil pupil in app.State.Pupils.Where(p => p.ClassRoomId == roomId))
            { Pupil p = pupil; Button(modalBody, p.Name + " · " + (int)p.Scores.Average() + "%", () => ShowPupil(p.Id), 42); }
            Button(modalBody, app.T("Zurück zum Campus", "Back to campus"), () => CloseModal(), 52);
        }
        private void ChooseTeacher(int roomId)
        {
            OpenModal(app.T("Klassenleitung wählen", "Choose a class teacher"), app.T("Eine bereits zugeordnete Lehrkraft wechselt in diese Klasse. Die vorige Klasse braucht dann Ersatz.", "An assigned teacher moves to this class. Their previous class will then need a replacement."));
            foreach (Employee teacher in app.State.Staff.Where(e => e.Role == StaffRole.Teacher))
            {
                Employee p = teacher; SchoolClass previous = app.State.Classes.FirstOrDefault(c => c.TeacherId == p.Id);
                Button(modalBody, p.Name + " · " + app.SubjectName(p.Specialty) + (previous == null ? "" : " · " + previous.Name), () => { CloseModal(); app.Act(app.Simulation.AssignTeacher(roomId, p.Id)); ShowClass(roomId); }, 62, true);
            }
            Button(modalBody, app.T("Zuordnung aufheben", "Remove assignment"), () => { CloseModal(); app.Act(app.Simulation.AssignTeacher(roomId, 0)); ShowClass(roomId); }, 45);
            Button(modalBody, app.T("Zurück", "Back"), () => ShowClass(roomId), 48);
        }
        private void RenameClass(int roomId)
        {
            SchoolClass group = app.State.Classes.First(c => c.RoomId == roomId);
            OpenModal(app.T("Klassenname", "Class name"), app.T("Wähle einen Namen mit bis zu 20 Zeichen.", "Choose a name with up to 20 characters."));
            RectTransform input = Panel("Class name", modalBody, CampusRenderer.ColorHex("E4EFEB")); Height(input, 64);
            InputField field = input.gameObject.AddComponent<InputField>(); field.characterLimit = 20;
            Text value = Text(input, "", 25, 60); Fit(value.rectTransform, 12); field.textComponent = value; field.text = group.Name;
            Button(modalBody, app.T("Speichern", "Save"), () => { Result r = app.Simulation.SetClassName(roomId, field.text); if (!r.Success) { Notify(app.Error(r.Code)); return; } CloseModal(); app.Act(r); ShowClass(roomId); }, 52, true);
            Button(modalBody, app.T("Zurück", "Back"), () => ShowClass(roomId), 48);
        }
        private void Pupils()
        {
            CardText(rightContent, app.T("DEINE SCHÜLER", "YOUR PUPILS"), app.T("Noten entstehen aus Unterricht, Lieblingsfach, Ausstattung, Förderung und Anwesenheit. Hoher Stress kann zu Fehlzeiten führen.", "Progress reflects teaching, favourite subjects, facilities, support and attendance. High stress can cause absences."), 122);
            string[] filters = { app.T("Alle Schüler", "All pupils"), app.T("Förderbedarf", "Needs support"), app.T("Heute abwesend", "Absent today") };
            Button(rightContent, filters[pupilFilter] + " ›", () => { pupilFilter = (pupilFilter + 1) % 3; pupilPage = 0; Refresh(); }, 44);
            var roster = app.State.Pupils.Where(p => pupilFilter == 0 || pupilFilter == 1 && (p.Scores.Min() < 45 || p.Stress > 65 || p.Wellbeing < 40) || pupilFilter == 2 && p.Absent).ToList();
            int pages = Math.Max(1, (roster.Count + 11) / 12); pupilPage = Math.Min(pupilPage, pages - 1);
            Text(rightContent, roster.Count + app.T(" Schüler · Seite ", " pupils · page ") + (pupilPage + 1) + "/" + pages, 16, 35, muted);
            RectTransform controls = Row(rightContent);
            Button(controls, "‹", () => { pupilPage = Math.Max(0, pupilPage - 1); Refresh(); });
            Button(controls, "›", () => { pupilPage = Math.Min(pages - 1, pupilPage + 1); Refresh(); });
            foreach (Pupil pupil in roster.Skip(pupilPage * 12).Take(12))
            {
                Pupil p = pupil; SchoolClass group = app.Simulation.PupilClass(p);
                Button(rightContent, p.Name + "\n" + (group == null ? app.T("Ohne Klasse", "Unassigned") : group.Name) + " · " + (int)p.Scores.Average() + "%" + (p.Absent ? app.T(" · abwesend", " · absent") : ""), () => ShowPupil(p.Id), 65);
            }
            if (roster.Count == 0) Text(rightContent, app.T("Hier gibt es aktuell keine Schüler.", "There are currently no pupils in this view."), 17, 60, muted);
        }
        private void ShowPupil(int id)
        {
            Pupil p = app.State.Pupils.FirstOrDefault(s => s.Id == id); if (p == null) return;
            SchoolClass group = app.Simulation.PupilClass(p);
            OpenModal(p.Name, app.T("Jahrgang ", "Grade ") + p.Grade + " · " + (group == null ? app.T("Ohne Klasse", "Unassigned") : group.Name) + "\n" + app.T("Lieblingsfach: ", "Favourite subject: ") + app.SubjectName(p.Favourite) + "\n" + app.T("Anwesend: ", "Attended: ") + p.DaysAttended + "/" + p.DaysEnrolled + app.T(" Schultage", " school days"));
            Text(modalBody, app.T("Wohlbefinden ", "Wellbeing ") + p.Wellbeing + "% · " + app.T("Stress ", "Stress ") + p.Stress + "%", 19, 44, teal, true);
            for (int i = 0; i < 5; i++) { Text(modalBody, app.SubjectName((Subject)i) + " · " + p.Scores[i] + "%", 18, 34); Progress(modalBody, p.Scores[i], 100); }
            Text(modalBody, app.T("Letzte Prüfung ", "Last exam ") + p.LastExam + "%", 17, 38, muted);
            string[] actions = { app.T("Förderunterricht · 90 €", "Tutoring · €90"), app.T("Beratung · 60 €", "Counselling · €60"), app.T("Persönliches Gespräch · 0 €", "Personal conversation · €0") };
            string[] details = { app.T("Schwächstes Fach +8, Stress +3.", "Weakest subject +8, stress +3."), app.T("Stress −18, Wohlbefinden +10.", "Stress −18, wellbeing +10."), app.T("Stress −6, Wohlbefinden +3.", "Stress −6, wellbeing +3.") };
            for (int i = 0; i < 3; i++)
            {
                int choice = i;
                Button b = Button(modalBody, actions[i] + "\n" + details[i], () => { Result r = app.Simulation.SupportPupil(id, choice); if (r.Success) { CloseModal(); app.Act(r, false); ShowPupil(id); } else Notify(app.Error(r.Code)); }, 70, true);
                b.interactable = p.LastSupportDay != app.State.Day && app.Simulation.CanOperate;
            }
            Button(modalBody, app.T("Klasse wechseln", "Change class"), () => ChoosePupilClass(id), 46);
            Button(modalBody, app.T("Zurück zum Campus", "Back to campus"), () => CloseModal(), 50);
        }
        private void ChoosePupilClass(int id)
        {
            OpenModal(app.T("Klasse zuordnen", "Assign a class"), app.T("Nur Klassen mit freien physischen Plätzen können weitere Schüler aufnehmen.", "Only classes with free physical seats can accept another pupil."));
            foreach (SchoolClass group in app.State.Classes)
            {
                SchoolClass c = group;
                Button(modalBody, c.Name + " · " + app.Simulation.ClassSize(c) + "/" + app.Simulation.RoomSeats(app.State.Rooms.First(r => r.Id == c.RoomId)), () => { Result r = app.Simulation.AssignPupil(id, c.RoomId); if (r.Success) { CloseModal(); app.Act(r); ShowPupil(id); } else Notify(app.Error(r.Code)); }, 50, true);
            }
            Button(modalBody, app.T("Zurück", "Back"), () => ShowPupil(id), 50);
        }
        private void Research()
        {
            var s = app.State;
            CardText(rightContent, app.T("SCHULENTWICKLUNG", "SCHOOL DEVELOPMENT"), app.T("Forschung braucht eine Bibliothek. Die Projektkosten fallen einmalig an; Fortschritt entsteht am Tagesende. Größere Bibliotheken forschen schneller.", "Research needs a library. Project costs are paid once; progress arrives at day end. Upgraded libraries work faster."), 132);
            if (s.ActiveResearch >= 0)
            {
                ResearchSpec active = AcademyCatalog.Research[s.ActiveResearch];
                Text(rightContent, app.ResearchName(active.Kind), 20, 54, teal, true); Progress(rightContent, s.ResearchProgress, active.Points);
                Text(rightContent, s.ResearchProgress + "/" + active.Points + app.T(" Forschungspunkte", " research points"), 17, 43, muted);
            }
            foreach (ResearchSpec spec in AcademyCatalog.Research)
            {
                ResearchKind kind = spec.Kind; bool done = s.CompletedResearch.Contains(kind);
                CardText(rightContent, (done ? "✓ " : "") + app.ResearchName(kind), app.T(spec.DetailDe, spec.DetailEn), 77);
                Text(rightContent, app.T("Schulstufe ", "School level ") + spec.Level + " · " + spec.Points + app.T(" Punkte", " points"), 16, 38, muted);
                Button b = Button(rightContent, done ? app.T("Abgeschlossen", "Completed") : app.T("Projekt starten · ", "Start project · ") + spec.Cost + " €", () => app.Act(app.Simulation.StartResearch(kind), false), 47, true);
                b.interactable = !done && s.ActiveResearch < 0 && s.Level >= spec.Level && app.Simulation.Count(RoomKind.Library) > 0 && app.Simulation.CanOperate;
            }
        }
        private void Activities()
        {
            CardText(rightContent, app.T("NACHMITTAG AUF DEM CAMPUS", "AFTERNOONS ON CAMPUS"), app.T("AGs starten ab 15 Uhr. Sie verbessern Fächer und Wohlbefinden. Ausstattung wird einmal gekauft; laufende Kosten fallen nur bei aktivem Angebot mit vorhandenem Raum an.", "Clubs start at 15:00. They improve subjects and wellbeing. Equipment is purchased once; daily costs require an active club and its matching room."), 142);
            foreach (ClubSpec spec in AcademyCatalog.Clubs)
            {
                ClubKind kind = spec.Kind; bool active = app.State.Clubs.Contains(kind), available = app.Simulation.ClubAvailable(kind);
                CardText(rightContent, app.ClubName(kind), app.RoomName(spec.Room) + " · " + app.SubjectName(spec.Subject) + "\n" + spec.Daily + app.T(" €/Tag", " €/day") + (available ? "" : app.T(" · Raum fehlt", " · room missing")), 70);
                int setup = app.State.ClubLicenses.Contains(kind) ? 0 : spec.Setup;
                Button b = Button(rightContent, active ? app.T("AG pausieren", "Pause club") : app.T("AG starten · ", "Start club · ") + setup + " €", () => app.Act(app.Simulation.SetClub(kind, !active)), 47, active);
                b.interactable = app.Simulation.CanOperate && (active || available);
            }
        }
        private void AcademicYear()
        {
            var sim = app.Simulation; var s = app.State;
            CardText(rightContent, app.T("SCHULJAHR ", "SCHOOL YEAR ") + sim.SchoolYear, app.T("Tag ", "Day ") + sim.YearDay + "/60 · " + app.T("Abschnitt ", "Term ") + sim.Term + "/3\n" + app.SeasonName() + " · " + app.WeatherName(), 70);
            Progress(rightContent, sim.YearDay - 1, 60);
            Text(rightContent, app.T("Prüfungen an Tag 20, 40 und 60. Am Jahresende werden Schüler ab 45 % versetzt. Nach Jahrgang 6 folgen Abschluss und neue Aufnahme.", "Exams are held on days 20, 40 and 60. At year end pupils with at least 45% advance. Grade 6 leads to graduation and fresh admissions."), 16, 138, muted);
            Text(rightContent, s.ExamsHeld + app.T(" Prüfungen · ", " exams · ") + s.Graduates + app.T(" Abschlüsse", " graduates"), 18, 42, teal, true);
            Button(rightContent, app.T("Letzten Tagesbericht lesen", "Read the latest daily report"), () => ShowDayReport(), 48, true);
            Text(rightContent, app.T("ZEUGNISSE", "REPORT CARDS"), 17, 38, ink, true);
            foreach (TermReport report in s.Reports.AsEnumerable().Reverse())
            {
                TermReport r = report;
                Button(rightContent, app.T("Jahr ", "Year ") + r.Year + " · " + app.T("Abschnitt ", "Term ") + r.Term + "\n" + app.T("Durchschnitt ", "Average ") + r.Average + "% · " + r.Passed + "/" + r.Students, () => ShowTermReport(r), 68);
            }
            Text(rightContent, app.T("ERFOLGE & SONDERFÖRDERUNG", "ACHIEVEMENTS & BONUS GRANTS"), 16, 48, teal, true);
            string[] criteriaDe = { "Eine Prüfung durchführen.", "Zwei Forschungen abschließen.", "Drei AGs aktiv anbieten.", "Jahreszeugnis mit mindestens 70 %.", "Ersten Schüler zum Abschluss führen.", "Ein Klassenzimmer auf Stufe 2 ausbauen." };
            string[] criteriaEn = { "Hold an exam.", "Complete two research projects.", "Offer three active clubs.", "Year-end report averaging at least 70%.", "Guide your first pupil to graduation.", "Upgrade a classroom to level 2." };
            for (int i = 0; i < criteriaDe.Length; i++)
                CardText(rightContent, ((s.AchievementMask & (1 << i)) != 0 ? "✓ " : "○ ") + app.T(AcademyCatalog.AchievementDe[i], AcademyCatalog.AchievementEn[i]), app.T(criteriaDe[i], criteriaEn[i]) + "\n" + AcademyCatalog.AchievementRewards[i] + " €", 72);
        }
        private void ShowTermReport(TermReport report)
        {
            OpenModal(app.T("Zeugnis · Jahr ", "Report card · year ") + report.Year + " · " + report.Term, app.T("Durchschnitt ", "Average ") + report.Average + "%\n" + report.Passed + "/" + report.Students + app.T(" bestanden · mindestens 50 %", " passed · at least 50%") + "\n" + report.Graduated + app.T(" Abschlüsse · ", " graduates · ") + report.Repeated + app.T(" Wiederholungen", " repeating pupils"));
            for (int i = 0; i < 5; i++) { Text(modalBody, app.SubjectName((Subject)i) + " · " + report.SubjectAverages[i] + "%", 20, 40); Progress(modalBody, report.SubjectAverages[i], 100); }
            Button(modalBody, app.T("Förderbedarf ansehen", "Review support needs"), () => { pupilFilter = 1; pupilPage = 0; OpenTab(4); }, 51, true);
            Button(modalBody, app.T("Stundenpläne anpassen", "Adjust timetables"), () => OpenTab(2), 51);
            Button(modalBody, app.T("Zurück zum Campus", "Back to campus"), () => CloseModal(), 51);
        }
        public void ShowDayReport()
        {
            DailyReport d = app.State.DailyReport;
            if (d.Day == 0) { Notify(app.T("Der erste Tagesbericht erscheint nach dem ersten Schultag.", "The first daily report arrives after the first school day.")); return; }
            Ledger l = app.State.LastLedger;
            OpenModal(app.T("Schultag ", "School day ") + d.Day, app.T("Tagesbilanz ", "Daily balance ") + Signed(l.Net) + " €\n" + d.Attended + app.T(" anwesend · ", " attended · ") + d.Absent + app.T(" abwesend", " absent") + "\n" + d.Joined + app.T(" Aufnahmen · ", " admissions · ") + d.Left + app.T(" Abgänge / Abschlüsse", " departures / graduates"));
            if (d.Unstaffed > 0) Text(modalBody, d.Unstaffed + app.T(" Schüler hatten keine zugeordnete Lehrkraft.", " pupils had no assigned teacher."), 18, 60, CampusRenderer.ColorHex("BA725C"));
            if (d.ResearchGained > 0) Text(modalBody, "+" + d.ResearchGained + app.T(" Forschungspunkte", " research points"), 18, 40, teal);
            if (d.ResearchFinished >= 0) Text(modalBody, "✓ " + app.ResearchName((ResearchKind)d.ResearchFinished), 20, 62, teal, true);
            if (d.TermFinished > 0)
            {
                TermReport report = app.State.Reports.Last();
                Button(modalBody, app.T("Neues Zeugnis ansehen", "View the new report card"), () => ShowTermReport(report), 52, true);
            }
            for (int i = 0; i < 6; i++) if ((d.NewAchievements & (1 << i)) != 0)
                Text(modalBody, "✓ " + app.T(AcademyCatalog.AchievementDe[i], AcademyCatalog.AchievementEn[i]) + " · +" + AcademyCatalog.AchievementRewards[i] + " €", 18, 60, teal);
            foreach (int id in d.StrugglingPupils)
            {
                Pupil pupil = app.State.Pupils.FirstOrDefault(p => p.Id == id); if (pupil == null) continue;
                int target = id; Button(modalBody, app.T("Förderbedarf: ", "Needs support: ") + pupil.Name, () => ShowPupil(target), 46);
            }
            Button(modalBody, app.T("Schüler & Förderung", "Pupils & support"), () => OpenTab(4), 50, true);
            Button(modalBody, app.T("Budget prüfen", "Review budget"), () => OpenTab(3), 50);
            Button(modalBody, app.T("Weiter zum Campus", "Continue to campus"), () => CloseModal(), 50);
        }
    }
}

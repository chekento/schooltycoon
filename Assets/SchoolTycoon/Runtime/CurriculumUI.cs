using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoSch.SchoolTycoon.Core;
namespace KoSch.SchoolTycoon
{
    public sealed partial class SchoolUI
    {
        private int planDay,subjectPage,subjectGroup=-1,subjectRegion;private string subjectQuery="";
        private string DayName(int i){return app.T(new[]{"Mo","Di","Mi","Do","Fr"}[i],new[]{"Mon","Tue","Wed","Thu","Fri"}[i]);}
        private string GroupName(SubjectGroup g){return app.T(new[]{"Mathematik","Sprachen","Naturwissenschaft","Geisteswissenschaft","Gesellschaft","Kreativität","Informatik","Technik","Gesundheit","Sport"}[(int)g],g.ToString());}
        private string RegionName(int r){return app.T(new[]{"International","Deutschland","England","USA"}[r],new[]{"International","Germany","England","USA"}[r]);}
        private InputField InputBox(Transform parent,string initial,int limit=60)
        {RectTransform rect=Panel("Input",parent,CampusRenderer.ColorHex("E5ECE4"));Height(rect,58);InputField f=rect.gameObject.AddComponent<InputField>();f.characterLimit=limit;Text t=Text(rect,"",22,54);Fit(t.rectTransform,12);f.textComponent=t;f.text=initial;return f;}
        private void CurriculumOptions(Transform p,int room)
        {
            Text(p,app.T("WOCHENPLAN WÄHLEN","CHOOSE A WEEKLY PLAN"),16,38,teal,true);
            foreach(int region in Enumerable.Range(0,4)){int r=region;Button(p,RegionName(r),()=>{app.Act(app.Simulation.ApplyCurriculum((CurriculumRegion)r,room),false);if(room!=0)ShowClass(room);},45);}
        }
        private void SubjectPicker(int room,int slot)
        {
            OpenModal(app.T("Fach auswählen","Choose a subject"),app.T("146 Fächer, eigene Kurse und deutsche, englische oder US-Vorlagen. Fachräume verbessern Unterricht; ohne Fachraum findet Theorie im Klassenzimmer statt.","146 subjects, custom courses and German, English or US templates. Specialist rooms improve learning; theory remains available in classrooms."));
            InputField q=InputBox(modalBody,subjectQuery);Button(modalBody,app.T("Suchen","Search"),()=>{subjectQuery=q.text;subjectPage=0;SubjectPicker(room,slot);},45,true);
            Button(modalBody,subjectGroup<0?app.T("Alle Fachgruppen","All subject groups"):GroupName((SubjectGroup)subjectGroup),()=>{subjectGroup=(subjectGroup+2)%11-1;subjectPage=0;SubjectPicker(room,slot);},46);
            Button(modalBody,RegionName(subjectRegion),()=>{subjectRegion=(subjectRegion+1)%4;subjectPage=0;SubjectPicker(room,slot);},46);
            var courses=CurriculumCatalog.All(app.State).Where(c=>(subjectGroup<0||(int)c.Group==subjectGroup)&&(subjectRegion==0||(c.Regions&(1<<(subjectRegion-1)))!=0)&&(string.IsNullOrEmpty(subjectQuery)||c.NameDe.IndexOf(subjectQuery,StringComparison.OrdinalIgnoreCase)>=0||c.NameEn.IndexOf(subjectQuery,StringComparison.OrdinalIgnoreCase)>=0)).ToList();
            int pages=Math.Max(1,(courses.Count+11)/12);subjectPage=Math.Min(subjectPage,pages-1);Text(modalBody,courses.Count+app.T(" Fächer · Seite "," subjects · page ")+(subjectPage+1)+"/"+pages,16,34,muted);
            RectTransform row=Row(modalBody,48);Button(row,"‹",()=>{subjectPage=Math.Max(0,subjectPage-1);SubjectPicker(room,slot);});Button(row,"›",()=>{subjectPage=Math.Min(pages-1,subjectPage+1);SubjectPicker(room,slot);});
            foreach(SubjectSpec spec in courses.Skip(subjectPage*12).Take(12)){Subject sub=spec.Id;Button(modalBody,app.SubjectName(sub)+"\n"+GroupName(spec.Group)+" · "+app.RoomName(spec.Facility)+" · "+spec.Materials+app.T(" €/Platz und Stunde"," €/seat and lesson"),()=>{Result r=room==0?app.Simulation.SetLesson(slot,sub):app.Simulation.SetWeeklyLesson(room,planDay,slot,sub);if(!r.Success){Notify(app.Error(r.Code));return;}CloseModal();app.Act(r,false);if(room!=0)ShowClass(room);},72);}
            Button(modalBody,app.T("Eigenes Fach hinzufügen","Add a custom subject"),()=>CustomSubject(room,slot),48);
            Button(modalBody,app.T("Zurück","Back"),()=>{if(room==0)CloseModal();else ShowClass(room);},50);
        }
        private void CustomSubject(int room,int slot,int group=0,int facility=0,int difficulty=1,string de="",string en="")
        {
            RoomKind[] rooms={RoomKind.Classroom,RoomKind.LanguageLab,RoomKind.ScienceLab,RoomKind.Library,RoomKind.ArtRoom,RoomKind.MusicRoom,RoomKind.ComputerLab,RoomKind.Workshop,RoomKind.FoodLab,RoomKind.Theatre,RoomKind.Garden,RoomKind.Gym};
            OpenModal(app.T("Dein eigenes Unterrichtsfach","Your custom subject"),app.T("Für regionale Wahlfächer und weitere Sprachen. Bis zu 200 eigene Kurse werden im Spielstand gespeichert.","For regional electives and additional languages. Up to 200 custom courses are stored with the school."));
            Text(modalBody,"Deutsch",18,32);InputField nameDe=InputBox(modalBody,de);Text(modalBody,"English",18,32);InputField nameEn=InputBox(modalBody,en);
            Button(modalBody,GroupName((SubjectGroup)group),()=>CustomSubject(room,slot,(group+1)%10,facility,difficulty,nameDe.text,nameEn.text),48);
            Button(modalBody,app.RoomName(rooms[facility]),()=>CustomSubject(room,slot,group,(facility+1)%rooms.Length,difficulty,nameDe.text,nameEn.text),48);
            Button(modalBody,app.T("Anspruch ","Difficulty ")+difficulty+"/3",()=>CustomSubject(room,slot,group,facility,difficulty%3+1,nameDe.text,nameEn.text),48);
            Button(modalBody,app.T("Fach anlegen","Create subject"),()=>{Result r=app.Simulation.AddSubject(nameDe.text,nameEn.text,(SubjectGroup)group,rooms[facility],difficulty);if(!r.Success){Notify(app.Error(r.Code));return;}app.Act(r,false);subjectQuery="";subjectGroup=-1;SubjectPicker(room,slot);},52,true);
            Button(modalBody,app.T("Zurück","Back"),()=>SubjectPicker(room,slot),50);
        }
        private void CareerProjects()
        {
            var sim=app.Simulation;var s=app.State;Text(rightContent,app.T("DEINE SCHULVISION","YOUR SCHOOL VISION"),18,42,teal,true);
            Button(rightContent,app.T("Abschluss nach Jahrgang ","Graduate after grade ")+s.GraduationGrade,()=>app.Act(sim.SetSchoolPath(s.GraduationGrade==6?12:s.GraduationGrade==12?13:6),false),54);
            string[] detailsDe={"Mindestens 7 Fachgruppen und 65 Lernen.","3 fortgebildete Lehrkräfte und 4 verschiedene Fachräume.","4 Klassen, 72 Schüler und ein Klassenraum im 2. Obergeschoss.","48 Schüler, 80 Freude, 80 Sauberkeit und aktive Förderung."};
            string[] detailsEn={"At least 7 subject groups and 65 learning.","3 trained teachers and 4 different specialist facilities.","4 classes, 72 pupils and a classroom on the third floor.","48 pupils, 80 happiness, 80 cleanliness and active support."};
            Text(rightContent,app.T("Halte die Ziele zehn Tage in Folge bei positiver Tagesbilanz. Ein Projekt pro Schuljahr. Ausbau allein genügt nicht: die Schule muss nachhaltig funktionieren.","Meet the goals for ten consecutive days with a positive daily forecast. One project per school year. Expansion needs a sustainable school."),16,120,muted);
            if(s.ActiveContract>=0){Text(rightContent,app.T(SchoolSimulation.CareerDe[s.ActiveContract],SchoolSimulation.CareerEn[s.ActiveContract])+" · "+s.ContractProgress+"/10",18,48,teal,true);Text(rightContent,app.T(detailsDe[s.ActiveContract],detailsEn[s.ActiveContract]),16,70);}
            else foreach(int choice in sim.CareerChoices()){int kind=choice;Button b=Button(rightContent,app.T(SchoolSimulation.CareerDe[kind],SchoolSimulation.CareerEn[kind])+" · "+sim.CareerCost+" €",()=>app.Act(sim.StartCareer(kind),false),54,true);b.interactable=s.Level>=1&&s.ContractYear!=sim.SchoolYear&&sim.YearDay<=51&&sim.CanOperate;Text(rightContent,app.T(detailsDe[kind],detailsEn[kind]),15,76,muted);}
            Text(rightContent,app.T("Prestige ","Prestige ")+s.Prestige+" · "+app.T("Projektprämie ","Project grant ")+sim.CareerReward+" €",17,44);
        }
        private void WelcomeArt(Transform parent)
        {
            Texture2D tex=Resources.Load<Texture2D>("Art/CampusWelcome");if(tex==null)return;RectTransform r=Rect("Campus welcome art",parent);Height(r,115);r.gameObject.AddComponent<RectMask2D>();RectTransform pic=Rect("Welcome image",r);Fit(pic);RawImage img=pic.gameObject.AddComponent<RawImage>();img.texture=tex;img.uvRect=new Rect(0,.12f,1,.66f);img.raycastTarget=false;
        }
    }
}

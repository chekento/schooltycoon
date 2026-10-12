using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using KoSch.SchoolTycoon.Core;
namespace KoSch.SchoolTycoon
{
    public sealed partial class SchoolUI
    {
        private bool buildOpen,manageOpen=true;private GameMode menuMode;
        public void Dismiss(){CloseModal();}
        public void ShowMainMenu()
        {
            app.InMainMenu=true;app.Paused=true;CloseModal();if(screen!=null)screen.SetActive(false);
            modal=Panel("Rektor main menu",safe,CampusRenderer.ColorHex("284B48"));Fit(modal);
            Texture2D art=Resources.Load<Texture2D>("Art/CampusWelcome");if(art!=null){RectTransform pic=Rect("Dream campus",modal);Fit(pic);RawImage img=pic.gameObject.AddComponent<RawImage>();img.texture=art;img.raycastTarget=false;img.uvRect=new Rect(0,0,1,1);}
            RectTransform shade=Panel("Quiet backdrop",modal,new Color(.08f,.19f,.18f,.28f));Fit(shade);
            RectTransform identity=Rect("Rektor identity",modal);identity.anchorMin=new Vector2(.04f,.35f);identity.anchorMax=new Vector2(.53f,.9f);identity.offsetMin=identity.offsetMax=Vector2.zero;VStack(identity,20,12);
            Text(identity,"REKTOR",82,112,Color.white,true);Text(identity,app.T("Deine Schule beginnt mit einer Idee.","Your school begins with an idea."),30,92,CampusRenderer.ColorHex("FFEFCD"),true);
            Text(identity,app.T("Vom ersten Klassenzimmer zum lebendigen Campus. Begleite Schüler, fördere dein Team und gestalte eine Schule mit Zukunft.","From one classroom to a thriving campus. Guide pupils, support your team and build a school with a future."),24,150,Color.white);
            RectTransform card=Panel("Main menu choices",modal,paper);card.anchorMin=new Vector2(.59f,.15f);card.anchorMax=new Vector2(.95f,.85f);card.offsetMin=card.offsetMax=Vector2.zero;
            RectTransform body=Scroll(card,0,0);Text(body,"REKTOR",20,38,teal,true);Text(body,app.T("Willkommen zurück","Welcome back"),34,72,ink,true);
            Button(body,app.T("Spielen · deine Spielstände","Play · your saved schools"),()=>ShowSlots(GameMode.Campaign),76,true);
            Button(body,app.T("Sandbox · unendlich Geld","Sandbox · unlimited funds"),()=>ShowSlots(GameMode.Sandbox),76);
            Button(body,app.T("Einstellungen","Settings"),()=>ShowSettings(true),68);
            Text(body,app.T("6 unabhängige Spielstände · 3 Schwierigkeitsstufen\n146 Fächer · aufrüstbare Räume · Schulgeschichten","6 independent saves · 3 difficulty levels\n146 subjects · room equipment · school stories"),18,116,muted);
            if(!Application.isMobilePlatform)Button(body,app.T("Spiel beenden","Quit game"),()=>Application.Quit(),56);
        }
        private void ShowSlots(GameMode mode)
        {
            menuMode=mode;OpenModal(mode==GameMode.Sandbox?app.T("Deine Sandbox-Campus","Your sandbox schools"):app.T("Deine Spielstände","Your saved schools"),app.T("Wähle einen Platz. Dort kannst du eine Schule fortsetzen oder mit einer von drei Schwierigkeitsstufen neu beginnen. Jeder Platz besitzt eine eigene Sicherung.","Choose a slot. Continue an existing school or start a new one with one of three difficulty levels. Each slot has its own backup."));
            for(int i=1;i<=SaveArchive.Slots;i++)
            {
                int slot=i;string error;SchoolState state=SchoolSave.Archive.Load(slot,out error);
                string details=state==null?(SchoolSave.Archive.HasData(slot)?app.T("Nicht lesbar · sichern oder neu beginnen","Unreadable · recover or start again"):app.T("Freier Platz","Empty slot")):state.SchoolName+"\n"+app.T("Tag ","Day ")+state.Day+" · "+state.Students+app.T(" Schüler"," pupils")+" · "+(state.Mode==GameMode.Sandbox?"Sandbox":app.T(RektorCatalog.Difficulties[(int)state.Difficulty].NameDe,RektorCatalog.Difficulties[(int)state.Difficulty].NameEn))+(error=="backup"?app.T(" · Sicherung"," · backup"):"");
                Button(modalBody,app.T("Spielstand ","Save slot ")+slot+" · "+details,()=>ShowSlot(slot,mode),92,state!=null);
            }
            Button(modalBody,app.T("Hauptmenü","Main menu"),()=>ShowMainMenu(),58);
        }
        private void ShowSlot(int slot,GameMode mode)
        {
            string error;SchoolState existing=SchoolSave.Archive.Load(slot,out error);
            OpenModal(app.T("Spielstand ","Save slot ")+slot,app.T("Drei Wege für einen neuen Campus. Bei bestehenden Schulen bleibt die gewählte Schwierigkeit beim Fortsetzen erhalten.","Three ways to start a new campus. Continuing a school preserves its saved difficulty."));
            if(existing!=null)Button(modalBody,app.T("Fortsetzen: ","Continue: ")+existing.SchoolName,()=>app.LoadSlot(slot),64,true);
            Text(modalBody,app.T("Name deiner neuen Schule","Name of your new school"),18,35);InputField school=InputBox(modalBody,app.T("Mein Campus ","My campus ")+slot,64);
            foreach(DifficultySpec spec in RektorCatalog.Difficulties)
            {
                Difficulty difficulty=spec.Id;string desc=difficulty==Difficulty.Relaxed?app.T("Mehr Reserven, günstigere Käufe und Zeit zum Lernen.","More reserves, lower prices and time to learn."):difficulty==Difficulty.Standard?app.T("Ausgewogene Förderung und Kosten. Plane schrittweise.","Balanced funding and costs. Develop step by step."):app.T("Knappe Reserven und höhere Kosten. Besetzte Klassen und Budget zählen.","Limited reserves and higher costs. Staffing and budget matter.");
                Button(modalBody,app.T(spec.NameDe,spec.NameEn)+"\n"+desc+"\n"+(mode==GameMode.Sandbox?app.T("Unendlich Geld · freie Freischaltungen","Unlimited funds · free unlocks"):spec.StartCash.ToString("N0")+app.T(" € Startkapital"," starting funds")),()=>
                {string name=school.text;Action start=()=>app.StartNew(slot,difficulty,mode,name);if(SchoolSave.Archive.HasData(slot))Confirm(app.T("Spielstand ersetzen?","Replace this save?"),app.T("Nur Platz ","Only slot ")+slot+app.T(" wird durch die neue Schule ersetzt. Andere Schulen bleiben erhalten."," will be replaced by the new school. Other schools remain saved."),start);else start();},108,true);
            }
            if(SchoolSave.Archive.HasData(slot))Button(modalBody,app.T("Diesen Spielstand löschen","Delete this save"),()=>Confirm(app.T("Spielstand und Sicherung löschen?","Delete this save and backup?"),app.T("Nur Spielstand ","Only save slot ")+slot,()=>{SchoolSave.Archive.Delete(slot);ShowSlots(mode);}),54);
            Button(modalBody,app.T("Zurück zur Auswahl","Back to saved schools"),()=>ShowSlots(mode),58);
        }
        private void SliderSetting(Transform parent,string label,float value,float min,float max,Action<float> change)
        {
            Text(parent,label,19,36);RectTransform holder=Panel("Slider area",parent,CampusRenderer.ColorHex("E3EAE1"));Height(holder,58);RectTransform track=Panel("Slider track",holder,CampusRenderer.ColorHex("CBDACF"));Fit(track,18);track.offsetMin=new Vector2(18,22);track.offsetMax=new Vector2(-18,-22);
            RectTransform handle=Panel("Thumb",holder,teal);handle.anchorMin=handle.anchorMax=new Vector2(0,.5f);handle.sizeDelta=new Vector2(32,42);var slider=holder.gameObject.AddComponent<Slider>();slider.minValue=min;slider.maxValue=max;slider.wholeNumbers=false;slider.handleRect=handle;slider.targetGraphic=handle.GetComponent<Image>();slider.value=value;slider.onValueChanged.AddListener(v=>{change(v);app.Settings.Save();app.Settings.Apply(app);});
        }
        private void ShowSettings(bool main)
        {
            OpenModal(app.T("Einstellungen","Settings"),app.T("Deine Einstellungen gelten für alle Spielstände. Die Grafikstufe kann jederzeit angepasst werden.","Settings apply to every save. You can change visual quality at any time."));
            Button(modalBody,app.T("Sprache: Deutsch","Language: English"),()=>{app.ToggleLanguage();ShowSettings(main);},55);
            SliderSetting(modalBody,app.T("Musik","Music"),app.Settings.Music,0,1,v=>app.Settings.Music=v);SliderSetting(modalBody,app.T("Effekte","Effects"),app.Settings.Effects,0,1,v=>app.Settings.Effects=v);
            SliderSetting(modalBody,app.T("Kamera-Empfindlichkeit","Camera sensitivity"),app.Settings.CameraSensitivity,.5f,2,v=>app.Settings.CameraSensitivity=v);
            string[] qualityDe={"Sparsam","Ausgewogen","Schönste Grafik"},qualityEn={"Low","Balanced","High"};
            Button(modalBody,app.T("Grafik: ","Graphics: ")+app.T(qualityDe[app.Settings.Graphics],qualityEn[app.Settings.Graphics]),()=>{app.Settings.Graphics=(app.Settings.Graphics+1)%3;app.Settings.Save();app.Settings.Apply(app);app.Campus.Rebuild();ShowSettings(main);},56);
            Button(modalBody,app.Settings.FrameRate+" FPS",()=>{app.Settings.FrameRate=app.Settings.FrameRate==30?60:30;app.Settings.Save();app.Settings.Apply(app);ShowSettings(main);},52);
            Button(modalBody,app.T("Weniger Bewegung: ","Reduced motion: ")+(app.Settings.ReducedMotion?"✓":"○"),()=>{app.Settings.ReducedMotion=!app.Settings.ReducedMotion;app.Settings.Save();app.Settings.Apply(app);ShowSettings(main);},55);
            Button(modalBody,app.T("Raumnamen: ","Room names: ")+(app.Settings.Labels?"✓":"○"),()=>{app.Settings.Labels=!app.Settings.Labels;app.Settings.Save();app.Settings.Apply(app);ShowSettings(main);},55);
            Button(modalBody,app.T("Automatisch speichern: ","Autosave: ")+(app.Settings.Autosave?"✓":"○"),()=>{app.Settings.Autosave=!app.Settings.Autosave;app.Settings.Save();ShowSettings(main);},55);
            Button(modalBody,app.T("Zurück","Back"),()=>{if(main)ShowMainMenu();else{CloseModal();Refresh();}},58,true);
        }
        private void RoomShop(int roomId)
        {
            Room r=app.State.Rooms.FirstOrDefault(x=>x.Id==roomId);if(r==null)return;
            OpenModal(app.RoomName(r.Kind)+app.T(" · Ausbaustufe "," · room tier ")+app.Simulation.RoomTier(r)+"/5",app.T("Kaufe einzelne Gegenstände. Nützliche Ausstattung verbessert Lernen, Wohlbefinden und Hygiene. Dekoration folgt erst nach den funktionalen Käufen.","Purchase individual items. Functional equipment improves learning, wellbeing and hygiene. Decoration follows functional purchases."));
            foreach(ItemSpec spec in RektorCatalog.For(r.Kind).OrderBy(i=>i.Decoration).ThenBy(i=>i.Day))
            {
                RoomItem item=spec.Id;bool owned=r.Items.Contains(item);Result availability=app.Simulation.CanBuyItem(r,item);string name=app.T(spec.NameDe,spec.NameEn);
                Text(modalBody,(owned?"✓ ":"")+name,21,52,owned?teal:ink,true);
                string effect=app.T("Lernen +","Learning +")+spec.Learning+app.T(" · Freude +"," · Joy +")+spec.Happiness+app.T(" · Wohlbefinden +"," · Wellbeing +")+spec.Wellbeing+app.T(" · Hygiene +"," · Hygiene +")+spec.Cleanliness+"\n"+spec.Daily+app.T(" €/Tag Betrieb"," €/day upkeep");Text(modalBody,effect,16,68,muted);
                if(!owned){string requirement=app.T("Ab Tag ","From day ")+spec.Day+(spec.Research.HasValue?" · "+app.ResearchName(spec.Research.Value):"");if(spec.Decoration)requirement+=app.T(" · funktionale Ausstattung komplett"," · all functional equipment owned");Text(modalBody,requirement,16,54,muted);Button b=Button(modalBody,availability.Success?app.T("Kaufen · ","Purchase · ")+app.Simulation.ItemCost(item)+" €":app.Error(availability.Code),()=>{app.Act(app.Simulation.BuyRoomItem(roomId,item));RoomShop(roomId);},62,availability.Success);b.interactable=availability.Success;}
            }
            Button(modalBody,app.T("Zurück zum Campus","Back to campus"),()=>CloseModal(),58);
        }
        private void StoryGuide()
        {
            var sim=app.Simulation;var state=app.State;if(!sim.Modern||sim.Sandbox)return;
            int chapter=Math.Min(11,state.StoryChapter);CardText(rightContent,app.T("MARA · DEINE BEGLEITUNG","MARA · YOUR GUIDE"),app.T(SchoolSimulation.StoryDe[chapter],SchoolSimulation.StoryEn[chapter]),60);
            Text(rightContent,sim.StoryAdvice(),17,174,ink);Progress(rightContent,state.StoryChapter,12);
            Button(rightContent,app.T("Nächsten Schritt ansehen","Show the next step"),()=>ShowStory(),52,true);
        }
        private void ShowStory()
        {
            var sim=app.Simulation;int chapter=Math.Min(11,app.State.StoryChapter);OpenModal(app.T(SchoolSimulation.StoryDe[chapter],SchoolSimulation.StoryEn[chapter]),sim.StoryAdvice());
            Text(modalBody,app.State.StoryChapter+"/12 · "+app.T("Kapitel abgeschlossen","chapters completed"),18,45,teal,true);
            foreach(var pair in new[]{new{Label=app.T("Team organisieren","Manage staff"),Tab=1},new{Label=app.T("Klassen & Unterricht","Classes and lessons"),Tab=2},new{Label=app.T("Budget prüfen","Review budget"),Tab=3},new{Label=app.T("Schüler fördern","Support pupils"),Tab=4},new{Label=app.T("Schulentwicklung","School development"),Tab=5},new{Label=app.T("Jahresvision","Annual vision"),Tab=7}}){int tabIndex=pair.Tab;Button(modalBody,pair.Label,()=>{manageOpen=true;OpenTab(tabIndex);},56);}
            if(app.State.Cash<0&&sim.Modern&&!sim.Sandbox)Button(modalBody,app.T("Übergangshilfe beantragen · 1.500 €","Request bridge funding · €1,500"),()=>{app.Act(sim.FinancialBridge(),false);ShowStory();},58);
            Button(modalBody,app.T("Campus erweitern","Expand the campus"),()=>{buildOpen=true;CloseModal();Refresh();},56,true);
            Button(modalBody,app.T("Weiter planen","Keep planning"),()=>CloseModal(),56);
        }
    }
}

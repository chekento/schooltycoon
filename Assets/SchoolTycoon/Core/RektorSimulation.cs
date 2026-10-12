using System;
using System.Collections.Generic;
using System.Linq;
namespace KoSch.SchoolTycoon.Core
{
    public sealed partial class SchoolSimulation
    {
        public bool Modern { get { return State.RulesVersion==1; } }
        public bool Sandbox { get { return State.Mode==GameMode.Sandbox; } }
        public DifficultySpec Rules { get { return RektorCatalog.Difficulties[(int)State.Difficulty]; } }
        public static SchoolState CreateCampaign(Difficulty difficulty,GameMode mode=GameMode.Campaign)
        {
            if(!Enum.IsDefined(typeof(Difficulty),difficulty)||!Enum.IsDefined(typeof(GameMode),mode))throw new ArgumentOutOfRangeException();
            SchoolState s=SchoolState.Create();s.RulesVersion=1;s.Difficulty=difficulty;s.Mode=mode;s.Cash=mode==GameMode.Sandbox?0:RektorCatalog.Difficulties[(int)difficulty].StartCash;
            foreach(Room r in s.Rooms){r.FurnishingVersion=1;r.Items=new List<RoomItem>();}
            new SchoolSimulation(s).ApplyCurriculum(CurriculumRegion.Germany);return s;
        }
        public int Price(int amount) { return Modern?(amount*Rules.PricePercent+99)/100:amount; }
        public bool CanAfford(int amount) { return Sandbox||amount==0||State.Cash>=amount; }
        public int EducationFunding(int pupils)
        {if(!Modern)return pupils*28;int first=Math.Min(48,pupils),middle=Math.Min(48,Math.Max(0,pupils-48)),rest=Math.Max(0,pupils-96);return first*Rules.Funding+middle*Rules.Funding*80/100+rest*Rules.Funding*65/100;}
        private void Spend(int amount) { if(!Sandbox)State.Cash-=amount; }
        private void Earn(int amount) { if(!Sandbox)State.Cash=(int)Math.Max(-1000000,Math.Min(100000000,(long)State.Cash+amount)); }
        public int AllowedFloor
        {get{return !Modern||Sandbox?5:State.Day>=360?5:State.Day>=240?4:State.Day>=150?3:State.Day>=80?2:State.Day>=35?1:0;}}
        public int UnlockDay(RoomKind kind)
        {
            if(!Modern||Sandbox)return 1;
            switch(kind){case RoomKind.Classroom:return 8;case RoomKind.Canteen:return 10;case RoomKind.Library:return 20;case RoomKind.LanguageLab:return 30;case RoomKind.Staircase:return 35;case RoomKind.ComputerLab:return 45;case RoomKind.FoodLab:return 45;case RoomKind.MusicRoom:return 50;case RoomKind.ScienceLab:return 60;case RoomKind.ArtRoom:return 75;case RoomKind.Workshop:return 95;case RoomKind.Gym:return 110;case RoomKind.Theatre:return 165;default:return 1;}
        }
        public bool RoomUnlocked(RoomKind kind,int floor=0)
        {
            if(Sandbox)return true;if(!Modern)return State.Level>=Catalog.Get(kind).UnlockLevel&&(floor==0||State.Level>=1);
            if(State.Day<UnlockDay(kind)||State.Level<Catalog.Get(kind).UnlockLevel||floor>AllowedFloor)return false;
            if(floor>=2&&!Researched(ResearchKind.GreenCampus))return false;
            if(kind==RoomKind.Library&&State.Students<24)return false;
            if(kind==RoomKind.ComputerLab||kind==RoomKind.Workshop)return Researched(ResearchKind.DigitalLearning);
            if(kind==RoomKind.Theatre)return Researched(ResearchKind.CreativeCampus);
            if(kind==RoomKind.ScienceLab)return State.Staff.Any(e=>e.Role==StaffRole.Teacher&&e.TrainingLevel>0);
            return true;
        }
        public string RoomRequirement(RoomKind kind,int floor=0)
        {
            string extra=kind==RoomKind.Library?" · 24 Schüler":kind==RoomKind.ComputerLab||kind==RoomKind.Workshop?" · Digitale Lernwerkstatt":kind==RoomKind.Theatre?" · Kreativer Campus":kind==RoomKind.ScienceLab?" · fortgebildete Lehrkraft":"";
            return State.Language=="de"?"Ab Tag "+UnlockDay(kind)+" · Schulstufe "+Catalog.Get(kind).UnlockLevel+extra+(floor>AllowedFloor?" · höhere Geschosse später":"")+(floor>=2?" · Nachhaltiger Campus":""):
                "From day "+UnlockDay(kind)+" · school level "+Catalog.Get(kind).UnlockLevel+(kind==RoomKind.Library?" · 24 pupils":kind==RoomKind.ComputerLab||kind==RoomKind.Workshop?" · Digital learning":kind==RoomKind.Theatre?" · Creative campus":kind==RoomKind.ScienceLab?" · trained teacher":"")+(floor>=2?" · Green campus":"");
        }
        public bool SubjectUnlocked(Subject sub)
        {SubjectSpec c=CurriculumCatalog.Get(sub,State);return c!=null&&(!Modern||Sandbox||c.Difficulty<3||State.Day>=(c.NameEn.StartsWith("AP ")?150:75)&&State.Learning>=(c.NameEn.StartsWith("AP ")?65:60)&&(!c.NameEn.StartsWith("AP ")||Researched(ResearchKind.DigitalLearning)));}
        public int ResearchRequired(ResearchKind kind) { return AcademyCatalog.Research[(int)kind].Points*(Modern?4:1); }
        public int ResearchStartDay(ResearchKind kind) { return !Modern||Sandbox?1:kind==ResearchKind.GreenCampus?30:kind==ResearchKind.DigitalLearning?40:kind==ResearchKind.InclusiveLearning?90:140; }
        public int ItemCost(RoomItem item){return Price(RektorCatalog.Get(item).Cost);}
        public int RoomTier(Room r)
        {if(r.FurnishingVersion==0)return 1+r.UpgradeLevel;var functional=RektorCatalog.For(r.Kind).Where(i=>!i.Decoration).ToList();if(functional.Count==0)return 1;return r.Items.Any(i=>RektorCatalog.Get(i).Decoration)?5:1+Math.Min(3,r.Items.Count(i=>!RektorCatalog.Get(i).Decoration)*4/(functional.Count+1));}
        public Result CanBuyItem(Room r,RoomItem item)
        {
            if(!CanOperate)return Result.Fail("blocked");ItemSpec spec=RektorCatalog.Get(item);
            if(r==null||r.FurnishingVersion!=1||spec==null||!RektorCatalog.For(r.Kind).Any(i=>i.Id==item))return Result.Fail("invalid");if(r.Items.Contains(item))return Result.Fail("done");
            if(!Sandbox)
            {
                if(State.Day<spec.Day||spec.Research.HasValue&&!Researched(spec.Research.Value))return Result.Fail("locked");
                if(spec.Day>=90&&!State.Staff.Any(e=>e.Role==StaffRole.Teacher&&e.TrainingLevel>0))return Result.Fail("locked");
            }
            if(item==RoomItem.DeskRow2&&!r.Items.Contains(RoomItem.DeskRow1)||item==RoomItem.DeskRow3&&!r.Items.Contains(RoomItem.DeskRow2))return Result.Fail("prerequisite");
            if(spec.Decoration&&RektorCatalog.For(r.Kind).Any(i=>!i.Decoration&&!r.Items.Contains(i.Id)))return Result.Fail("decorationlast");
            if(item==RoomItem.WallArt&&!r.Items.Contains(RoomItem.Plants)||item==RoomItem.SignatureDecor&&!r.Items.Contains(RoomItem.WallArt))return Result.Fail("prerequisite");
            return CanAfford(ItemCost(item))?Result.Ok():Result.Fail("funds");
        }
        public Result BuyRoomItem(int roomId,RoomItem item)
        {
            Room r=State.Rooms.FirstOrDefault(x=>x.Id==roomId);Result check=CanBuyItem(r,item);if(!check.Success)return check;
            Spend(ItemCost(item));r.Items.Add(item);int total=RektorCatalog.For(r.Kind).Count(i=>!i.Decoration),owned=r.Items.Count(i=>!RektorCatalog.Get(i).Decoration);r.UpgradeLevel=Math.Min(2,owned*3/(total+1));r.Condition=Math.Min(100,r.Condition+5);PlacePupils(false);CheckStory();CheckAchievements();return Result.Ok();
        }
        private Result UpgradeNextItem(Room r)
        {
            var available=RektorCatalog.For(r.Kind).Where(i=>!r.Items.Contains(i.Id)).OrderBy(i=>i.Decoration).ThenBy(i=>i.Day).ToList();if(available.Count==0)return Result.Fail("maxed");
            ItemSpec next=available.FirstOrDefault(i=>CanBuyItem(r,i.Id).Success);return next==null?CanBuyItem(r,available[0].Id):BuyRoomItem(r.Id,next.Id);
        }
        public int RoomLearning(Room r)
        {return r.FurnishingVersion==0?r.UpgradeLevel*3:Math.Min(14,r.Items.Sum(i=>RektorCatalog.Get(i).Learning));}
        public int RoomComfort(Room r)
        {return r.FurnishingVersion==0?0:Math.Min(10,r.Items.Sum(i=>RektorCatalog.Get(i).Happiness));}
        public int RoomSupport(Room r)
        {return r.FurnishingVersion==0?0:Math.Min(7,r.Items.Sum(i=>RektorCatalog.Get(i).Wellbeing));}
        private int PupilRoomAverage(Func<Room,int> score)
        {return State.Pupils.Count==0?0:(int)State.Pupils.Average(p=>{Room r=State.Rooms.FirstOrDefault(x=>x.Id==p.ClassRoomId);return r==null?0:score(r);});}
        private int StaffRestBonus {get{return Math.Min(4,State.Rooms.Where(r=>r.Kind==RoomKind.Staffroom).Sum(RoomSupport)/2);}}
        private int EquipmentCleanBonus {get{return Math.Min(6,State.Rooms.Where(r=>r.FurnishingVersion==1&&r.Kind==RoomKind.Toilet).Sum(r=>r.Items.Sum(i=>RektorCatalog.Get(i).Cleanliness)));}}
        private int EquipmentHappyBonus {get{return PupilRoomAverage(RoomComfort)+Math.Min(6,State.Rooms.Where(r=>r.Kind==RoomKind.Canteen||r.Kind==RoomKind.Garden).Sum(RoomComfort));}}
        public static readonly string[] StoryDe={"Ein Schlüssel, ein Anfang","Ein guter Schultag","Weitere Stimmen","Zusammen am Mittagstisch","Ein Ort für Neugier","Mehr als eine Tafel","Über den Hof hinaus","Talente entdecken","Ein Jahr voller Geschichten","Unser Stadtteilcampus","Wege ins Leben","Deine eigene Schulvision"};
        public static readonly string[] StoryEn={"A key and a beginning","A good school day","More voices","Lunch together","A place for curiosity","Beyond the chalkboard","Beyond the courtyard","Discovering talents","A year of stories","Our neighbourhood campus","Paths into life","Your own school vision"};
        public static readonly int[] StoryRewards={300,600,700,900,1100,900,1400,1700,2000,2200,2500,3500};
        public string StoryAdvice()
        {
            if(ServedStudents<State.Students)return State.Language=="de"?"Mara: Deinen Schülern fehlt Unterricht. Stelle eine Lehrkraft ein und ordne sie der Klasse zu.":"Mara: Pupils need lessons. Hire a teacher and assign them to their class.";
            if(State.Cleanliness<70)return State.Language=="de"?"Mara: Der Campus wächst schneller als der Hausdienst. Stelle zusätzliche Hauskräfte ein oder bilde sie fort. Gute Hygiene stärkt die Zufriedenheit.":"Mara: The campus is outgrowing its caretakers. Hire or train more caretakers. Good hygiene supports happiness.";
            int toilets=State.Rooms.Where(r=>r.Kind==RoomKind.Toilet).Sum(r=>48+r.UpgradeLevel*12);if(toilets<State.Students)return State.Language=="de"?"Mara: Für "+State.Students+" Schüler reichen die Sanitärplätze noch nicht. Baue weitere Sanitärräume oder verbessere ihre Ausstattung.":"Mara: Restroom capacity is below "+State.Students+" pupils. Build more restrooms or improve their equipment.";
            int meals=State.Rooms.Where(r=>r.Kind==RoomKind.Canteen).Sum(r=>72+r.UpgradeLevel*12);if(State.StoryChapter>=4&&meals<State.Students)return State.Language=="de"?"Mara: Die Mensa versorgt "+meals+" von "+State.Students+" Schülern. Eine weitere Essensausgabe oder Mensa verhindert lange Wartezeiten.":"Mara: Lunch serves "+meals+" of "+State.Students+" pupils. Expand food service to prevent long queues.";
            Ledger f=Forecast();if(f.Income<f.Expenses)return State.Language=="de"?"Mara: Prüfe zuerst das Budget. Besetze freie Klassen, pausiere teure AGs und wähle sparsamere Materialien, bevor du weiterbaust.":"Mara: Review the budget first. Staff vacant classes, pause costly clubs and use fewer supplies before expanding.";
            string[] de={"Stelle deine erste Lehrkraft ein. Pausiere zum Planen; Kosten werden erst am Tagesende abgerechnet.","Baue Sanitärraum und Lehrerzimmer. Überstehe drei Tage mit betreuten Schülern und mindestens 60 Freude.","Ab Tag 8: ein zweites Klassenzimmer und eine zweite Lehrkraft. Ein Raum ohne Lehrkraft bringt keine Bildungsförderung.","Ab Tag 10: Mensa und mindestens 24 Schüler. Mehr Plätze brauchen genug Toiletten und Hausdienst.","Ab Tag 20: Bibliothek und erstes Zeugnis. Lies den Förderbedarf einzelner Schüler, bevor du den Stundenplan änderst.","Kaufe drei nützliche Raum-Items und bilde eine Lehrkraft fort. Geräte und gute Lernbedingungen kommen vor Dekoration.","Ab Tag 35: Treppenhaus, passendes Treppenhaus darüber, Flur und Klassenzimmer. Jede obere Fläche braucht einen Raum darunter.","Bis Tag 90: Labor, Atelier und zwei abgeschlossene Forschungen. Biete mindestens sieben Fachgruppen im Wochenplan an.","Ab Tag 120: zwei aktive AGs, ein Jahreszeugnis mit mindestens 65 und zehn Tage positive Bilanz in Folge.","Ab Tag 180: betreue 96 Schüler, eröffne eine Klasse im 2. Obergeschoss und schließe ein Jahresprojekt ab.","Begleite mindestens drei Schüler zum Abschluss. Prüfungen, Stress, Förderung und Jahrgangswechsel entscheiden über ihren Weg.","Erreiche Prestige 3 und schließe alle vier Forschungen ab. Danach bestimmen wechselnde Jahresprojekte deine Schulvision."};
            string[] en={"Hire your first teacher. Pause while planning; running costs settle at day end.","Build a restroom and staffroom. Complete three days with staffed pupils and at least 60 happiness.","From day 8: a second classroom and teacher. Unstaffed rooms earn no education funding.","From day 10: a canteen and 24 pupils. More seats need enough toilets and caretakers.","From day 20: a library and first report card. Review individual support needs before changing timetables.","Buy three functional room items and train a teacher. Useful equipment comes before decoration.","From day 35: build aligned stairwells, an upper corridor and classroom. Every upper tile needs support below.","By day 90: a lab, art studio and two research projects. Offer at least seven weekly subject groups.","From day 120: two active clubs, a year-end report averaging 65 and ten consecutive profitable days.","From day 180: staff 96 pupils, open a third-floor class and complete an annual project.","Guide at least three pupils to graduation. Exams, stress, support and promotion shape their path.","Reach prestige 3 and complete all four research projects. Rotating annual projects then guide your vision."};
            return "Mara: "+(State.Language=="de"?de:en)[Math.Min(State.StoryChapter,11)];
        }
        public void CheckStory()
        {
            if(!Modern||Sandbox)return;
            while(State.StoryChapter<12)
            {
                bool done=false;switch(State.StoryChapter)
                {
                    case 0:done=Teachers>0;break;
                    case 1:done=Count(RoomKind.Toilet)>0&&Count(RoomKind.Staffroom)>0&&State.Day>=4&&State.Happiness>=60;break;
                    case 2:done=Count(RoomKind.Classroom)>=2&&Teachers>=2;break;
                    case 3:done=Count(RoomKind.Canteen)>0&&State.Students>=24;break;
                    case 4:done=Count(RoomKind.Library)>0&&State.ExamsHeld>0;break;
                    case 5:done=State.Rooms.Sum(r=>r.Items.Count(i=>!RektorCatalog.Get(i).Decoration))>=3&&State.Staff.Any(e=>e.Role==StaffRole.Teacher&&e.TrainingLevel>0);break;
                    case 6:done=State.Classes.Any(c=>State.Rooms.First(r=>r.Id==c.RoomId).Floor>0);break;
                    case 7:done=State.Day>=90&&Count(RoomKind.ScienceLab)>0&&Count(RoomKind.ArtRoom)>0&&State.CompletedResearch.Count>=2&&State.Classes.SelectMany(c=>c.Week).Select(s=>CurriculumCatalog.Get(s,State).Group).Distinct().Count()>=7;break;
                    case 8:done=State.Day>=120&&ActiveClubCount>=2&&State.Reports.Any(r=>r.Term==3&&r.Average>=65)&&State.ConsecutivePositive>=10;break;
                    case 9:done=State.Day>=180&&ServedStudents>=96&&State.Prestige>0&&State.Classes.Any(c=>State.Rooms.First(r=>r.Id==c.RoomId).Floor>=2);break;
                    case 10:done=State.Graduates>=3;break;
                    case 11:done=State.Prestige>=3&&State.CompletedResearch.Count==4;break;
                }
                if(!done)break;int index=State.StoryChapter++;Earn(StoryRewards[index]);State.News.Insert(0,"story:"+index);if(State.News.Count>10)State.News.RemoveAt(10);
            }
        }
        public Result FinancialBridge()
        {
            if(!Modern||Sandbox||State.GameOver||State.Cash>=0||YearDay<10||State.LastRescueYear==SchoolYear)return Result.Fail("done");State.LastRescueYear=SchoolYear;Earn(1500);return Result.Ok();
        }
        private static void UpgradeRektor(SchoolState s)
        {
            s.SaveVersion=5;s.SchoolName="Campus von "+s.Director.Name;s.RulesVersion=0;s.Difficulty=Difficulty.Standard;s.Mode=GameMode.Campaign;s.StoryChapter=s.ConsecutivePositive=s.LastRescueYear=0;
            foreach(Room r in s.Rooms){r.FurnishingVersion=0;r.Items=new List<RoomItem>();}
        }
        private static bool ValidateRektor(SchoolState s)
        {
            if(string.IsNullOrWhiteSpace(s.SchoolName)||s.SchoolName.Length>64)return false;
            if(s.RulesVersion<0||s.RulesVersion>1||!Enum.IsDefined(typeof(Difficulty),s.Difficulty)||!Enum.IsDefined(typeof(GameMode),s.Mode)||s.StoryChapter<0||s.StoryChapter>12||s.ConsecutivePositive<0||s.ConsecutivePositive>s.Day||s.LastRescueYear<0||s.LastRescueYear>(s.Day-1)/60+1)return false;
            foreach(Room r in s.Rooms)
            {
                if(r.Items==null||r.FurnishingVersion<0||r.FurnishingVersion>1||r.Items.Distinct().Count()!=r.Items.Count||r.Items.Any(i=>!RektorCatalog.For(r.Kind).Any(spe=>spe.Id==i))||r.FurnishingVersion==0&&r.Items.Count>0)return false;
                if(r.Items.Contains(RoomItem.DeskRow2)&&!r.Items.Contains(RoomItem.DeskRow1)||r.Items.Contains(RoomItem.DeskRow3)&&!r.Items.Contains(RoomItem.DeskRow2))return false;
                if(r.Items.Any(i=>RektorCatalog.Get(i).Decoration)&&RektorCatalog.For(r.Kind).Any(i=>!i.Decoration&&!r.Items.Contains(i.Id)))return false;
            }
            return true;
        }
    }
}

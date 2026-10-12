using System;
using System.Collections.Generic;
using System.Linq;
namespace KoSch.SchoolTycoon.Core
{
    public enum Difficulty { Relaxed, Standard, Expert }
    public enum GameMode { Campaign, Sandbox }
    public enum RoomItem { Storage, TaskLighting, TeachingMaterials, ErgonomicChairs, SafetyKit, AcousticPanels, DisplayScreen, AccessibilityDesk, ClimateControl, DeskRow1, DeskRow2, DeskRow3, ReferenceBooks, DigitalLibrary, QuietStudy, Microscopes, ChemistrySet, LabSafety, Workstations, NetworkKit, CADKit, Instruments, RecordingDesk, AcousticPiano, BenchTools, ElectronicsKit, Fabricator, CookingUtensils, NutritionKit, AllergyStation, StageLights, SoundRig, ProductionStage, Headsets, LanguageBooks, VideoCalls, Easels, Printmaking, MediaStudio, SportsEquipment, TeamSports, AdaptiveSports, Handwash, HygieneKit, SensorTaps, CoffeeStation, RestSeats, TrainingCorner, ServingLine, MealPrep, FreshFood, RaisedBeds, RainBarrel, OutdoorClass, Plants, WallArt, SignatureDecor }
    public sealed class DifficultySpec
    {
        public Difficulty Id;public string NameDe,NameEn;public int StartCash,Funding,PricePercent,UpkeepPercent,DebtGrace,DebtLimit,LearningPenalty;
        public DifficultySpec(Difficulty id,string de,string en,int cash,int funding,int price,int upkeep,int grace,int limit,int penalty)
        {Id=id;NameDe=de;NameEn=en;StartCash=cash;Funding=funding;PricePercent=price;UpkeepPercent=upkeep;DebtGrace=grace;DebtLimit=limit;LearningPenalty=penalty;}
    }
    public sealed class ItemSpec
    {
        public RoomItem Id;public string NameDe,NameEn;public int Cost,Daily,Day,Learning,Happiness,Wellbeing,Cleanliness;public ResearchKind? Research;public RoomKind? Room;public bool Decoration;
        public ItemSpec(RoomItem id,string de,string en,int cost,int day,int learning=0,int happy=0,int wellbeing=0,int clean=0,int daily=0,ResearchKind? research=null,RoomKind? room=null,bool decor=false)
        {Id=id;NameDe=de;NameEn=en;Cost=cost;Day=day;Learning=learning;Happiness=happy;Wellbeing=wellbeing;Cleanliness=clean;Daily=daily;Research=research;Room=room;Decoration=decor;}
    }
    public static class RektorCatalog
    {
        public static readonly DifficultySpec[] Difficulties={
            new DifficultySpec(Difficulty.Relaxed,"Entspannt","Relaxed",21000,24,85,90,9,8000,0),
            new DifficultySpec(Difficulty.Standard,"Ausgewogen","Balanced",16000,20,100,100,5,5000,2),
            new DifficultySpec(Difficulty.Expert,"Anspruchsvoll","Challenging",13000,18,112,110,3,3500,4)};
        public static readonly ItemSpec[] Items={
            new ItemSpec(RoomItem.Storage,"Materialschrank","Storage cabinet",280,5,1,0,0,1),
            new ItemSpec(RoomItem.TaskLighting,"Lernbeleuchtung","Task lighting",460,12,2,1),
            new ItemSpec(RoomItem.TeachingMaterials,"Lehrmittel-Sammlung","Teaching resources",620,18,3),
            new ItemSpec(RoomItem.ErgonomicChairs,"Ergonomische Sitzplätze","Ergonomic seating",800,25,2,1,3),
            new ItemSpec(RoomItem.SafetyKit,"Sicherheitsausstattung","Safety equipment",480,35,0,1,2,1),
            new ItemSpec(RoomItem.AcousticPanels,"Akustikpaneele","Acoustic panels",1200,45,2,3,2),
            new ItemSpec(RoomItem.DisplayScreen,"Interaktives Display","Interactive display",2200,65,4,0,0,0,3,ResearchKind.DigitalLearning),
            new ItemSpec(RoomItem.AccessibilityDesk,"Barrierefreier Lernplatz","Accessible learning desk",1450,100,2,1,3,0,0,ResearchKind.InclusiveLearning),
            new ItemSpec(RoomItem.ClimateControl,"Angenehmes Raumklima","Climate control",1750,120,2,4,2,0,4,ResearchKind.GreenCampus),
            new ItemSpec(RoomItem.DeskRow1,"Zusätzliche Tischgruppe I · +4 Plätze","Extra desk group I · +4 seats",750,15,room:RoomKind.Classroom),
            new ItemSpec(RoomItem.DeskRow2,"Zusätzliche Tischgruppe II · +4 Plätze","Extra desk group II · +4 seats",1250,45,room:RoomKind.Classroom),
            new ItemSpec(RoomItem.DeskRow3,"Zusätzliche Tischgruppe III · +4 Plätze","Extra desk group III · +4 seats",1850,110,research:ResearchKind.InclusiveLearning,room:RoomKind.Classroom),
            new ItemSpec(RoomItem.ReferenceBooks,"Fachbuchregale","Reference shelves",950,25,3,room:RoomKind.Library),
            new ItemSpec(RoomItem.DigitalLibrary,"Digitale Bibliothek","Digital library",2300,80,4,daily:3,research:ResearchKind.DigitalLearning,room:RoomKind.Library),
            new ItemSpec(RoomItem.QuietStudy,"Ruhige Lesekoje","Quiet reading nook",1550,110,2,2,3,research:ResearchKind.InclusiveLearning,room:RoomKind.Library),
            new ItemSpec(RoomItem.Microscopes,"Mikroskop-Satz","Microscope set",1500,60,4,room:RoomKind.ScienceLab),
            new ItemSpec(RoomItem.ChemistrySet,"Experimentier-Satz","Chemistry equipment",2100,95,3,daily:3,room:RoomKind.ScienceLab),
            new ItemSpec(RoomItem.LabSafety,"Sicherer Laborarbeitsplatz","Safe laboratory workspace",1750,125,1,1,3,2,room:RoomKind.ScienceLab),
            new ItemSpec(RoomItem.Workstations,"Schüler-PCs","Student workstations",1800,45,4,daily:4,research:ResearchKind.DigitalLearning,room:RoomKind.ComputerLab),
            new ItemSpec(RoomItem.NetworkKit,"Lernnetzwerk","Learning network",1900,95,3,daily:2,research:ResearchKind.DigitalLearning,room:RoomKind.ComputerLab),
            new ItemSpec(RoomItem.CADKit,"CAD & Medienplätze","CAD and media stations",2900,155,4,daily:4,research:ResearchKind.DigitalLearning,room:RoomKind.ComputerLab),
            new ItemSpec(RoomItem.Instruments,"Instrumenten-Sammlung","Instrument collection",1200,50,4,room:RoomKind.MusicRoom),
            new ItemSpec(RoomItem.RecordingDesk,"Aufnahmestation","Recording desk",2400,100,3,daily:3,research:ResearchKind.DigitalLearning,room:RoomKind.MusicRoom),
            new ItemSpec(RoomItem.AcousticPiano,"Konzertklavier","Acoustic piano",3200,155,4,2,research:ResearchKind.CreativeCampus,room:RoomKind.MusicRoom),
            new ItemSpec(RoomItem.BenchTools,"Werkbank-Werkzeuge","Workbench tools",1500,95,3,room:RoomKind.Workshop),
            new ItemSpec(RoomItem.ElectronicsKit,"Elektronikstation","Electronics station",2600,140,4,daily:3,research:ResearchKind.DigitalLearning,room:RoomKind.Workshop),
            new ItemSpec(RoomItem.Fabricator,"Schul-Fabrikator","School fabrication station",4100,210,5,daily:5,research:ResearchKind.DigitalLearning,room:RoomKind.Workshop),
            new ItemSpec(RoomItem.CookingUtensils,"Koch-Satz","Cooking utensils",900,45,3,room:RoomKind.FoodLab),
            new ItemSpec(RoomItem.NutritionKit,"Ernährungs-Lernstation","Nutrition learning station",1800,95,3,2,room:RoomKind.FoodLab),
            new ItemSpec(RoomItem.AllergyStation,"Allergenbewusste Küche","Allergen-aware kitchen",2100,140,2,2,3,research:ResearchKind.InclusiveLearning,room:RoomKind.FoodLab),
            new ItemSpec(RoomItem.StageLights,"Bühnenbeleuchtung","Stage lighting",1900,165,3,daily:3,research:ResearchKind.CreativeCampus,room:RoomKind.Theatre),
            new ItemSpec(RoomItem.SoundRig,"Bühnenton","Stage sound system",2700,205,4,daily:3,research:ResearchKind.DigitalLearning,room:RoomKind.Theatre),
            new ItemSpec(RoomItem.ProductionStage,"Verwandelbare Bühne","Flexible production stage",4200,265,5,3,room:RoomKind.Theatre),
            new ItemSpec(RoomItem.Headsets,"Sprachlern-Headsets","Language headsets",1100,30,3,room:RoomKind.LanguageLab),
            new ItemSpec(RoomItem.LanguageBooks,"Mehrsprachige Lernmedien","Multilingual resources",1250,65,3,room:RoomKind.LanguageLab),
            new ItemSpec(RoomItem.VideoCalls,"Austausch-Videostation","Exchange video station",2400,125,4,2,daily:2,research:ResearchKind.DigitalLearning,room:RoomKind.LanguageLab),
            new ItemSpec(RoomItem.Easels,"Staffeleien & Farben","Easels and paints",1000,75,3,room:RoomKind.ArtRoom),
            new ItemSpec(RoomItem.Printmaking,"Druckwerkstatt","Printmaking station",2100,115,4,room:RoomKind.ArtRoom),
            new ItemSpec(RoomItem.MediaStudio,"Kreatives Medienatelier","Creative media studio",2900,170,4,2,daily:3,research:ResearchKind.DigitalLearning,room:RoomKind.ArtRoom),
            new ItemSpec(RoomItem.SportsEquipment,"Sportgeräte","Sports equipment",1450,110,3,1,2,room:RoomKind.Gym),
            new ItemSpec(RoomItem.TeamSports,"Teamsport-Satz","Team sports kit",2100,160,3,2,2,room:RoomKind.Gym),
            new ItemSpec(RoomItem.AdaptiveSports,"Inklusiver Sport-Satz","Adaptive sports kit",2700,210,3,2,3,research:ResearchKind.InclusiveLearning,room:RoomKind.Gym),
            new ItemSpec(RoomItem.Handwash,"Zusätzliche Handwaschplätze","Extra washbasins",550,12,clean:2,room:RoomKind.Toilet),
            new ItemSpec(RoomItem.HygieneKit,"Hygiene-Satz","Hygiene kit",650,30,wellbeing:2,clean:3,room:RoomKind.Toilet),
            new ItemSpec(RoomItem.SensorTaps,"Wassersparende Armaturen","Water-saving taps",1100,75,clean:2,research:ResearchKind.GreenCampus,room:RoomKind.Toilet),
            new ItemSpec(RoomItem.CoffeeStation,"Teeküche","Staff kitchenette",650,12,happy:2,wellbeing:2,room:RoomKind.Staffroom),
            new ItemSpec(RoomItem.RestSeats,"Erholungssofa","Rest lounge",1200,50,happy:3,wellbeing:4,room:RoomKind.Staffroom),
            new ItemSpec(RoomItem.TrainingCorner,"Fortbildungs-Ecke","Training corner",1650,85,learning:3,room:RoomKind.Staffroom),
            new ItemSpec(RoomItem.ServingLine,"Zusätzliche Essensausgabe","Extra serving line",750,12,happy:2,room:RoomKind.Canteen),
            new ItemSpec(RoomItem.MealPrep,"Frischeküche","Fresh meal kitchen",1600,60,happy:3,wellbeing:2,daily:2,room:RoomKind.Canteen),
            new ItemSpec(RoomItem.FreshFood,"Vielfältiges Essensangebot","Varied meal service",2200,95,happy:3,wellbeing:3,daily:3,room:RoomKind.Canteen),
            new ItemSpec(RoomItem.RaisedBeds,"Projekt-Hochbeete","Project raised beds",450,8,learning:2,happy:1,room:RoomKind.Garden),
            new ItemSpec(RoomItem.RainBarrel,"Regenwasser-Sammlung","Rainwater collection",650,50,happy:1,research:ResearchKind.GreenCampus,room:RoomKind.Garden),
            new ItemSpec(RoomItem.OutdoorClass,"Grünes Klassenzimmer","Outdoor classroom",1600,90,learning:3,happy:2,room:RoomKind.Garden),
            new ItemSpec(RoomItem.Plants,"Pflanzen & kleine Akzente","Plants and small accents",650,145,happy:2,decor:true),
            new ItemSpec(RoomItem.WallArt,"Schülerkunst & Farben","Student art and colours",1100,180,happy:2,decor:true),
            new ItemSpec(RoomItem.SignatureDecor,"Individuelles Raumdesign","Signature room design",1900,240,happy:3,decor:true)};
        public static ItemSpec Get(RoomItem id){return Items.FirstOrDefault(x=>x.Id==id);}
        public static bool TeachingRoom(RoomKind k){return k==RoomKind.Classroom||k==RoomKind.Library||(int)k>=7&&k!=RoomKind.Staircase;}
        public static IEnumerable<ItemSpec> For(RoomKind kind)
        {
            if(kind==RoomKind.Corridor||kind==RoomKind.Staircase)return Enumerable.Empty<ItemSpec>();
            return Items.Where(i=>i.Decoration||i.Room==kind||i.Room==null&&!i.Decoration&&(TeachingRoom(kind)||kind==RoomKind.Staffroom&&new[]{RoomItem.Storage,RoomItem.TaskLighting,RoomItem.AcousticPanels,RoomItem.ClimateControl}.Contains(i.Id)||kind==RoomKind.Toilet&&new[]{RoomItem.Storage,RoomItem.TaskLighting,RoomItem.ClimateControl}.Contains(i.Id)));
        }
    }
}

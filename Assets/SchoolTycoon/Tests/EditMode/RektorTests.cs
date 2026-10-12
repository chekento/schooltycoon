using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using KoSch.SchoolTycoon.Core;
namespace KoSch.SchoolTycoon.Tests
{
    public partial class SchoolSimulationTests
    {
        private void Campaign(Difficulty d=Difficulty.Standard,GameMode mode=GameMode.Campaign)
        {sim=new SchoolSimulation(SchoolSimulation.CreateCampaign(d,mode));sim.State.HasDirector=true;sim.State.SchoolName="Test campus";}
        private string SaveJson(SchoolState s)
        {
#if UNITY_5_3_OR_NEWER
            return UnityEngine.JsonUtility.ToJson(s,true);
#else
            return System.Text.Json.JsonSerializer.Serialize(s,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});
#endif
        }
        private SchoolState ReadJson(string json)
        {
#if UNITY_5_3_OR_NEWER
            return UnityEngine.JsonUtility.FromJson<SchoolState>(json);
#else
            return System.Text.Json.JsonSerializer.Deserialize<SchoolState>(json,new System.Text.Json.JsonSerializerOptions{IncludeFields=true});
#endif
        }
        [Test] public void NewCampaignsHaveDistinctDifficultiesAndOnlyBasicRoomEquipment()
        {foreach(Difficulty d in Enum.GetValues(typeof(Difficulty))){Campaign(d);Assert.AreEqual(RektorCatalog.Difficulties[(int)d].StartCash,sim.State.Cash);Assert.AreEqual(18,sim.PhysicalCapacity);Assert.AreEqual(0,sim.State.Rooms.Sum(r=>r.Items.Count));Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));}Assert.Greater(RektorCatalog.Difficulties[0].Funding,RektorCatalog.Difficulties[1].Funding);Assert.Greater(RektorCatalog.Difficulties[1].Funding,RektorCatalog.Difficulties[2].Funding);}
        [Test] public void AdvancedRoomsAndUpperFloorsNeedTimeResearchAndRealRequirements()
        {Campaign();sim.Hire(0);sim.Build(RoomKind.Toilet,16,6);sim.Build(RoomKind.Staffroom,12,6);Assert.IsFalse(sim.RoomUnlocked(RoomKind.Classroom));Assert.IsFalse(sim.RoomUnlocked(RoomKind.Library));Assert.AreEqual(0,sim.AllowedFloor);sim.State.Day=80;Assert.IsFalse(sim.RoomUnlocked(RoomKind.ComputerLab));Assert.IsFalse(sim.RoomUnlocked(RoomKind.Classroom,2));sim.State.CompletedResearch.Add(ResearchKind.GreenCampus);Assert.IsTrue(sim.RoomUnlocked(RoomKind.Classroom,2));}
        [Test] public void RoomItemsAreBoughtOnceAndDeskGroupsAddActualSeats()
        {Campaign();sim.State.Day=15;Room r=sim.State.Rooms.First(x=>x.Id==2);int cash=sim.State.Cash;Assert.IsTrue(sim.BuyRoomItem(r.Id,RoomItem.DeskRow1).Success);Assert.AreEqual(cash-750,sim.State.Cash);Assert.AreEqual(22,sim.PhysicalCapacity);Assert.AreEqual("done",sim.BuyRoomItem(r.Id,RoomItem.DeskRow1).Code);Assert.AreEqual(cash-750,sim.State.Cash);Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));}
        [Test] public void DecorationRequiresAllFunctionalEquipmentAndPreviousDecorationTiers()
        {Campaign(mode:GameMode.Sandbox);Room r=sim.State.Rooms.First(x=>x.Id==2);Assert.AreEqual("decorationlast",sim.BuyRoomItem(r.Id,RoomItem.Plants).Code);foreach(ItemSpec i in RektorCatalog.For(r.Kind).Where(i=>!i.Decoration))Assert.IsTrue(sim.BuyRoomItem(r.Id,i.Id).Success,i.NameEn);Assert.AreEqual("prerequisite",sim.BuyRoomItem(r.Id,RoomItem.WallArt).Code);Assert.IsTrue(sim.BuyRoomItem(r.Id,RoomItem.Plants).Success);Assert.IsTrue(sim.BuyRoomItem(r.Id,RoomItem.WallArt).Success);Assert.IsTrue(sim.BuyRoomItem(r.Id,RoomItem.SignatureDecor).Success);Assert.AreEqual(5,sim.RoomTier(r));Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));}
        [Test] public void FunctionalItemsInfluenceLearningComfortAndRecurringExpenses()
        {Campaign(mode:GameMode.Sandbox);Room r=sim.State.Rooms.First(x=>x.Id==2);int cost=sim.Forecast().Maintenance;Assert.IsTrue(sim.BuyRoomItem(r.Id,RoomItem.ErgonomicChairs).Success);Assert.Greater(sim.RoomLearning(r),0);Assert.Greater(sim.RoomSupport(r),0);sim.BuyRoomItem(r.Id,RoomItem.DisplayScreen);Assert.Greater(sim.Forecast().Maintenance,cost);}
        [Test] public void PurchasedEquipmentAppearsInTheSharedThreeDimensionalRecipes()
        {Campaign(mode:GameMode.Sandbox);Room r=sim.State.Rooms.First(x=>x.Id==2);var before=CampusDesign.Room(r);Assert.IsFalse(before.Any(p=>p.Name=="Interactive learning display"));sim.BuyRoomItem(r.Id,RoomItem.DisplayScreen);var after=CampusDesign.Room(r);Assert.IsTrue(after.Any(p=>p.Name=="Interactive learning display"));Assert.Greater(after.Count,before.Count);Assert.IsFalse(before.Any(p=>p.Name=="Ceramic planter"));}
        [Test] public void SandboxFundsNeverDecreaseForPurchasesStaffEventsAndDailySettlement()
        {Campaign(mode:GameMode.Sandbox);Assert.AreEqual(0,sim.State.Cash);sim.Hire(0);sim.Hire(3);sim.Build(RoomKind.Toilet,16,6);sim.Build(RoomKind.Staffroom,12,6);sim.Build(RoomKind.Classroom,16,10);sim.Hire(1);sim.State.PendingEvent=0;Assert.IsTrue(sim.ChooseEvent(0).Success);foreach(ItemSpec i in RektorCatalog.For(RoomKind.Classroom).Where(i=>!i.Decoration))Assert.IsTrue(sim.BuyRoomItem(2,i.Id).Success);RunDays(60);Assert.AreEqual(0,sim.State.Cash);Assert.AreEqual(0,sim.State.DebtDays);Assert.IsFalse(sim.State.GameOver);}
        [Test] public void DifficultyPricesAndFundingAreUsedByTheActualTransactions()
        {int[] costs=new int[3],funding=new int[3];for(int i=0;i<3;i++){Campaign((Difficulty)i);int before=sim.State.Cash;sim.Build(RoomKind.Toilet,16,6);costs[i]=before-sim.State.Cash;sim.Hire(0);funding[i]=sim.Forecast().Funding;}Assert.Less(costs[0],costs[1]);Assert.Less(costs[1],costs[2]);Assert.Greater(funding[0],funding[1]);Assert.Greater(funding[1],funding[2]);}
        [Test] public void DifficultiesHaveDifferentDebtGracePeriodsAndSandboxHasNone()
        {foreach(Difficulty d in Enum.GetValues(typeof(Difficulty))){Campaign(d);sim.State.Cash=-100;int limit=sim.Rules.DebtGrace;for(int i=0;i<limit;i++){if(sim.State.PendingEvent>=0)sim.ChooseEvent(2);sim.AdvanceDay();if(i<limit-1)Assert.IsFalse(sim.State.GameOver);}Assert.IsTrue(sim.State.GameOver);}Campaign(mode:GameMode.Sandbox);sim.State.Cash=-100;RunDays(12);Assert.IsFalse(sim.State.GameOver);}
        [Test] public void StoryRewardsCannotBeClaimedRepeatedly()
        {Campaign();sim.Hire(0);int cash=sim.State.Cash;Assert.AreEqual(1,sim.State.StoryChapter);sim.CheckStory();sim.CheckGoals();Assert.AreEqual(cash,sim.State.Cash);Assert.IsNotEmpty(sim.StoryAdvice());}
        [Test] public void FinancialBridgeCanOnlyBeClaimedOnceInEachSchoolYear()
        {Campaign();sim.State.Day=10;sim.State.Cash=-100;Assert.IsTrue(sim.FinancialBridge().Success);Assert.AreEqual(1400,sim.State.Cash);sim.State.Cash=-100;Assert.AreEqual("done",sim.FinancialBridge().Code);sim.State.Day=70;Assert.IsTrue(sim.FinancialBridge().Success);}
        [Test] public void APAndHigherCourseLevelsUnlockAfterPreparation()
        {Campaign();Assert.IsFalse(sim.SetWeeklyLesson(2,0,0,Subject.APPhysics1).Success);Assert.IsFalse(sim.SetCourseLevel(2,2).Success);sim.State.Day=150;sim.State.Learning=70;sim.State.CompletedResearch.Add(ResearchKind.DigitalLearning);Assert.IsTrue(sim.SetWeeklyLesson(2,0,0,Subject.APPhysics1).Success);Assert.IsTrue(sim.SetCourseLevel(2,3).Success);}
        [Test] public void VersionFourMigrationPreservesSchoolProgressAndExistingFurnishedRooms()
        {ReadyAcademy();RunDays(20);sim.State.SaveVersion=4;int score=sim.State.Pupils[0].Scores[0],cash=sim.State.Cash;Assert.IsTrue(SchoolSimulation.MigrateSave(sim.State));Assert.AreEqual(5,sim.State.SaveVersion);Assert.AreEqual(cash,sim.State.Cash);Assert.AreEqual(score,sim.State.Pupils[0].Scores[0]);Assert.AreEqual(0,sim.State.RulesVersion);Assert.AreEqual(24,sim.RoomSeats(sim.State.Rooms.First(r=>r.Id==2)));}
        [Test] public void ModernSaveValidationRejectsDuplicateItemsMissingPrerequisitesAndNullInventories()
        {Campaign();Room r=sim.State.Rooms.First(x=>x.Id==2);r.Items.Add(RoomItem.DeskRow2);Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));r.Items.Clear();r.Items.Add(RoomItem.Storage);r.Items.Add(RoomItem.Storage);Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));r.Items=null;Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));}
        [Test] public void SixSaveSlotsRemainIndependentAndRoundTripModesDifficultiesAndCourseMarks()
        {
            string folder=Path.Combine(Path.GetTempPath(),"rektor-tests-"+Guid.NewGuid());try
            {var store=new SaveArchive(folder,SaveJson,ReadJson);for(int slot=1;slot<=6;slot++){Campaign((Difficulty)((slot-1)%3),slot==6?GameMode.Sandbox:GameMode.Campaign);sim.State.SchoolName="Campus "+slot;sim.SetWeeklyLesson(2,0,1,Subject.German);sim.SetScore(sim.State.Pupils[0],Subject.German,30+slot);string error;Assert.IsTrue(store.Write(slot,sim.State,out error),error);}for(int slot=1;slot<=6;slot++){string error;SchoolState s=store.Load(slot,out error);Assert.NotNull(s,error);Assert.AreEqual("Campus "+slot,s.SchoolName);Assert.AreEqual((Difficulty)((slot-1)%3),s.Difficulty);Assert.AreEqual(30+slot,new SchoolSimulation(s).Score(s.Pupils[0],Subject.German));}store.Delete(2);Assert.IsFalse(store.HasData(2));Assert.IsTrue(store.HasData(1));Assert.IsTrue(store.HasData(6));}
            finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
        }
        [Test] public void CorruptPrimarySaveRecoversBackupWithoutOverwritingThatBackup()
        {
            string folder=Path.Combine(Path.GetTempPath(),"rektor-tests-"+Guid.NewGuid());try
            {var store=new SaveArchive(folder,SaveJson,ReadJson);Campaign();string error;Assert.IsTrue(store.Write(1,sim.State,out error));sim.State.Cash=12345;Assert.IsTrue(store.Write(1,sim.State,out error));File.WriteAllText(store.PathFor(1),"corrupt");SchoolState recovered=store.Load(1,out error);Assert.NotNull(recovered);Assert.AreEqual("backup",error);Assert.AreEqual(16000,recovered.Cash);Assert.IsTrue(store.Write(1,recovered,out error));File.WriteAllText(store.PathFor(1),"corrupt again");Assert.NotNull(store.Load(1,out error));Assert.AreEqual("backup",error);}
            finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
        }
        [Test] public void InvalidSaveWritesDoNotDamageExistingSlots()
        {
            string folder=Path.Combine(Path.GetTempPath(),"rektor-tests-"+Guid.NewGuid());try
            {var store=new SaveArchive(folder,SaveJson,ReadJson);Campaign();string error;Assert.IsTrue(store.Write(1,sim.State,out error));sim.State.Rooms[1].Floor=5;Assert.IsFalse(store.Write(1,sim.State,out error));Assert.NotNull(store.Load(1,out error));Assert.Throws<ArgumentOutOfRangeException>(()=>store.PathFor(0));}
            finally{if(Directory.Exists(folder))Directory.Delete(folder,true);}
        }
        [Test] public void ModernSandboxCanBuildAndRouteThroughAllSixFloors()
        {
            Campaign(mode:GameMode.Sandbox);sim.Hire(0);Assert.IsTrue(sim.Build(RoomKind.Staircase,16,10).Success);
            for(int f=1;f<6;f++){Assert.IsTrue(sim.Build(RoomKind.Staircase,16,10,false,f).Success);for(int x=16;x>=12;x--)Assert.IsTrue(sim.Build(RoomKind.Corridor,x,9,false,f).Success);Assert.IsTrue(sim.Build(RoomKind.Classroom,12,10,false,f).Success);}
            var target=new Cell(16,10,5);var path=CampusGrid.Path(sim.State,CampusGrid.Entrance,target);Assert.Greater(path.Count,5);Assert.AreEqual(5,path.Zip(path.Skip(1),(a,b)=>a.Floor!=b.Floor).Count(v=>v));Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));Assert.AreEqual(6,sim.State.Classes.Count);Assert.AreEqual("floor",sim.Build(RoomKind.Corridor,12,9,false,6).Code);
        }
        [Test] public void ModernEnglandAndUSCurriculaRemainSustainableWithActualStartingCapital()
        {
            foreach(CurriculumRegion region in new[]{CurriculumRegion.England,CurriculumRegion.USA})
            {Campaign(Difficulty.Expert);sim.ApplyCurriculum(region);for(int day=0;day<360;day++){CampaignPlan();Assert.IsTrue(sim.AdvanceDay().Success);Assert.IsFalse(sim.State.GameOver,region.ToString());Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));}Assert.GreaterOrEqual(sim.State.CompletedResearch.Count,3,region.ToString());}
        }
        [Test] public void FundingGrowsMoreSlowlyBeyondFortyEightAndNinetySixPupils()
        {Campaign();Assert.AreEqual(960,sim.EducationFunding(48));Assert.AreEqual(960+48*16,sim.EducationFunding(96));Assert.Less(sim.EducationFunding(150),150*20);Assert.Greater(sim.EducationFunding(150),sim.EducationFunding(96));}
        private bool Affordable(int amount,int reserve=2200){return sim.State.Cash>=amount+reserve;}
        private bool CampaignBuild(RoomKind kind)
        {if(!sim.RoomUnlocked(kind)||!Affordable(sim.BuildCost(kind)))return false;return BuildSomewhere(kind);}
        private bool CampaignTeacher()
        {Employee e=sim.Candidates().FirstOrDefault(e=>e.Role==StaffRole.Teacher&&!sim.State.Staff.Any(s=>s.Name==e.Name));return e!=null&&Affordable(sim.Price(700))&&sim.Hire(e.Id).Success;}
        private void CampaignPlan()
        {
            if(sim.State.PendingEvent>=0)sim.ChooseEvent(2);
            if(sim.Teachers==0)sim.Hire(0);
            if(sim.Count(RoomKind.Toilet)==0)sim.Build(RoomKind.Toilet,16,6);
            if(sim.Count(RoomKind.Staffroom)==0)sim.Build(RoomKind.Staffroom,12,6);
            if(!sim.State.Staff.Any(e=>e.Role==StaffRole.Caretaker))sim.Hire(3);
            if(sim.State.Day>=8&&sim.Count(RoomKind.Classroom)<2&&Affordable(sim.BuildCost(RoomKind.Classroom)+sim.Price(700)))
            {Assert.IsTrue(sim.Build(RoomKind.Classroom,16,10).Success);CampaignTeacher();}
            if(sim.State.Day>=10&&sim.Count(RoomKind.Canteen)==0&&Affordable(sim.BuildCost(RoomKind.Canteen)))sim.Build(RoomKind.Canteen,18,6);
            if(sim.State.Day>=20)
            {
                foreach(Cell c in new[]{new Cell(11,9),new Cell(11,10)})if(CampusGrid.At(sim.State,c)==null&&Affordable(sim.BuildCost(RoomKind.Corridor)))sim.Build(RoomKind.Corridor,c.X,c.Y);
                if(sim.Count(RoomKind.Library)==0&&sim.RoomUnlocked(RoomKind.Library)&&Affordable(sim.BuildCost(RoomKind.Library)))sim.Build(RoomKind.Library,7,10);
            }
            if(sim.State.Day>=25)
            {
                for(int x=18;x<=23;x++)if(CampusGrid.At(sim.State,new Cell(x,9))==null&&Affordable(80))sim.Build(RoomKind.Corridor,x,9);
                if(sim.Count(RoomKind.Library)>0&&sim.Count(RoomKind.Classroom)<3&&Affordable(sim.BuildCost(RoomKind.Classroom)+sim.Price(700))){if(sim.Build(RoomKind.Classroom,20,10).Success)CampaignTeacher();}
                for(int y=11;y<=14;y++)if(CampusGrid.At(sim.State,new Cell(11,y))==null&&Affordable(80))sim.Build(RoomKind.Corridor,11,y);
                for(int x=12;x<=23;x++)if(CampusGrid.At(sim.State,new Cell(x,14))==null&&Affordable(80))sim.Build(RoomKind.Corridor,x,14);
            }
            if(sim.State.Day>=35&&sim.Count(RoomKind.Staircase)==0&&Affordable(sim.BuildCost(RoomKind.Staircase)))sim.Build(RoomKind.Staircase,22,7);
            if(sim.State.Day>=80&&sim.Researched(ResearchKind.GreenCampus))for(int floor=1;floor<=2;floor++)
            {
                if(!sim.State.Rooms.Any(r=>r.Kind==RoomKind.Staircase&&r.Floor==floor)&&Affordable(sim.BuildCost(RoomKind.Staircase,floor)))sim.Build(RoomKind.Staircase,22,7,false,floor);
                if(sim.State.Rooms.Any(r=>r.Kind==RoomKind.Staircase&&r.Floor==floor))
                {for(int x=22;x>=12;x--)if(CampusGrid.At(sim.State,new Cell(x,9,floor))==null&&Affordable(sim.BuildCost(RoomKind.Corridor,floor)))sim.Build(RoomKind.Corridor,x,9,false,floor);
                 if(!sim.State.Rooms.Any(r=>r.Kind==RoomKind.Classroom&&r.Floor==floor)&&Affordable(sim.BuildCost(RoomKind.Classroom,floor)+sim.Price(700))&&sim.Build(RoomKind.Classroom,12,10,false,floor).Success)CampaignTeacher();}
            }
            if(sim.State.Rooms.Where(r=>r.Kind==RoomKind.Toilet).Sum(r=>48+r.UpgradeLevel*12)<sim.State.Students)CampaignBuild(RoomKind.Toilet);
            if(sim.Count(RoomKind.Canteen)>0&&sim.State.Rooms.Where(r=>r.Kind==RoomKind.Canteen).Sum(r=>72+r.UpgradeLevel*12)<sim.State.Students)CampaignBuild(RoomKind.Canteen);
            if(sim.State.Cleanliness<90&&sim.State.Staff.Count(e=>e.Role==StaffRole.Caretaker)<2&&Affordable(sim.Price(400)))sim.Hire(3);
            if(sim.Count(RoomKind.Garden)==0)CampaignBuild(RoomKind.Garden);
            if(sim.State.Day>=60&&sim.State.CampusLevel==0&&Affordable(sim.Price(9000),8000))sim.BuyLand();
            foreach(Employee e in sim.State.Staff.Where(e=>e.Role==StaffRole.Teacher&&e.TrainingLevel<3).ToList())if(Affordable(sim.TrainingCost(e)))sim.TrainStaff(e.Id);
            foreach(ResearchKind research in new[]{ResearchKind.GreenCampus,ResearchKind.DigitalLearning,ResearchKind.InclusiveLearning,ResearchKind.CreativeCampus})
                if(sim.State.ActiveResearch<0&&!sim.Researched(research)&&sim.State.Day>=sim.ResearchStartDay(research)&&Affordable(sim.Price(AcademyCatalog.Research[(int)research].Cost),4000))sim.StartResearch(research);
            foreach(RoomKind kind in new[]{RoomKind.ScienceLab,RoomKind.ArtRoom,RoomKind.LanguageLab,RoomKind.ComputerLab,RoomKind.MusicRoom,RoomKind.Gym,RoomKind.Workshop,RoomKind.FoodLab,RoomKind.Theatre})if(sim.Count(kind)==0)CampaignBuild(kind);
            foreach(Room r in sim.State.Rooms.Where(r=>r.Kind==RoomKind.Classroom).ToList())foreach(RoomItem item in new[]{RoomItem.DeskRow1,RoomItem.Storage,RoomItem.TeachingMaterials,RoomItem.TaskLighting,RoomItem.DeskRow2,RoomItem.ErgonomicChairs,RoomItem.DisplayScreen,RoomItem.DeskRow3})
                if(sim.CanBuyItem(r,item).Success&&Affordable(sim.ItemCost(item),4000))sim.BuyRoomItem(r.Id,item);
            foreach(ClubKind kind in new[]{ClubKind.Reading,ClubKind.Gardening})if(!sim.State.Clubs.Contains(kind)&&sim.ClubAvailable(kind)&&Affordable(sim.Price(AcademyCatalog.Clubs[(int)kind].Setup),8000))sim.SetClub(kind,true);
            Ledger f=sim.Forecast();if(f.Income<f.Expenses)sim.SetPolicy(1,0);
            if(sim.State.ActiveContract<0&&sim.State.ContractYear!=sim.SchoolYear&&sim.YearDay<=51&&Affordable(sim.CareerCost))foreach(int k in sim.CareerChoices())if(sim.CareerCondition(k)){sim.StartCareer(k);break;}
        }
        [Test] public void AllThreeDifficultiesSupportSevenHundredTwentyDaysOfEarnedProgress()
        {
            foreach(Difficulty d in Enum.GetValues(typeof(Difficulty)))
            {
                Campaign(d);int minimumCash=sim.State.Cash;
                for(int day=0;day<720;day++){CampaignPlan();Assert.IsTrue(sim.AdvanceDay().Success,d+" day "+sim.State.Day);minimumCash=Math.Min(minimumCash,sim.State.Cash);Assert.IsFalse(sim.State.GameOver,d+" became insolvent on day "+sim.State.Day);Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State),d+" invalid on day "+sim.State.Day);}
                Console.WriteLine("BALANCE "+d+" cash="+sim.State.Cash+" min="+minimumCash+" pupils="+sim.State.Students+" graduates="+sim.State.Graduates+" chapters="+sim.State.StoryChapter+" prestige="+sim.State.Prestige);
                Assert.Greater(sim.State.Graduates,0,d.ToString());Assert.GreaterOrEqual(sim.State.CompletedResearch.Count,3,d.ToString());Assert.GreaterOrEqual(sim.State.StoryChapter,8,d.ToString());Assert.GreaterOrEqual(sim.State.Prestige,3,d.ToString());Assert.GreaterOrEqual(sim.State.Rooms.Max(r=>r.Floor),2,d.ToString());
            }
        }
    }
}

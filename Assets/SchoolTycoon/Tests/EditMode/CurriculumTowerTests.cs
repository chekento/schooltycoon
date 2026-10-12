using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using KoSch.SchoolTycoon.Core;
namespace KoSch.SchoolTycoon.Tests
{
    public partial class SchoolSimulationTests
    {
        private void TowerReady()
        {sim.State.Cash=100000;sim.Hire(0);sim.Build(RoomKind.Toilet,10,8);sim.Build(RoomKind.Staffroom,12,6);Assert.IsTrue(sim.Build(RoomKind.Staircase,16,10).Success);Assert.IsTrue(sim.Build(RoomKind.Staircase,16,10,false,1).Success);for(int x=16;x>=12;x--)Assert.IsTrue(sim.Build(RoomKind.Corridor,x,9,false,1).Success);Assert.IsTrue(sim.Build(RoomKind.Classroom,12,10,false,1).Success);}
        [Test] public void CourseCatalogHas146UniqueCoursesAnd43APCourses()
        {Assert.AreEqual(146,CurriculumCatalog.Subjects.Length);Assert.AreEqual(43,CurriculumCatalog.Subjects.Count(s=>s.NameEn.StartsWith("AP ")));Assert.AreEqual(146,CurriculumCatalog.Subjects.Select(s=>s.Id).Distinct().Count());for(int i=0;i<146;i++)Assert.AreEqual((Subject)i,CurriculumCatalog.Subjects[i].Id);}
        [Test] public void RegionalPlansHaveThirtyLessonsAcrossSevenOrMoreSubjectGroups()
        {foreach(CurriculumRegion region in new[]{CurriculumRegion.Germany,CurriculumRegion.England,CurriculumRegion.USA}){var week=CurriculumCatalog.Week(region);Assert.AreEqual(30,week.Count);Assert.GreaterOrEqual(week.Select(s=>CurriculumCatalog.Get(s).Group).Distinct().Count(),7);}}
        [Test] public void EditingOneWeekdayPreservesOtherDaysAndClasses()
        {ReadyAcademy();SchoolClass c=sim.State.Classes[0],other=sim.State.Classes[1];var before=new List<Subject>(c.Week);Assert.IsTrue(sim.SetWeeklyLesson(c.RoomId,3,2,Subject.Physics).Success);Assert.AreEqual(1,c.Week.Where((s,i)=>s!=before[i]).Count());Assert.AreNotEqual(Subject.Physics,other.Week[20]);}
        [Test] public void EveryBuiltinSubjectCanBeTaughtAndPersistItsOwnScore()
        {ReadyAcademy();foreach(SubjectSpec spec in CurriculumCatalog.Subjects){if(sim.State.PendingEvent>=0)sim.ChooseEvent(2);Assert.IsTrue(sim.SetLesson(0,spec.Id).Success);Pupil p=sim.State.Pupils[0];Assert.IsTrue(sim.AdvanceDay().Success);Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State),spec.NameEn);if((int)spec.Id>=5)Assert.Greater(p.CourseMarks.First(m=>m.Subject==spec.Id).Lessons,0,spec.NameEn);}}
        [Test] public void CustomSubjectsReceiveIndependentProgressAndRejectDuplicateNames()
        {sim.Hire(0);Assert.IsTrue(sim.AddSubject("Dänisch","Danish",SubjectGroup.Languages,RoomKind.LanguageLab,2).Success);Subject sub=(Subject)1000;Assert.IsTrue(sim.SetLesson(1,sub).Success);sim.AdvanceDay();Assert.Greater(sim.State.Pupils[0].CourseMarks.First(m=>m.Subject==sub).Lessons,0);Assert.AreEqual("done",sim.AddSubject("Dänisch","Danish",SubjectGroup.Languages,RoomKind.LanguageLab,2).Code);Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));}
        [Test] public void AdvancedCoursesChargePerPupilAndLessonWithoutUnstaffedCharges()
        {sim.Hire(0);SchoolClass c=sim.State.Classes[0];sim.SetWeeklyLesson(c.RoomId,0,0,Subject.APPhysics1);sim.SetCourseLevel(c.RoomId,3);Assert.AreEqual(18*4,sim.Forecast().Curriculum);sim.AssignTeacher(c.RoomId,0);Assert.AreEqual(0,sim.Forecast().Curriculum);}
        [Test] public void SpecialistRoomsReserveOneWholeClassForEachLesson()
        {ReadyAcademy();Assert.IsTrue(BuildSomewhere(RoomKind.ScienceLab));sim.SetLesson(0,Subject.Physics);SchoolClass first=sim.State.Classes.OrderBy(c=>c.RoomId).First(),second=sim.State.Classes.OrderBy(c=>c.RoomId).Last();Assert.NotNull(sim.SpecialistRoom(first,Subject.Physics,0));Assert.IsNull(sim.SpecialistRoom(second,Subject.Physics,0));}
        [Test] public void TowersHaveDistinctFloorCoordinatesAndRealVerticalPaths()
        {TowerReady();Cell ground=new Cell(16,10),upper=new Cell(16,10,1);Assert.AreNotEqual(ground,upper);Assert.Greater(CampusGrid.Path(sim.State,CampusGrid.Entrance,upper).Count,0);Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));Assert.IsTrue(sim.State.Rooms.Any(r=>r.Floor==1&&r.Kind==RoomKind.Classroom));}
        [Test] public void FloatingRoomsMisalignedStairsAndSeventhFloorsAreRejectedWithoutCharge()
        {TowerReady();int cash=sim.State.Cash;Assert.AreEqual("support",sim.Build(RoomKind.Classroom,20,15,false,1).Code);Assert.AreEqual("stairs",sim.Build(RoomKind.Staircase,12,10,false,2).Code);Assert.AreEqual("floor",sim.Build(RoomKind.Corridor,12,10,false,6).Code);Assert.AreEqual("outdoors",sim.Build(RoomKind.Garden,12,10,false,1).Code);Assert.AreEqual(cash,sim.State.Cash);}
        [Test] public void SupportingFloorsAndTheOnlyStairRouteCannotBeDemolished()
        {TowerReady();Room lower=sim.State.Rooms.First(r=>r.Kind==RoomKind.Staircase&&r.Floor==0),upper=sim.State.Rooms.First(r=>r.Kind==RoomKind.Staircase&&r.Floor==1);Assert.AreEqual("supports",sim.Demolish(lower.Id).Code);Assert.AreEqual("disconnect",sim.Demolish(upper.Id).Code);}
        [Test] public void TowerCostsAndUpkeepIncreaseWithFloorHeight()
        {Assert.AreEqual(4320,sim.BuildCost(RoomKind.Classroom,1));Room a=new Room{Kind=RoomKind.Classroom},b=new Room{Kind=RoomKind.Classroom,Floor=5};Assert.Greater(sim.RoomRunningCost(b),sim.RoomRunningCost(a));}
        [Test] public void GraduationCanExtendToGradeThirteenAndCannotSkipOlderPupils()
        {Assert.IsTrue(sim.SetSchoolPath(13).Success);sim.State.Pupils[0].Grade=12;Assert.AreEqual("grade",sim.SetSchoolPath(6).Code);Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));}
        [Test] public void VersionThreeMigrationRetainsScoresClassesAndExistingReports()
        {ReadyAcademy();RunDays(20);sim.State.SaveVersion=3;Pupil p=sim.State.Pupils[0];p.Scores[0]=42;int reports=sim.State.Reports.Count;Assert.IsTrue(SchoolSimulation.MigrateSave(sim.State));Assert.AreEqual(42,p.Scores[0]);Assert.AreEqual(reports,sim.State.Reports.Count);Assert.AreEqual(30,sim.State.Classes[0].Week.Count);}
        [Test] public void NewSaveValidationRejectsFloatingFloorsAndInvalidCourseScores()
        {sim.Hire(0);sim.SetLesson(0,Subject.Physics);Pupil p=sim.State.Pupils[0];p.CourseMarks[0].Score=101;Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));p.CourseMarks[0].Score=50;sim.State.Rooms[1].Floor=1;Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));}
        [Test] public void AnnualProjectsPayOnceAfterTenConsecutiveSustainableDays()
        {ReadyAcademy();sim.ApplyCurriculum(CurriculumRegion.Germany);Assert.IsTrue(sim.StartCareer(0).Success);int cash=sim.State.Cash;Assert.AreEqual("busy",sim.StartCareer(0).Code);Assert.AreEqual(cash,sim.State.Cash);RunDays(35);Assert.AreEqual(1,sim.State.Prestige);Assert.AreEqual(1,sim.State.CareerHistory.Count);Assert.AreEqual("done",sim.StartCareer(0).Code);}
        [Test] public void AnnualProjectDeadlinesPreventUnfinishableLateStarts()
        {ReadyAcademy();sim.State.Day=52;Assert.AreEqual("lateyear",sim.StartCareer(0).Code);sim.State.Day=50;sim.StartCareer(0);RunDays(11);Assert.AreEqual(-1,sim.State.ActiveContract);Assert.AreEqual(0,sim.State.Prestige);}
        [Test] public void EachRegionalCurriculumRunsThreeHundredSixtyDaysSolvent()
        {foreach(CurriculumRegion region in new[]{CurriculumRegion.Germany,CurriculumRegion.England,CurriculumRegion.USA}){Setup();ReadyAcademy();sim.ApplyCurriculum(region);sim.SetWellbeingPolicy(2,1);foreach(Employee e in sim.State.Staff.Where(e=>e.Role==StaffRole.Teacher))sim.TrainStaff(e.Id);sim.SetClub(ClubKind.Reading,true);RunDays(360);Assert.Greater(sim.State.Cash,10000,region.ToString());Assert.IsFalse(sim.State.GameOver);}}
        [Test] public void AllSeventeenRoomTypesAndCharactersHaveValidDetailedArtRecipes()
        {foreach(RoomSpec s in Catalog.Rooms){var parts=CampusDesign.Room(new Room{Kind=s.Kind,Width=s.Width,Height=s.Height});Assert.Greater(parts.Count,0,s.NameEn);Assert.IsTrue(parts.All(p=>p.W>0&&p.H>0&&p.D>0&&p.Color.Length==6),s.NameEn);}Assert.Greater(CampusDesign.Person(1,false).Count,30);Assert.IsTrue(CampusDesign.Room(new Room{Kind=RoomKind.MusicRoom,Width=4,Height=3}).Any(p=>p.Name=="Ivory piano key"));}
        [Test] public void CreativeClubsCannotBoostEverySubjectAtOnce()
        {ReadyAcademy();sim.State.CompletedResearch.Add(ResearchKind.CreativeCampus);sim.SetWellbeingPolicy(0,1);foreach(SchoolClass c in sim.State.Classes)for(int i=0;i<6;i++)sim.SetClassLesson(c.RoomId,i,Subject.Mathematics);SchoolClass group=sim.State.Classes[0];Subject[] extras={Subject.Physics,Subject.History,Subject.English,Subject.Music,Subject.Geography,Subject.DesignTechnology};for(int i=0;i<6;i++)sim.SetWeeklyLesson(group.RoomId,1,i,extras[i]);sim.SetClub(ClubKind.Reading,true);sim.SetClub(ClubKind.Gardening,true);Pupil p=sim.State.Pupils.First();foreach(Subject sub in extras)sim.SetScore(p,sub,30);sim.AdvanceDay();Assert.LessOrEqual(extras.Sum(sub=>sim.Score(p,sub)-30),2);}
    }
}

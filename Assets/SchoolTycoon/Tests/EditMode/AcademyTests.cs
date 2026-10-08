using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using KoSch.SchoolTycoon.Core;

namespace KoSch.SchoolTycoon.Tests
{
    public partial class SchoolSimulationTests
    {
        private void ReadyAcademy()
        {
            sim.Hire(0); sim.Hire(3); sim.Build(RoomKind.Toilet, 16, 6); sim.Build(RoomKind.Staffroom, 12, 6);
            sim.Build(RoomKind.Classroom, 16, 10); sim.Hire(1); sim.Build(RoomKind.Canteen, 18, 6);
            sim.Build(RoomKind.Corridor, 11, 9); sim.Build(RoomKind.Corridor, 11, 10);
            sim.Build(RoomKind.Library, 7, 10); sim.Build(RoomKind.Garden, 8, 7);
            Assert.AreEqual(4, sim.State.CompletedGoals);
        }
        private void RunDays(int days)
        {
            for (int i = 0; i < days; i++)
            {
                if (sim.State.PendingEvent >= 0) Assert.IsTrue(sim.ChooseEvent(2).Success);
                Assert.IsTrue(sim.AdvanceDay().Success);
                Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State), "Save invalid after day " + (sim.State.Day - 1));
            }
            if (sim.State.PendingEvent >= 0) sim.ChooseEvent(2);
        }
        [Test] public void StarterPupilsHavePersistentUniqueProfilesAndAHomeClass()
        {
            Assert.AreEqual(18, sim.State.Pupils.Count);
            Assert.AreEqual(18, sim.State.Pupils.Select(p => p.Id).Distinct().Count());
            Assert.IsTrue(sim.State.Pupils.All(p => p.ClassRoomId == 2 && p.Scores.Count == 5));
            var first = sim.State.Pupils.First(); sim.Hire(0); sim.AdvanceDay();
            Assert.AreSame(first, sim.State.Pupils.First(p => p.Id == first.Id));
        }
        [Test] public void TeachersCannotLeadTwoClassesAndFundingFollowsActualAssignments()
        {
            sim.Hire(0); sim.Build(RoomKind.Classroom, 16, 10);
            int teacherId = sim.State.Staff[0].Id, otherRoom = sim.State.Classes.Last().RoomId;
            Assert.IsTrue(sim.AssignTeacher(otherRoom, teacherId).Success);
            Assert.AreEqual(0, sim.State.Classes.First().TeacherId);
            Assert.AreEqual(24, sim.Capacity); Assert.AreEqual(0, sim.Forecast().Funding);
            Assert.AreEqual("invalid", sim.AssignTeacher(otherRoom, 999).Code);
            Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
        }
        [Test] public void ManualTeacherChoicesSurviveUnrelatedBuildingAndSupportHiring()
        {
            ReadyAcademy(); sim.AssignTeacher(2, 0);
            Assert.IsTrue(sim.Build(RoomKind.Corridor, 11, 8).Success);
            Assert.AreEqual(0, sim.State.Classes.First(c => c.RoomId == 2).TeacherId);
            Assert.IsTrue(sim.Hire(4).Success);
            Assert.AreEqual(0, sim.State.Classes.First(c => c.RoomId == 2).TeacherId);
            int teacher = sim.State.Staff.First(p => p.Role == StaffRole.Teacher).Id;
            Assert.IsTrue(sim.AssignTeacher(2, teacher).Success);
            Assert.AreEqual(teacher, sim.State.Classes.First(c => c.RoomId == 2).TeacherId);
        }
        [Test] public void EachClassHasItsOwnPlanAndTheGlobalTemplateCanResetAllClasses()
        {
            ReadyAcademy(); int room = sim.State.Classes.Last().RoomId;
            Subject original = sim.State.Classes.First().Lessons[0];
            Assert.IsTrue(sim.SetClassLesson(room, 0, Subject.Arts).Success);
            Assert.AreEqual(original, sim.State.Classes.First().Lessons[0]);
            Assert.AreEqual(Subject.Arts, sim.State.Classes.Last().Lessons[0]);
            sim.SetLesson(0, Subject.Sports); Assert.IsTrue(sim.State.Classes.All(c => c.Lessons[0] == Subject.Sports));
            Assert.IsFalse(sim.SetClassLesson(room, 7, Subject.Arts).Success);
        }
        [Test] public void PupilTransfersRespectPhysicalSeatsAndRetainIndividualProgress()
        {
            ReadyAcademy(); RunDays(8);
            Pupil pupil = sim.State.Pupils.First(); int target = sim.State.Classes.Last().RoomId;
            Assert.AreEqual("full", sim.AssignPupil(pupil.Id, target).Code);
            sim.UpgradeRoom(target);
            List<int> before = new List<int>(pupil.Scores);
            Assert.IsTrue(sim.AssignPupil(pupil.Id, target).Success);
            Assert.AreEqual(target, pupil.ClassRoomId); CollectionAssert.AreEqual(before, pupil.Scores);
        }
        [Test] public void ClassNamesAreValidatedAndPersistInTheModel()
        {
            Assert.IsTrue(sim.SetClassName(2, "  Sonnenklasse  ").Success);
            Assert.AreEqual("Sonnenklasse", sim.State.Classes.First().Name);
            Assert.IsFalse(sim.SetClassName(2, " ").Success);
            Assert.IsFalse(sim.SetClassName(2, new string('x', 21)).Success);
        }
        [Test] public void IndividualSupportIsPaidOncePerDayAndTargetsTheWeakestSubject()
        {
            sim.Hire(0); Pupil p = sim.State.Pupils.First(); p.Scores = new List<int> { 70, 20, 60, 55, 50 };
            int cash = sim.State.Cash;
            Assert.IsTrue(sim.SupportPupil(p.Id, 0).Success); Assert.AreEqual(28, p.Scores[1]);
            Assert.AreEqual(cash - 90, sim.State.Cash);
            Assert.AreEqual("done", sim.SupportPupil(p.Id, 1).Code); Assert.AreEqual(cash - 90, sim.State.Cash);
            sim.AdvanceDay(); Assert.IsTrue(sim.SupportPupil(p.Id, 2).Success);
        }
        [Test] public void FreeConversationWorksInDebtWithoutCreatingMoney()
        {
            Pupil p = sim.State.Pupils.First(); sim.State.Cash = -100;
            Assert.AreEqual("funds", sim.SupportPupil(p.Id, 1).Code);
            Assert.IsTrue(sim.SupportPupil(p.Id, 2).Success); Assert.AreEqual(-100, sim.State.Cash);
            Assert.AreEqual(14, p.Stress); Assert.AreEqual(68, p.Wellbeing);
        }
        [Test] public void UntaughtSubjectsDoNotGainClassroomProgress()
        {
            sim.Hire(0); for (int i = 0; i < 6; i++) sim.SetLesson(i, Subject.Mathematics);
            Pupil p = sim.State.Pupils.First(); p.Scores = new List<int> { 10, 30, 30, 30, 30 };
            sim.SetWellbeingPolicy(0, 1); sim.AdvanceDay();
            Assert.Greater(p.Scores[0], 10); Assert.AreEqual(30, p.Scores[1]); Assert.AreEqual(30, p.Scores[2]);
        }
        [Test] public void StressCanCauseAbsenceAndAbsentPupilsMissLessons()
        {
            sim.Hire(0); sim.SetWellbeingPolicy(0, 1); Pupil p = sim.State.Pupils[2]; p.Stress = 100; p.Wellbeing = 10;
            sim.AdvanceDay(); Assert.IsTrue(p.Absent);
            var scores = new List<int>(p.Scores); int attended = p.DaysAttended;
            sim.AdvanceDay(); CollectionAssert.AreEqual(scores, p.Scores); Assert.AreEqual(attended, p.DaysAttended);
            Assert.Greater(sim.State.DailyReport.Absent, 0);
        }
        [Test] public void RoutesUseAssignedClassAndItsSpecialistSubject()
        {
            ReadyAcademy(); Assert.IsTrue(BuildSomewhere(RoomKind.ScienceLab));
            Pupil p = sim.State.Pupils.First();
            sim.SetClassLesson(2, 0, Subject.Mathematics); Assert.AreEqual(2, sim.Destination(p, null, 480, p.Id).Id);
            sim.SetClassLesson(2, 0, Subject.Science); Assert.AreEqual(RoomKind.ScienceLab, sim.Destination(p, null, 480, p.Id).Kind);
            Employee teacher = sim.ClassTeacher(sim.PupilClass(p)); Assert.AreEqual(RoomKind.ScienceLab, sim.Destination(null, teacher, 480, 0).Kind);
            sim.AssignTeacher(2, 0); Assert.AreEqual(2, sim.Destination(p, null, 480, p.Id).Id);
        }
        [Test] public void LunchClubsAndRainHaveDistinctDestinations()
        {
            ReadyAcademy(); Pupil p = sim.State.Pupils.First();
            Assert.AreEqual(RoomKind.Canteen, sim.Destination(p, null, 720, 0).Kind);
            Assert.AreEqual(RoomKind.Garden, sim.Destination(p, null, 900, 0).Kind);
            sim.State.Day = 2; Assert.AreEqual(WeatherKind.Rainy, sim.Weather);
            Assert.AreEqual(RoomKind.Library, sim.Destination(p, null, 900, 0).Kind);
            sim.SetClub(ClubKind.Reading, true); Assert.AreEqual(RoomKind.Library, sim.Destination(p, null, 900, 0).Kind);
            p.Absent = true; Assert.IsNull(sim.Destination(p, null, 480, 0));
        }
        [Test] public void LessonClockSkipsLunchAndEndsBeforeClubs()
        {
            Assert.AreEqual(0, SchoolSimulation.LessonSlot(480)); Assert.AreEqual(3, SchoolSimulation.LessonSlot(719));
            Assert.AreEqual(-1, SchoolSimulation.LessonSlot(720)); Assert.AreEqual(4, SchoolSimulation.LessonSlot(780));
            Assert.AreEqual(5, SchoolSimulation.LessonSlot(840)); Assert.AreEqual(-1, SchoolSimulation.LessonSlot(900));
        }
        [Test] public void RoomUpgradesAddStaffedSeatsAndRecurringCostsWithoutRepeatedBonuses()
        {
            sim.Hire(0); int cash = sim.State.Cash, maintenance = sim.Forecast().Maintenance;
            Assert.IsTrue(sim.UpgradeRoom(2).Success); Assert.AreEqual(cash - 1200, sim.State.Cash);
            Assert.AreEqual(28, sim.Capacity); Assert.AreEqual(maintenance + 8, sim.Forecast().Maintenance);
            Assert.IsTrue(sim.UpgradeRoom(2).Success); Assert.AreEqual(32, sim.Capacity);
            Assert.AreEqual(1 << 5, sim.State.AchievementMask);
            cash = sim.State.Cash; Assert.AreEqual("maxed", sim.UpgradeRoom(2).Code); Assert.AreEqual(cash, sim.State.Cash);
        }
        [Test] public void WearAndRepairsChargeOnlyForActualDamage()
        {
            sim.Hire(0); RunDays(2); Room room = sim.State.Rooms.First(r => r.Id == 2);
            Assert.AreEqual(98, room.Condition); int cost = sim.RepairCost(room), cash = sim.State.Cash;
            Assert.IsTrue(sim.RepairRoom(2).Success); Assert.AreEqual(cash - cost, sim.State.Cash); Assert.AreEqual(100, room.Condition);
            cash = sim.State.Cash; sim.RepairRoom(2); Assert.AreEqual(cash, sim.State.Cash);
        }
        [Test] public void CaretakersPreventWearAndStaffroomsRecoverTeacherEnergy()
        {
            ReadyAcademy(); Employee teacher = sim.State.Staff.First(p => p.Role == StaffRole.Teacher); teacher.Energy = 30;
            RunDays(4); Assert.Greater(teacher.Energy, 30); Assert.IsTrue(sim.State.Rooms.All(r => r.Condition == 100));
        }
        [Test] public void StaffTrainingImprovesSkillsAndRaisesDailyPayWithinALimit()
        {
            sim.Hire(0); Employee p = sim.State.Staff.First(); int skill = p.Skill, salary = p.Salary, cash = sim.State.Cash;
            Assert.IsTrue(sim.TrainStaff(p.Id).Success); Assert.AreEqual(Math.Min(100, skill + 8), p.Skill); Assert.AreEqual(salary + 5, p.Salary); Assert.AreEqual(cash - 500, sim.State.Cash);
            sim.TrainStaff(p.Id); sim.TrainStaff(p.Id); cash = sim.State.Cash;
            Assert.AreEqual("maxed", sim.TrainStaff(p.Id).Code); Assert.AreEqual(cash, sim.State.Cash);
            int effective = sim.EffectiveSkill(p); p.Energy = 0; p.Morale = 0; Assert.Less(sim.EffectiveSkill(p), effective);
        }
        [Test] public void ResearchRequiresFacilitiesChargesOnceAndFinishesOverSeveralDays()
        {
            Assert.AreEqual("researchlocked", sim.StartResearch(ResearchKind.DigitalLearning).Code);
            ReadyAcademy(); int cash = sim.State.Cash;
            Assert.IsTrue(sim.StartResearch(ResearchKind.DigitalLearning).Success); Assert.AreEqual(cash - 2200, sim.State.Cash);
            Assert.AreEqual("busy", sim.StartResearch(ResearchKind.GreenCampus).Code);
            RunDays(10); Assert.IsTrue(sim.Researched(ResearchKind.DigitalLearning)); Assert.AreEqual(-1, sim.State.ActiveResearch);
            cash = sim.State.Cash; Assert.AreEqual("done", sim.StartResearch(ResearchKind.DigitalLearning).Code); Assert.AreEqual(cash, sim.State.Cash);
        }
        [Test] public void ResearchPausesIfItsLibraryIsRemoved()
        {
            ReadyAcademy(); sim.StartResearch(ResearchKind.GreenCampus);
            Room library = sim.State.Rooms.First(r => r.Kind == RoomKind.Library); sim.Demolish(library.Id);
            RunDays(2); Assert.AreEqual(0, sim.State.ResearchProgress); Assert.AreEqual(0, sim.State.DailyReport.ResearchGained);
        }
        [Test] public void GreenResearchActuallyReducesDailyRoomExpenses()
        {
            ReadyAcademy(); int normal = sim.Forecast().Maintenance;
            sim.StartResearch(ResearchKind.GreenCampus); RunDays(8);
            Assert.IsTrue(sim.Researched(ResearchKind.GreenCampus)); Assert.AreEqual(normal * 80 / 100, sim.Forecast().Maintenance);
        }
        [Test] public void ClubsKeepEquipmentAcrossPausesAndOnlyChargeActiveDailyCosts()
        {
            ReadyAcademy(); int cash = sim.State.Cash;
            Assert.IsTrue(sim.SetClub(ClubKind.Reading, true).Success); Assert.AreEqual(cash - 450, sim.State.Cash);
            Assert.AreEqual(22, sim.Forecast().Activities); cash = sim.State.Cash;
            sim.SetClub(ClubKind.Reading, true); Assert.AreEqual(cash, sim.State.Cash);
            sim.SetClub(ClubKind.Reading, false); Assert.AreEqual(0, sim.Forecast().Activities);
            sim.SetClub(ClubKind.Reading, true); Assert.AreEqual(cash, sim.State.Cash);
            sim.Demolish(sim.State.Rooms.First(r => r.Kind == RoomKind.Library).Id); Assert.AreEqual(0, sim.Forecast().Activities);
        }
        [Test] public void ClubLearningReachesPupilsEvenWhenItsSubjectIsNotScheduled()
        {
            ReadyAcademy(); for (int i = 0; i < 6; i++) sim.SetLesson(i, Subject.Mathematics);
            sim.SetWellbeingPolicy(0, 1); sim.SetClub(ClubKind.Reading, true); Pupil p = sim.State.Pupils.First(); p.Scores[1] = 30;
            sim.AdvanceDay(); Assert.AreEqual(31, p.Scores[1]);
        }
        [Test] public void WellbeingPoliciesAreIncludedInTheRecurringForecast()
        {
            ReadyAcademy(); sim.SetWellbeingPolicy(2, 2); Ledger f = sim.Forecast();
            Assert.AreEqual(sim.State.Students * 3, f.Support); Assert.AreEqual(f.Salaries + f.Maintenance + f.Supplies + f.Activities + f.Support, f.Expenses);
            Assert.IsFalse(sim.SetWellbeingPolicy(3, 1).Success);
        }
        [Test] public void TermsProduceReportCardsAndTheSchoolYearPromotesPupils()
        {
            ReadyAcademy(); Pupil p = sim.State.Pupils.First(); RunDays(60);
            Assert.AreEqual(3, sim.State.Reports.Count); Assert.AreEqual(3, sim.State.ExamsHeld);
            Assert.AreEqual(2, p.Grade); Assert.AreEqual(2, sim.SchoolYear); Assert.AreEqual(1, sim.YearDay);
            Assert.AreEqual(3, sim.State.Reports.Last().Term); Assert.AreEqual(5, sim.State.Reports.Last().SubjectAverages.Count);
        }
        [Test] public void GraduationRemovesOnlySixthGradeGraduatesAndPreservesPromotedFifthGraders()
        {
            ReadyAcademy(); Pupil senior = sim.State.Pupils[0], junior = sim.State.Pupils[1]; senior.Grade = 6; junior.Grade = 5;
            senior.Scores = new List<int> { 80,80,80,80,80 }; junior.Scores = new List<int> { 80,80,80,80,80 }; sim.State.Day = 60;
            sim.AdvanceDay(); Assert.AreEqual(1, sim.State.Graduates); Assert.IsFalse(sim.State.Pupils.Any(p => p.Id == senior.Id));
            Assert.IsTrue(sim.State.Pupils.Contains(junior)); Assert.AreEqual(6, junior.Grade);
            Assert.AreEqual(1, sim.State.Reports.Last().Graduated); Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
        }
        [Test] public void AWeakYearEndExamKeepsThePupilInTheSameGrade()
        {
            ReadyAcademy(); Pupil p = sim.State.Pupils.First(); p.Scores = new List<int> { 0,0,0,0,0 }; sim.State.Day = 60;
            sim.AdvanceDay(); Assert.AreEqual(1, p.Grade); Assert.Greater(sim.State.Reports.Last().Repeated, 0);
        }
        [Test] public void TwoResearchProjectsAwardTheirAchievementOnlyOnce()
        {
            ReadyAcademy(); sim.StartResearch(ResearchKind.GreenCampus); RunDays(8); sim.StartResearch(ResearchKind.DigitalLearning); RunDays(10);
            Assert.IsTrue((sim.State.AchievementMask & (1 << 1)) != 0); int cash = sim.State.Cash;
            sim.SetClub(ClubKind.Gardening, true); int after = sim.State.Cash; sim.SetClub(ClubKind.Gardening, true);
            Assert.AreEqual(cash - 350, after); Assert.AreEqual(after, sim.State.Cash);
        }
        [Test] public void LegacyMigrationRetainsTheCampusMoneyDirectorAndTimetable()
        {
            ReadyAcademy(); SchoolState s = sim.State; s.SaveVersion = 2; s.Day = 37; s.Cash = 12345;
            s.HasDirector = true; s.Director.Name = "Robin"; s.Director.Hair = "Silver"; s.ClockMinute = 987;
            int rooms = s.Rooms.Count, staff = s.Staff.Count; s.Pupils = null; s.Classes = null; s.CompletedResearch = null; s.ClubLicenses = null;
            Assert.IsTrue(SchoolSimulation.MigrateSave(s)); Assert.AreEqual(3, s.SaveVersion); Assert.AreEqual(12345, s.Cash);
            Assert.AreEqual(rooms, s.Rooms.Count); Assert.AreEqual(staff, s.Staff.Count); Assert.AreEqual("Robin", s.Director.Name); Assert.AreEqual(987, s.ClockMinute);
            Assert.AreEqual(s.Students, s.Pupils.Count); Assert.AreEqual(2, s.Classes.Count);
            Assert.IsTrue(SchoolSimulation.MigrateSave(s)); Assert.AreEqual(12345, s.Cash);
        }
        [Test] public void CorruptLegacyAndUnknownVersionsAreRejectedBeforeMigration()
        {
            SchoolState s = sim.State; s.SaveVersion = 2; s.Rooms[1].X = 12; s.Rooms[1].Y = 8;
            Assert.IsFalse(SchoolSimulation.MigrateSave(s)); Assert.AreEqual(2, s.SaveVersion);
            s = SchoolState.Create(); s.SaveVersion = 99; Assert.IsFalse(SchoolSimulation.MigrateSave(s));
        }
        [Test] public void NewSaveValidationRejectsBrokenRostersScoresAssignmentsAndResearch()
        {
            ReadyAcademy(); Pupil p = sim.State.Pupils.First(); p.Scores[0] = 101; Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State)); p.Scores[0] = 50;
            int id = p.Id; p.Id = sim.State.Pupils[1].Id; Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State)); p.Id = id;
            int teacher = sim.State.Classes[1].TeacherId; sim.State.Classes[1].TeacherId = sim.State.Classes[0].TeacherId;
            Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State)); sim.State.Classes[1].TeacherId = teacher;
            sim.State.ActiveResearch = 0; sim.State.CompletedResearch.Add(ResearchKind.DigitalLearning); Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
            sim.State.CompletedResearch.Clear(); sim.State.ActiveResearch = -1; Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
        }
        [Test] public void EveryEventHasThreeAffordablePathsAndCanOnlyBeSettledOnce()
        {
            Assert.AreEqual(12, SchoolSimulation.Events.Length);
            for (int i = 0; i < SchoolSimulation.Events.Length; i++)
            {
                SchoolEvent e = SchoolSimulation.Events[i]; Assert.AreEqual(3, e.OptionsDe.Length); Assert.AreEqual(3, e.OptionsEn.Length); Assert.AreEqual(0, e.Costs[2]);
                var school = new SchoolSimulation(SchoolState.Create()); school.State.PendingEvent = i; school.State.Cash = -10;
                Assert.IsTrue(school.ChooseEvent(2).Success); Assert.AreEqual(-10, school.State.Cash); Assert.IsFalse(school.ChooseEvent(2).Success);
                Assert.IsTrue(SchoolSimulation.ValidateSave(school.State));
            }
        }
        [Test] public void ALongCampaignGraduatesCohortsAndKeepsBoundedValidRecords()
        {
            ReadyAcademy(); sim.SetWellbeingPolicy(2, 1); sim.SetClub(ClubKind.Reading, true); sim.SetClub(ClubKind.Gardening, true);
            foreach (Employee e in sim.State.Staff.Where(e => e.Role == StaffRole.Teacher)) { sim.TrainStaff(e.Id); sim.TrainStaff(e.Id); sim.TrainStaff(e.Id); }
            RunDays(660); Assert.Greater(sim.State.Graduates, 0); Assert.AreEqual(30, sim.State.Reports.Count); Assert.AreEqual(60, sim.State.History.Count);
            Assert.AreEqual(sim.State.Students, sim.State.Pupils.Count); Assert.LessOrEqual(sim.State.Students, sim.Capacity); Assert.IsFalse(sim.State.GameOver);
        }
        [Test] public void FullCampusCanRunAllClubsCompleteEveryResearchAndContinueInSandbox()
        {
            AllSevenGrantsCanBeReachedInASustainableCampaign();
            if (sim.State.PendingEvent >= 0) sim.ChooseEvent(2);
            foreach (ClubKind kind in Enum.GetValues(typeof(ClubKind))) Assert.IsTrue(sim.SetClub(kind, true).Success);
            Assert.AreEqual(5, sim.ActiveClubCount);
            foreach (ResearchKind kind in Enum.GetValues(typeof(ResearchKind)))
            { Assert.IsTrue(sim.StartResearch(kind).Success); RunDays(15); Assert.IsTrue(sim.Researched(kind)); }
            Assert.IsTrue(sim.UpgradeRoom(2).Success); Assert.IsTrue(sim.UpgradeRoom(2).Success); RunDays(20);
            int milestones = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 5);
            Assert.AreEqual(milestones, sim.State.AchievementMask & milestones);
            Assert.AreEqual(7, sim.State.CompletedGoals); Assert.AreEqual(4, sim.State.CompletedResearch.Count);
            Assert.IsFalse(sim.State.GameOver); Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
        }
    }
}

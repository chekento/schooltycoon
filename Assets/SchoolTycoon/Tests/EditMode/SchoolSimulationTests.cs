using System;
using System.Linq;
using NUnit.Framework;
using KoSch.SchoolTycoon.Core;

namespace KoSch.SchoolTycoon.Tests
{
    public class SchoolSimulationTests
    {
        private SchoolSimulation sim;
        [SetUp] public void Setup() { sim = new SchoolSimulation(SchoolState.Create()); }

        [Test] public void StarterCampusIsValidAndHasNoUnstaffedCapacity()
        {
            Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
            Assert.AreEqual(24, sim.PhysicalCapacity);
            Assert.AreEqual(0, sim.Capacity);
            Assert.AreEqual(18, sim.State.Students);
        }
        [Test] public void BuildingChargesOnceAndRefusedPlacementDoesNotSpend()
        {
            int before = sim.State.Cash;
            Assert.IsTrue(sim.Build(RoomKind.Toilet, 10, 8).Success);
            Assert.AreEqual(before - 1100, sim.State.Cash);
            int after = sim.State.Cash;
            Assert.AreEqual("occupied", sim.Build(RoomKind.Toilet, 10, 8).Code);
            Assert.AreEqual(after, sim.State.Cash);
            Ledger f = sim.Forecast();
            Assert.AreEqual(62, f.Maintenance); // 12 corridor tiles + starter classroom + restroom.
            Assert.AreEqual(0, f.Funding);
        }
        [Test] public void IsolatedAndUnownedRoomsAreRejected()
        {
            Assert.AreEqual("connection", sim.Build(RoomKind.Classroom, 20, 15).Code);
            Assert.AreEqual("land", sim.Build(RoomKind.Classroom, 0, 0).Code);
            Assert.AreEqual(2, sim.State.Rooms.Count);
        }
        [Test] public void RotatedRoomsUseTheirRealFootprint()
        {
            Assert.IsTrue(sim.Build(RoomKind.Staffroom, 18, 8, true).Success);
            Room r = sim.State.Rooms.Last();
            Assert.AreEqual(2, r.Width); Assert.AreEqual(3, r.Height);
            Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
        }
        [Test] public void ATeacherAddsStaffedSeatsAndTheSameCandidateCannotBeHiredTwice()
        {
            Assert.IsTrue(sim.Hire(0).Success);
            Assert.AreEqual(24, sim.Capacity);
            int after = sim.State.Cash;
            Assert.AreEqual("hired", sim.Hire(0).Code);
            Assert.AreEqual(after, sim.State.Cash);
            Assert.AreEqual(1, sim.State.CompletedGoals);
        }
        [Test] public void GrantsAreAwardedOnlyOnce()
        {
            int before = sim.State.Cash;
            sim.Hire(0);
            Assert.AreEqual(before - 700 + 1400, sim.State.Cash);
            int after = sim.State.Cash;
            sim.CheckGoals(); sim.CheckGoals();
            Assert.AreEqual(after, sim.State.Cash);
        }
        [Test] public void GrantsUnlockAmenitiesInOrder()
        {
            Assert.AreEqual("locked", sim.Build(RoomKind.Canteen, 18, 6).Code);
            sim.Hire(0); sim.Build(RoomKind.Toilet, 10, 8); sim.Build(RoomKind.Staffroom, 12, 6);
            Assert.AreEqual(2, sim.State.CompletedGoals);
            Assert.AreEqual(1, sim.State.Level);
            Assert.IsTrue(sim.Build(RoomKind.Canteen, 18, 6).Success);
        }
        [Test] public void DailyLedgerIncludesOnlyRecurringExpensesAndExistingStudents()
        {
            sim.Hire(0); sim.Build(RoomKind.Toilet, 10, 8);
            Ledger forecast = sim.Forecast();
            int cash = sim.State.Cash;
            Assert.IsTrue(sim.AdvanceDay().Success);
            Ledger actual = sim.State.LastLedger;
            Assert.AreEqual(18 * 28, actual.Funding);
            Assert.AreEqual(72, actual.Supplies);
            Assert.AreEqual(forecast.Income - forecast.Expenses, actual.Net);
            Assert.AreEqual(cash + actual.Net, sim.State.Cash);
            Assert.AreEqual(18, actual.Students);
            Assert.LessOrEqual(sim.State.Students, sim.Capacity);
        }
        [Test] public void OnlyCurrentTimetableSubjectsReceiveSpecialistFacilityBonuses()
        {
            for (int i = 0; i < 6; i++) Assert.IsTrue(sim.SetLesson(i, Subject.Arts).Success);
            Assert.AreEqual(1, sim.State.Timetable.Distinct().Count());
            Assert.IsFalse(sim.SetLesson(6, Subject.Arts).Success);
            Assert.IsFalse(sim.SetLesson(0, (Subject)900).Success);
        }
        [Test] public void FiringTeacherRemovesCapacityWithoutInstantlyDeletingStudents()
        {
            sim.Hire(0); int students = sim.State.Students;
            Assert.IsTrue(sim.Fire(sim.State.Staff[0].Id).Success);
            Assert.AreEqual(0, sim.Capacity); Assert.AreEqual(students, sim.State.Students);
            sim.AdvanceDay(); Assert.AreEqual(students - 4, sim.State.Students);
        }
        [Test] public void EssentialRoomsAndConnectingCorridorsCannotBeDemolished()
        {
            Assert.AreEqual("foundation", sim.Demolish(1).Code);
            Assert.AreEqual("foundation", sim.Demolish(2).Code);
            sim.Build(RoomKind.Corridor, 11, 9); int connector = sim.State.Rooms.Last().Id;
            sim.Build(RoomKind.Corridor, 11, 10); int end = sim.State.Rooms.Last().Id;
            Assert.AreEqual("disconnect", sim.Demolish(connector).Code);
            Assert.IsTrue(sim.Demolish(end).Success);
            Assert.IsTrue(sim.Demolish(connector).Success);
        }
        [Test] public void RemovingRoomRefundsHalfOnce()
        {
            sim.Build(RoomKind.Toilet, 10, 8); int id = sim.State.Rooms.Last().Id, cash = sim.State.Cash;
            Assert.IsTrue(sim.Demolish(id).Success); Assert.AreEqual(cash + 550, sim.State.Cash);
            Assert.IsFalse(sim.Demolish(id).Success); Assert.AreEqual(cash + 550, sim.State.Cash);
        }
        [Test] public void PathsUseCorridorsAndCannotCutThroughClassroomWalls()
        {
            sim.Build(RoomKind.Corridor, 11, 9); sim.Build(RoomKind.Corridor, 11, 10);
            var path = CampusGrid.Path(sim.State, CampusGrid.Entrance, new Cell(11, 10));
            Assert.Greater(path.Count, 0);
            Assert.IsTrue(path.All(c => CampusGrid.At(sim.State, c).Kind == RoomKind.Corridor));
            Assert.AreEqual(0, CampusGrid.Path(sim.State, CampusGrid.Entrance, new Cell(12, 10)).Count);
        }
        [Test] public void UnaffordableEventKeepsPendingChoiceAndFreeChoiceAlwaysWorks()
        {
            sim.State.PendingEvent = 0; sim.State.Cash = -10;
            Assert.AreEqual("funds", sim.ChooseEvent(0).Code);
            Assert.AreEqual(0, sim.State.PendingEvent);
            Assert.IsTrue(sim.ChooseEvent(2).Success);
            Assert.AreEqual(-10, sim.State.Cash);
            Assert.AreEqual(-1, sim.State.PendingEvent);
            Assert.IsFalse(sim.ChooseEvent(2).Success);
        }
        [Test] public void EventsBlockBuildingAndDailySettlementUntilResolved()
        {
            sim.State.PendingEvent = 0;
            Assert.AreEqual("blocked", sim.Build(RoomKind.Toilet, 10, 8).Code);
            Assert.AreEqual("blocked", sim.AdvanceDay().Code);
            sim.ChooseEvent(2); Assert.IsTrue(sim.AdvanceDay().Success);
        }
        [Test] public void SaveValidationRejectsOverlapsAndBrokenPaths()
        {
            sim.State.Rooms.Add(new Room { Id = 50, Kind = RoomKind.Toilet, X = 12, Y = 10, Width = 2, Height = 2 });
            sim.State.NextId = 51;
            Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
            sim.State.Rooms.Last().X = 20; sim.State.Rooms.Last().Y = 17;
            Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
        }
        [Test] public void SaveValidationRejectsInvalidEnumsIdsAndClock()
        {
            sim.State.ClockMinute = 1020; Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
            sim.State.ClockMinute = 480; sim.State.Timetable[0] = (Subject)999;
            Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
            sim.State.Timetable[0] = Subject.Arts; sim.State.NextId = 2;
            Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
        }
        [Test] public void ProlongedDebtEndsTheSchoolAndFurtherSettlementIsBlocked()
        {
            sim.State.Cash = -500;
            for (int day = 0; day < 5; day++)
            { if (sim.State.PendingEvent >= 0) sim.ChooseEvent(2); sim.AdvanceDay(); }
            Assert.IsTrue(sim.State.GameOver); Assert.AreEqual(5, sim.State.DebtDays);
            int cash = sim.State.Cash; Assert.AreEqual("blocked", sim.AdvanceDay().Code); Assert.AreEqual(cash, sim.State.Cash);
        }
        [Test] public void StableSchoolCanRunForSixtyDaysWithoutOverEnrollmentOrCorruptState()
        {
            sim.Hire(0); sim.Hire(3); sim.Build(RoomKind.Toilet, 10, 8); sim.Build(RoomKind.Staffroom, 12, 6);
            sim.Build(RoomKind.Classroom, 16, 10); sim.Hire(1); sim.Build(RoomKind.Canteen, 18, 6);
            for (int i = 0; i < 60; i++)
            {
                if (sim.State.PendingEvent >= 0) Assert.IsTrue(sim.ChooseEvent(2).Success);
                Assert.IsTrue(sim.AdvanceDay().Success);
                Assert.LessOrEqual(sim.State.Students, sim.Capacity);
                Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
                Assert.IsFalse(sim.State.GameOver);
            }
            Assert.AreEqual(48, sim.State.Students);
            Assert.Greater(sim.State.Cash, 10000);
        }
        [Test] public void BuyingLandCannotChargeTwice()
        {
            sim.Hire(0); sim.Build(RoomKind.Toilet, 10, 8); sim.Build(RoomKind.Staffroom, 12, 6);
            sim.State.Cash = 12000;
            Assert.IsTrue(sim.BuyLand().Success); Assert.AreEqual(3000, sim.State.Cash);
            Assert.AreEqual("owned", sim.BuyLand().Code); Assert.AreEqual(3000, sim.State.Cash);
            Assert.IsTrue(CampusGrid.IsOwned(sim.State, new Cell(0, 0)));
        }
        [Test] public void SaveValidationRejectsMissingDirectorAndNullLedgerEntries()
        {
            sim.State.HasDirector = true; sim.State.Director.Name = "";
            Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
            sim.State.Director.Name = "KoSch"; sim.State.History.Add(null);
            Assert.IsFalse(SchoolSimulation.ValidateSave(sim.State));
        }
        [Test] public void AllSevenGrantsCanBeReachedInASustainableCampaign()
        {
            sim.Hire(0); sim.Hire(3); sim.Build(RoomKind.Toilet, 16, 6); sim.Build(RoomKind.Staffroom, 12, 6);
            sim.Build(RoomKind.Classroom, 16, 10); sim.Hire(1); sim.Build(RoomKind.Canteen, 18, 6);
            sim.Build(RoomKind.Corridor, 11, 9); sim.Build(RoomKind.Corridor, 11, 10);
            Assert.IsTrue(sim.Build(RoomKind.Library, 7, 10).Success, "Library site must connect to the west corridor.");
            Assert.IsTrue(sim.Build(RoomKind.Garden, 8, 7).Success, "Garden site must stay clear of other rooms.");
            Assert.AreEqual(4, sim.State.CompletedGoals);
            for (int day = 0; day < 220 && sim.State.CompletedGoals < 7; day++)
            {
                if (sim.State.PendingEvent >= 0) sim.ChooseEvent(2);
                if (sim.Teachers < 4)
                    foreach (Employee e in sim.Candidates().Where(e => e.Role == StaffRole.Teacher))
                    { if (sim.Teachers < 4) sim.Hire(e.Id); }
                if (sim.State.Cash > 11000 && sim.State.CampusLevel == 0) sim.BuyLand();
                if (sim.State.Cash > 7000 && sim.Count(RoomKind.ScienceLab) == 0) BuildSomewhere(RoomKind.ScienceLab);
                if (sim.State.Cash > 6000 && sim.Count(RoomKind.ArtRoom) == 0) BuildSomewhere(RoomKind.ArtRoom);
                if (sim.State.Cash > 4500 && sim.Count(RoomKind.Classroom) < 4) BuildSomewhere(RoomKind.Classroom);
                if (sim.State.Cash > 1500 && sim.Count(RoomKind.Toilet) < 2) BuildSomewhere(RoomKind.Toilet);
                if (sim.State.Cash > 10000 && sim.State.Level >= 3 && sim.Count(RoomKind.Gym) == 0) BuildSomewhere(RoomKind.Gym);
                Assert.IsTrue(sim.AdvanceDay().Success);
                Assert.IsFalse(sim.State.GameOver);
                Assert.IsTrue(SchoolSimulation.ValidateSave(sim.State));
            }
            Assert.AreEqual(7, sim.State.CompletedGoals);
            Assert.GreaterOrEqual(sim.State.Students, 96);
        }
        private bool BuildSomewhere(RoomKind kind)
        {
            // Try every owned location. If no site fits, extend one reachable corridor towards open land.
            for (int turn = 0; turn < 35; turn++)
            {
                for (int x = 0; x < CampusGrid.Width; x++) for (int y = 0; y < CampusGrid.Height; y++)
                    if (sim.CanBuild(kind, x, y, false).Success) return sim.Build(kind, x, y).Success;
                bool extended = false;
                for (int y = 0; y < CampusGrid.Height && !extended; y++) for (int x = 0; x < CampusGrid.Width && !extended; x++)
                    if (sim.CanBuild(RoomKind.Corridor, x, y, false).Success) { sim.Build(RoomKind.Corridor, x, y); extended = true; }
                if (!extended) return false;
            }
            return false;
        }
    }
}

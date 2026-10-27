using Godot;
using KoSch.SchoolTycoon.Core;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>Runs inside the actual Godot scene. Uses isolated saves, never a player's slots.</summary>
public static class FrontendSmoke
{
    private static int checks;
    private static void Check(bool ok, string label)
    { if (!ok) throw new InvalidOperationException(label); checks++; GD.Print("PASS Godot " + label); }
    public static async void Run(RektorApp app, bool quit = true)
    {
        try
        {
            checks = 0;
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(RektorSaveHash.Hex("") == "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
                && RektorSaveHash.Hex("abc") == "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad", "portable SHA256 empty and ASCII vectors");
            Check(RektorSaveHash.Hex("Rektor · Schüler 🎓\n") == "a0b90a77dbffe3e36aafde9bdd10263a04bfde05ddec26ddc61595a668380792"
                && RektorSaveHash.Hex(new string('a', 1000000)) == "cdc76e5c9914fb9281a1c7e284d73e67f1809a48a497200e046d39ccc7112cd0", "portable SHA256 UTF8 and multi-block vectors");
            Check(app.Menu && app.UI.IsInsideTree() && app.Campus.Camera.Current, "scene boot and main menu");
            Check((app.Checking || !app.Campus.WorldBuilt) && app.GetViewport().Disable3D, "main menu has no allocated campus or active 3D renderer");
            app.UI.TestClick("campaign"); app.UI.TestClick("new1"); app.UI.TestClick("difficulty1");
            app.UI.TestClick("gender2"); app.UI.TestClick("hair0"); app.UI.TestClick("directorDone");
            app.UI.TestClick("storyTeam"); app.UI.TestClick("hire0");
            Check(app.State.HasDirector && app.State.Difficulty == Difficulty.Standard && app.Sim.Teachers == 1 && app.Sim.Capacity == 18, "menu difficulty designer story and staff button signals");
            app.ChooseBuild(RoomKind.Toilet); app.BuildAt(new Cell(18, 8));
            Check(app.Sim.Count(RoomKind.Toilet) == 1, "world construction through frontend");
            app.ChooseBuild(RoomKind.Staffroom); app.BuildAt(new Cell(15, 6));
            Check(app.Sim.Count(RoomKind.Staffroom) == 1, "connected room construction");
            app.ChooseBuild(null);
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            ulong initialRoom = app.Campus.RoomIdentity(1);
            int initialBuilds = app.Campus.RoomVisualBuilds;
            Check(initialRoom != 0 && app.Campus.WorldBuilt && !app.Campus.BuildPending, "deferred campus creation and coalesced startup");
            for (int i = 0; i < 40; i++)
            {
                if (app.State.PendingEvent >= 0) { app.UI.Pending(); app.UI.TestClick("event2"); }
                app.UI.TestClick("endDay");
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            if (app.State.PendingEvent >= 0) { app.UI.Pending(); app.UI.TestClick("event2"); }
            Check(app.State.Day == 41 && !app.State.GameOver && SchoolSimulation.ValidateSave(app.State), "forty school days events and state invariants");
            Check(app.Campus.RoomIdentity(1) == initialRoom && app.Campus.RoomVisualBuilds == initialBuilds, "daily simulation reuses unchanged room geometry");
            var serialized = RektorApp.Encode(app.State); var decoded = RektorApp.Decode(serialized);
            Check(SchoolSimulation.ValidateSave(decoded) && decoded.Pupils.Count == app.State.Pupils.Count && decoded.Classes[0].Week.SequenceEqual(app.State.Classes[0].Week), "Godot source-generated save serialization");
            Check(app.Save(false, true), "native save write");
            Check(app.Saves.Load(1, out _)?.Day == 41, "native save read");
            for (int tab = 0; tab < 8; tab++) { app.UI.ShowManagement(tab); await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame); }
            Check(true, "all eight management views instantiated");
            app.UI.CloseModal(); app.UI.Hud(); app.UI.TestClick("view"); app.UI.TestClick("roadmap"); app.UI.CloseModal();
            app.UI.TestClick("view"); app.UI.TestClick("towerGuide"); app.UI.CloseModal();
            Check(true, "Mara chapter roadmap and tower construction guide accessible");
            for (int slot = 2; slot <= 6; slot++)
            {
                app.NewCampaign(slot, (Difficulty)((slot - 2) % 3), slot == 6 ? GameMode.Sandbox : GameMode.Campaign, "Smoke " + slot);
                app.UI.TestClick("gender0"); app.UI.TestClick("hair2"); app.UI.TestClick("directorDone"); app.UI.CloseModal();
                Check(app.Save(false, true) && app.Saves.Load(slot, out _)?.SchoolName == "Smoke " + slot, "isolated save slot " + slot);
            }
            app.UI.ShowManagement(1); app.UI.TestClick("hire0");
            app.ChooseBuild(RoomKind.Classroom); app.BuildAt(new Cell(16, 10));
            Check(app.Sim.Sandbox && app.State.Cash == 0 && app.Sim.Count(RoomKind.Classroom) == 2, "sandbox infinite funds construction");
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            ulong baseRoom = app.Campus.RoomIdentity(1), upgradedRoom = app.Campus.RoomIdentity(2);
            app.Act(app.Sim.BuyRoomItem(2, RoomItem.DeskRow1));
            Check(app.Sim.RoomSeats(app.State.Rooms.First(r => r.Id == 2)) == 22, "functional room upgrade and seats");
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(app.Campus.RoomIdentity(1) == baseRoom && app.Campus.RoomIdentity(2) != upgradedRoom, "room purchase refreshes only the changed room");
            Check(app.Saves.Load(1, out _)?.Day == 41, "other saves preserved");
            app.ReturnToMenu(); Check(app.Load(1) && app.State.Day == 41 && app.State.Difficulty == Difficulty.Standard, "continue preserves campaign and difficulty");
            app.UI.CloseModal(); app.SetFloor(2); app.Campus.Orbit(); app.Campus.Zoom(-3); app.SetFloor(0);
            Check(app.Campus.Pick(app.Campus.Camera.UnprojectPosition(new Vector3(16.5f, .15f, 8.5f)))?.Equals(new Cell(16, 8)) == true, "camera projection and build picking");
            for (int i = 0; i < 5; i++)
            {
                app.ReturnToMenu();
                Check(app.GetViewport().Disable3D, "menu suspends 3D cycle " + i);
                Check(app.Load(1) && app.State.Day == 41, "save continue cycle " + i);
                app.UI.CloseModal();
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            app.NewCampaign(6, Difficulty.Standard, GameMode.Sandbox, "Tower regression");
            app.CompleteDirector(new() { Name = "Tower", Gender = "Diverse", Hair = "Brown" }); app.UI.CloseModal();
            Check(app.Sim.Hire(0).Success && app.Sim.Build(RoomKind.Staircase, 16, 10).Success, "tower foundation through shared rules");
            for (int floor = 1; floor < 6; floor++)
            {
                if (!app.Sim.Build(RoomKind.Staircase, 16, 10, false, floor).Success) throw new InvalidOperationException("Tower staircase " + floor);
                for (int x = 16; x >= 12; x--) if (!app.Sim.Build(RoomKind.Corridor, x, 9, false, floor).Success) throw new InvalidOperationException("Tower corridor");
                if (!app.Sim.Build(RoomKind.Classroom, 12, 10, false, floor).Success) throw new InvalidOperationException("Tower classroom");
            }
            app.Campus.Rebuild(); app.SetFloor(5);
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(app.Campus.VisualRoomCount == app.State.Rooms.Count && app.State.Classes.Count == 6 && SchoolSimulation.ValidateSave(app.State), "all six floors rendered without lost rooms or invalid saves");
            Check(app.Campus.VisualPeopleCount <= 57 && app.Save(false, true), "bounded displayed population and six-floor save");
            app.ReturnToMenu(); app.UI.TestClick("diagnostics"); app.UI.CloseModal();
            Check(app.Diagnostics.Report().Contains("campus-ready"), "persistent local startup diagnostics");
            GD.Print($"GODOT_SMOKE_PASS {checks} checks"); if (quit) app.GetTree().Quit(0); else app.EndCheck();
        }
        catch (Exception e) { GD.PushError("GODOT_SMOKE_FAIL " + e); if (quit) app.GetTree().Quit(1); else { app.EndCheck(); app.Diagnostics.Failure(e); app.UI.Toast("Funktionstest fehlgeschlagen. Diagnose kopieren."); } }
    }
}

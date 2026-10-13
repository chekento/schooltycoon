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
    public static async void Run(RektorApp app)
    {
        try
        {
            await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            Check(app.Menu && app.UI.IsInsideTree() && app.Campus.Camera.Current, "scene boot and main menu");
            app.UI.TestClick("campaign"); app.UI.TestClick("new1"); app.UI.TestClick("difficulty1");
            app.UI.TestClick("gender2"); app.UI.TestClick("hair0"); app.UI.TestClick("directorDone");
            app.UI.TestClick("storyTeam"); app.UI.TestClick("hire0");
            Check(app.State.HasDirector && app.State.Difficulty == Difficulty.Standard && app.Sim.Teachers == 1 && app.Sim.Capacity == 18, "menu difficulty designer story and staff button signals");
            app.ChooseBuild(RoomKind.Toilet); app.BuildAt(new Cell(18, 8));
            Check(app.Sim.Count(RoomKind.Toilet) == 1, "world construction through frontend");
            app.ChooseBuild(RoomKind.Staffroom); app.BuildAt(new Cell(15, 6));
            Check(app.Sim.Count(RoomKind.Staffroom) == 1, "connected room construction");
            app.ChooseBuild(null);
            for (int i = 0; i < 40; i++)
            {
                if (app.State.PendingEvent >= 0) { app.UI.Pending(); app.UI.TestClick("event2"); }
                app.UI.TestClick("endDay");
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
            }
            if (app.State.PendingEvent >= 0) { app.UI.Pending(); app.UI.TestClick("event2"); }
            Check(app.State.Day == 41 && !app.State.GameOver && SchoolSimulation.ValidateSave(app.State), "forty school days events and state invariants");
            var serialized = RektorApp.Encode(app.State); var decoded = RektorApp.Decode(serialized);
            Check(SchoolSimulation.ValidateSave(decoded) && decoded.Pupils.Count == app.State.Pupils.Count && decoded.Classes[0].Week.SequenceEqual(app.State.Classes[0].Week), "Godot source-generated save serialization");
            Check(app.Save(false, true), "native save write");
            Check(app.Saves.Load(1, out _)?.Day == 41, "native save read");
            for (int tab = 0; tab < 8; tab++) { app.UI.ShowManagement(tab); await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame); }
            Check(true, "all eight management views instantiated");
            for (int slot = 2; slot <= 6; slot++)
            {
                app.NewCampaign(slot, (Difficulty)((slot - 2) % 3), slot == 6 ? GameMode.Sandbox : GameMode.Campaign, "Smoke " + slot);
                app.UI.TestClick("gender0"); app.UI.TestClick("hair2"); app.UI.TestClick("directorDone"); app.UI.CloseModal();
                Check(app.Save(false, true) && app.Saves.Load(slot, out _)?.SchoolName == "Smoke " + slot, "isolated save slot " + slot);
            }
            app.UI.ShowManagement(1); app.UI.TestClick("hire0");
            app.ChooseBuild(RoomKind.Classroom); app.BuildAt(new Cell(16, 10));
            Check(app.Sim.Sandbox && app.State.Cash == 0 && app.Sim.Count(RoomKind.Classroom) == 2, "sandbox infinite funds construction");
            app.Act(app.Sim.BuyRoomItem(2, RoomItem.DeskRow1));
            Check(app.Sim.RoomSeats(app.State.Rooms.First(r => r.Id == 2)) == 22, "functional room upgrade and seats");
            Check(app.Saves.Load(1, out _)?.Day == 41, "other saves preserved");
            app.ReturnToMenu(); Check(app.Load(1) && app.State.Day == 41 && app.State.Difficulty == Difficulty.Standard, "continue preserves campaign and difficulty");
            app.UI.CloseModal(); app.SetFloor(2); app.Campus.Orbit(); app.Campus.Zoom(-3); app.SetFloor(0);
            Check(app.Campus.Pick(app.Campus.Camera.UnprojectPosition(new Vector3(16.5f, .15f, 8.5f)))?.Equals(new Cell(16, 8)) == true, "camera projection and build picking");
            GD.Print($"GODOT_SMOKE_PASS {checks} checks"); app.GetTree().Quit(0);
        }
        catch (Exception e) { GD.PushError("GODOT_SMOKE_FAIL " + e); app.GetTree().Quit(1); }
    }
}

using Godot;
using KoSch.SchoolTycoon.Core;
using System;
using System.IO;
using System.Linq;

/// <summary>Developer-only real rendered captures with isolated temporary saves.</summary>
public static class CapturePreview
{
    public static async void Run(RektorApp app)
    {
        try
        {
            string folder = ProjectSettings.GlobalizePath("user://captures"); Directory.CreateDirectory(folder);
            async System.Threading.Tasks.Task Capture(string name)
            {
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
                await app.ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                var image = app.GetViewport().GetTexture().GetImage(); var path = Path.Combine(folder, name + ".png");
                if (image.SavePng(path) != Error.Ok) throw new IOException("Capture could not be saved");
                GD.Print("PREVIEW_SAVED " + path);
            }
            await Capture("godot-menu");
            app.NewCampaign(1, Difficulty.Standard, GameMode.Sandbox, "Der Linden-Campus");
            app.CompleteDirector(new() { Name = "KoSch", Gender = "Diverse", Hair = "Brown" }); app.UI.CloseModal();
            app.Act(app.Sim.Hire(0)); app.Act(app.Sim.Hire(1)); app.Act(app.Sim.Hire(3));
            foreach (var build in new[] { (RoomKind.Classroom, 16, 10), (RoomKind.Library, 8, 8), (RoomKind.ScienceLab, 12, 5), (RoomKind.Staffroom, 16, 6), (RoomKind.Staircase, 18, 8), (RoomKind.Toilet, 20, 8) })
            { var r = app.Sim.Build(build.Item1, build.Item2, build.Item3); if (!r.Success) throw new Exception("Preview construction: " + r.Code); }
            foreach (var room in app.State.Rooms.Where(r => !CampusGrid.Walkway(r)))
                foreach (var item in RektorCatalog.For(room.Kind)) { var r = app.Sim.BuyRoomItem(room.Id, item.Id); if (!r.Success) throw new Exception("Preview item: " + r.Code); }
            app.Campus.Rebuild(); app.Campus.Zoom(-6); app.UI.Hud(); await Capture("godot-campus");
            app.Selected = 2; app.UI.ShowManagement(0); await Capture("godot-upgrades");
            app.GetTree().Quit(0);
        }
        catch (Exception e) { GD.PushError("PREVIEW_FAIL " + e); app.GetTree().Quit(1); }
    }
}

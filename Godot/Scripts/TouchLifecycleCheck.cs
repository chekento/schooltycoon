using Godot;
using System;
using System.Threading.Tasks;

/// <summary>ADB taps the real, rendered buttons. Developer flag uses isolated saves and leaves the app running.</summary>
public static class TouchLifecycleCheck
{
    public static async void Run(RektorApp app)
    {
        try
        {
            async Task Wait(Func<bool> condition, string name)
            {
                for (int i = 0; i < 1800; i++)
                {
                    if (condition()) return;
                    await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
                }
                throw new InvalidOperationException("Touch transition timed out: " + name);
            }
            async Task Tap(string id, Func<bool> changed)
            {
                await Wait(() => app.UI.TestButton(id) != null, id);
                var button = app.UI.TestButton(id);
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
                app.UI.TestReveal(button);
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
                await app.ToSignal(app.GetTree(), SceneTree.SignalName.ProcessFrame);
                var point = app.GetViewport().GetScreenTransform() * button.GetGlobalRect().GetCenter();
                GD.Print($"REKTOR_TOUCH {id} {Mathf.RoundToInt(point.X)} {Mathf.RoundToInt(point.Y)}");
                app.UI.Toast(app.T("Prüfschritt: ", "Check step: ") + button.Text);
                await Wait(changed, id);
            }
            await Tap("campaign", () => app.UI.TestButton("new1") != null);
            await Tap("new1", () => app.UI.TestButton("difficulty1") != null);
            await Tap("difficulty1", () => app.UI.TestButton("gender2") != null);
            await Tap("gender2", () => app.UI.TestButton("hair0") != null);
            await Tap("hair0", () => app.UI.TestButton("directorDone") != null);
            await Tap("directorDone", () => app.State.HasDirector && app.UI.TestButton("storyTeam") != null);
            await Tap("storyTeam", () => app.UI.TestButton("hire0") != null);
            await Tap("hire0", () => app.Sim.Teachers == 1 && !app.Campus.BuildPending);
            await Tap("menu", () => app.Menu);
            await Tap("campaign", () => app.UI.TestButton("continue1") != null);
            await Tap("continue1", () => !app.Menu && !app.Campus.BuildPending);
            if (app.State.Day != 1 || app.Sim.Teachers != 1 || !app.State.HasDirector || app.Saves.Load(1, out _) == null)
                throw new InvalidOperationException("Touch save/continue invariants");
            GD.Print("REKTOR_TOUCH_PASS 11 real taps; save and continue preserved");
        }
        catch (Exception error) { app.Diagnostics.Failure(error); GD.PushError("REKTOR_TOUCH_FAIL " + error); }
    }
}

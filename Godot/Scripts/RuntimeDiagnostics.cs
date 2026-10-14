using Godot;
using System;
using System.IO;

/// <summary>Small local startup trace. It never records pupil names or save contents.</summary>
public sealed class RuntimeDiagnostics
{
    private readonly string trace, marker;
    public bool Interrupted { get; }
    public string PreviousStage { get; } = "";
    public RuntimeDiagnostics(string root)
    {
        trace = Path.Combine(root, "runtime.log"); marker = Path.Combine(root, "session.running");
        try
        {
            Directory.CreateDirectory(root);
            Interrupted = File.Exists(marker);
            if (Interrupted) PreviousStage = File.ReadAllText(marker);
            if (File.Exists(trace) && new FileInfo(trace).Length > 48 * 1024) File.Copy(trace, trace + ".previous", true);
            File.WriteAllText(trace, "REKTOR 0.7.0 · Godot " + Engine.GetVersionInfo()["string"] + " · " + OS.GetName() + "\n");
        }
        catch (Exception e) { GD.PushWarning("Diagnostic storage unavailable: " + e.Message); }
        Stage("boot");
    }
    public void Stage(string stage)
    {
        GD.Print("REKTOR_STAGE " + stage);
        try
        {
            File.WriteAllText(marker, stage);
            File.AppendAllText(trace, DateTime.UtcNow.ToString("O") + " " + stage + " · managed=" + GC.GetTotalMemory(false) / 1024 + " KiB\n");
        }
        catch (Exception e) { GD.PushWarning("Diagnostic write unavailable: " + e.Message); }
    }
    public void Failure(Exception error)
    {
        GD.PushError("REKTOR_ACTION_FAIL " + error);
        try { File.AppendAllText(trace, error + "\n"); } catch { }
    }
    public void Clean()
    {
        try { if (File.Exists(marker)) File.Delete(marker); } catch { }
    }
    public string Report()
    {
        try { return (Interrupted ? "Previous interrupted session: " + PreviousStage + "\n" : "") + File.ReadAllText(trace); }
        catch { return "Rektor 0.7.0 · diagnostic storage unavailable"; }
    }
}

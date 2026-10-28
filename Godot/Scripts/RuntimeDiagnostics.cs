using Godot;
using System;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>Small local startup trace. It never records pupil names or save contents.</summary>
public sealed class RuntimeDiagnostics
{
    private readonly string trace, marker, nativeTrace;
    [DllImport("libc.so", EntryPoint = "dup2", SetLastError = true)]
    private static extern int DuplicateDescriptor(int source, int target);
    public bool Interrupted { get; }
    public string PreviousStage { get; } = "";
    public RuntimeDiagnostics(string root)
    {
        trace = Path.Combine(root, "runtime.log"); marker = Path.Combine(root, "session.running");
        nativeTrace = Path.Combine(root, "native-runtime.log");
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
    public void CaptureNativeErrors()
    {
        if (!OS.HasFeature("android")) return;
        try
        {
            if (File.Exists(nativeTrace) && new FileInfo(nativeTrace).Length > 0)
            {
                File.Copy(nativeTrace, nativeTrace + ".previous", true);
                foreach (string line in File.ReadAllText(nativeTrace).Split('\n'))
                    GD.Print("REKTOR_PREVIOUS_NATIVE " + line);
            }
            using var file = new FileStream(nativeTrace, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            int descriptor = file.SafeFileHandle.DangerousGetHandle().ToInt32();
            if (DuplicateDescriptor(descriptor, 1) < 0 || DuplicateDescriptor(descriptor, 2) < 0)
                throw new IOException("Native runtime diagnostic redirection failed");
        }
        catch (Exception error) { GD.PushWarning("Native diagnostics unavailable: " + error.Message); }
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
        try { return (Interrupted ? "Previous interrupted session: " + PreviousStage + "\n" : "") + File.ReadAllText(trace)
            + (File.Exists(nativeTrace + ".previous") ? "\nPrevious native runtime:\n" + File.ReadAllText(nativeTrace + ".previous") : ""); }
        catch { return "Rektor 0.7.0 · diagnostic storage unavailable"; }
    }
}

using Godot;
using System;

public partial class CampusAudio : Node
{
    private RektorApp app;
    private AudioStreamPlayer music, effects;
    public void Initialize(RektorApp owner)
    {
        app = owner;
        music = new AudioStreamPlayer { Name = "Ambient", Stream = Tone(12, true) };
        effects = new AudioStreamPlayer { Name = "Click", Stream = Tone(.07, false) };
        AddChild(music); AddChild(effects); music.Play(); Apply();
    }
    private static AudioStreamWav Tone(double seconds, bool loop)
    {
        const int rate = 22050;
        byte[] data = new byte[(int)(seconds * rate) * 2];
        for (int i = 0; i < data.Length / 2; i++)
        {
            double t = (double)i / rate;
            double wave = loop ? .045 * (Math.Sin(t * Math.Tau * 220) + .4 * Math.Sin(t * Math.Tau * 330) + .25 * Math.Sin(t * Math.Tau * 440)) * Math.Pow(Math.Sin(Math.PI * t / seconds), 2) : .13 * Math.Sin(t * Math.Tau * 660) * Math.Exp(-t * 90);
            short value = (short)(wave * short.MaxValue); data[i * 2] = (byte)(value & 255); data[i * 2 + 1] = (byte)(value >> 8);
        }
        return new AudioStreamWav { Format = AudioStreamWav.FormatEnum.Format16Bits, MixRate = rate, Data = data, LoopMode = loop ? AudioStreamWav.LoopModeEnum.Forward : AudioStreamWav.LoopModeEnum.Disabled, LoopBegin = 0, LoopEnd = data.Length / 2 };
    }
    public void Apply()
    {
        music.VolumeDb = Mathf.LinearToDb(Math.Max(.0001f, app.Settings.Music));
        effects.VolumeDb = Mathf.LinearToDb(Math.Max(.0001f, app.Settings.Effects));
    }
    public void Click() { if (app.Settings.Effects > 0) effects.Play(); }
}

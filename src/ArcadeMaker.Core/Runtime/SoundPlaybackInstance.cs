using ArcadeMaker.Core.Resources;
using Exp;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Runtime;

public abstract class SoundPlaybackInstance<T> : Exp.Instance
{
    public Sound Sound { get; }
    public T Instance { get; }
    public abstract float Volume { get; set; }
    public abstract float Pan { get; set; } 
    public abstract float Pitch { get; set; }

    public SoundPlaybackInstance(Sound sound, T instance) : base(SoundPlaybackInstance.Class, addProperties: false)
    {
        this.Sound = sound;
        this.Instance = instance;

        // create the volume, pan and pitch properties
        Vars.Add(new Exp.CustomVariable(SoundPlaybackInstance.VolumePName, () => ((double)Volume).ToExp(), (value) => Volume = (float)(value?.Number ?? 1))); // volume
        Vars.Add(new Exp.CustomVariable(SoundPlaybackInstance.PanPName, () => ((double)Pan).ToExp(), (value) => Pan = (float)(value?.Number ?? 0d.ToExp()))); // pan
        Vars.Add(new Exp.CustomVariable(SoundPlaybackInstance.PitchPName, () => ((double)Pitch).ToExp(), (value) => Pitch = (float)(value?.Number ?? 0d.ToExp()))); // pitch
    }
}

public static class SoundPlaybackInstance
{
    public const string VolumePName = "volume";
    public const string PanPName    = "pan";
    public const string PitchPName  = "pitch";
    public static ClassDefSpan Class { get; } = new("SoundPlayback", [
        new(null, false, VolumePName, false),
        new(null, false, PanPName,    false),
        new(null, false, PitchPName,  false)
    ], []);
}

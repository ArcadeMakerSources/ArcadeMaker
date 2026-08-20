using ArcadeMaker.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Text;

namespace ArcadeMaker.Core.Resources;

public class Sound(string name, string filePath, float startVolume, float startPan, float startPitch, Sound.Types type) : ISetsID
{
    public int ID { get; } = Core.ID.Generate();

    public enum Formats
    {
        Wav,
        Mp3,
        Ogg,

        // NOTE: This attribute currently filters out WMA on windows machine[SupportedOSPlatform("Windows")]
        // There is no need to filter windows only format since there is no plans to port engine cross-platform
        Wma
    }

    public enum Types
    {
        SoundEffect,
        BackgroundMusic
    }

    public string Name => name;
    public string FilePath => filePath;
    public float StartVolume => startVolume;
    public float StartPan => startPan;
    public float StartPitch => startPitch;
    public Types Type => type;
}

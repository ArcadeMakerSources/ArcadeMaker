using ArcadeMaker.Core.Resources;
using Microsoft.Xna.Framework.Media;
using System;
using System.Reflection;

namespace ArcadeMaker.Engines.MonoGame.Core.Runtime;

public class SongPlaybackInstance : ArcadeMaker.Core.Runtime.SoundPlaybackInstance<Song>
{
    public static Func<string, object>? Android_Net_Uri_Parse { get; set; }

    public override float Volume
    {
        get => ArcadeMakerMonoGame.CurrentlyPlayedBackgroundMusic == Instance ? MediaPlayer.Volume : Sound.StartVolume;
        set
        {
            if (ArcadeMakerMonoGame.CurrentlyPlayedBackgroundMusic == Instance)
                MediaPlayer.Volume = value;
            else
                throw new Exception("Volume of a specific background music playback can only be set while it's playing.");
        }
    }

    public override float Pan
    {
        get => 0;
        set => throw new NotImplementedException("Cannot set pan for background music.");
    }

    public override float Pitch
    {
        get => 0;
        set => throw new NotImplementedException("Cannot set pitch for background music.");
    }

    internal SongPlaybackInstance(Sound sound, Song instance) : base(sound, instance)
    {

    }
}
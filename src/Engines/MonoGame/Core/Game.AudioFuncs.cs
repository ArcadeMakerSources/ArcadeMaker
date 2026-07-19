using ArcadeMaker.Engines.MonoGame.Core.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using ArcadeMaker.Core;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Runtime;
using Microsoft.Xna.Framework.Content;
using Exp;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Engines.MonoGame.Core.Graphics;
using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.Resources.Serializeables;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System.Linq;
using Exp.Spans;
using System.IO;

namespace ArcadeMaker.Engines.MonoGame.Core;

public partial class ArcadeMakerMonoGame
{
    private readonly Dictionary<Sound, SoundEffect> soundEffects = [];
    private readonly Dictionary<Sound, Song> backgroundMusics = [];
    private readonly Dictionary<Sound, List<SoundEffectInstance>> soundEffectInstances = [];
    private readonly List<FileStream> openedSongFilesStreams = [];
    internal static Song? CurrentlyPlayedBackgroundMusic { get; private set; }

    public Exp.Instance? PlaySound(Exp.Instance? _, IValue?[] args)
    {
        // resolve arguments
        Sound sound = GetSoundFromArgs(args[0]);
        bool loop = args.Length >= 2 && args[1].ThrowIfNull().Bool;
        float volume = args.Length >= 3 ? (float)args[2].ThrowIfNull().Number : sound.StartVolume;
        float pan = args.Length >= 4 ? (float)args[3].ThrowIfNull().Number : sound.StartPan;
        float pitch = args.Length >= 5 ? (float)args[4].ThrowIfNull().Number : sound.StartPitch;

        // play it
        if (sound.Type == Sound.Types.BackgroundMusic)
        {
            if (MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Stop();

            if (backgroundMusics.TryGetValue(sound, out Song? song))
            {
                MediaPlayer.Volume = volume;
                MediaPlayer.IsRepeating = loop;
                MediaPlayer.Play(song);
                CurrentlyPlayedBackgroundMusic = song;
                return new Runtime.SongPlaybackInstance(sound, song);
            }
            else
                throw new Exception($"No MonoGame {typeof(Song).Name} was found for this background music.");
        }
        else if (sound.Type == Sound.Types.SoundEffect)
        {
            if (soundEffects.TryGetValue(sound, out SoundEffect? effect))
            {
                var sInst = effect.CreateInstance();
                sInst.Volume = volume;
                sInst.Pan = pan;
                sInst.Pitch = pitch;
                sInst.IsLooped = loop;
                soundEffectInstances.Add(sound, sInst);
                sInst.Play();
                return new Runtime.SoundEffectPlaybackInstance(sound, sInst);
            }
            else
                throw new Exception($"No MonoGame {typeof(SoundEffect).Name} was found for this sound effect.");
        }

        return null;
    }

    private Sound GetSoundFromArgs(IValue? arg)
    {
        arg.ThrowIfNull();
        return Sounds.FirstOrDefault(s => s.ID == arg!.Number) ?? throw new Exception($"No sound with ID {arg!.Number} was found. Use {ExpSrc.EngineNamespace}{NamespaceSpecificationSpan.Symbol}Sounds enum to get the ID of a sound.");
    }

    public Exp.Void PauseSound(Exp.Instance? inst, IValue?[] args)
    {
        // soundPlaybackInstance.pause():
        if (inst is SoundPlaybackInstance<SoundEffectInstance> seInst)
        {
            args.ValidateArgsNumber(0);
            seInst.Instance.Pause();
        }
        else if (inst is SoundPlaybackInstance<Song> sInst)
        {
            if (CurrentlyPlayedBackgroundMusic == sInst.Instance && MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Pause();
        }

        // pauseSound(soundID):
        else
        {
            args.ValidateArgsNumber(1);
            Sound sound = GetSoundFromArgs(args[0]);
            if (soundEffectInstances.TryGetValue(sound, out var ls))
                ls.ForEach(inst => inst.Pause());
        }

        return Exp.Void.Return;
    }

    public Exp.Void PauseAllSounds(Exp.Instance? _, IValue?[] args)
    {
        if (MediaPlayer.State == MediaState.Playing)
            MediaPlayer.Pause();

        soundEffectInstances.ForEach(s => s.Value.ForEach(i => i.Pause()));

        return Exp.Void.Return;
    }

    public Exp.Void ResumeSound(Exp.Instance? inst, IValue?[] args)
    {
        // soundPlaybackInstance.resume():
        if (inst is SoundPlaybackInstance<SoundEffectInstance> seInst)
        {
            args.ValidateArgsNumber(0);
            seInst.Instance.Resume();
        }
        else if (inst is SoundPlaybackInstance<Song> sInst)
        {
            if (CurrentlyPlayedBackgroundMusic == sInst.Instance && MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Resume();
        }

        // resumeSound(soundID):
        else
        {
            args.ValidateArgsNumber(1);
            Sound sound = GetSoundFromArgs(args[0]);
            if (soundEffectInstances.TryGetValue(sound, out var ls))
                ls.ForEach(inst => inst.Resume());
        }

        return Exp.Void.Return;
    }

    public Exp.Void ResumeAllSounds(Exp.Instance? _, IValue?[] args)
    {
        if (MediaPlayer.State == MediaState.Paused)
            MediaPlayer.Resume();

        soundEffectInstances.ForEach(s => s.Value.ForEach(i => i.Resume()));

        return Exp.Void.Return;
    }
}
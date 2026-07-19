using ArcadeMaker.Core.Math.Shapes;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Core.Runtime;
using Exp;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.IDE.Debugging;

internal sealed class FutileGame : ArcadeMaker.Core.IGame
{
    public List<Sprite> Sprites { get; } = [];
    public List<ObjectModel> Objects { get; } = [];
    public List<Sound> Sounds { get; } = [];
    public List<Background> Backgrounds { get; } = [];
    public List<Core.Resources.Path> Paths { get; } = [];
    public List<ArcadeMaker.Core.Resources.Serializeables.GameFont> FontsData { get; } = [];
    public List<RoomModel> Rooms { get; } = [];
    public List<ScriptDocument> Scripts { get; } = [];
    public RoomInstance? CurrentRoom { get; set; }
    public TextureAtlasMap MainTextureAtlasMap { get; set; }
    public string MainTextureAtlasFilePath { get; set; }
    public int CurrentViewIndex { get; }

    public void Init() { }
    public Exp.Void DrawInstance(ArcadeMaker.Core.Runtime.Instance instance) => Exp.Void.Return;
    public void DrawBackgrounds() { }
    public void DrawLine(double x1, double y1, double x2, double y2, double thickness) { }
    public void SetWindowsSize(int w, int h) { }
    public void SetCaption(string caption) { }
    public Color BackColor { get; set; }

    public Exp.Void ShowMessage(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;

    public BoolValue KeyDown(Exp.Instance? _, IValue?[] args) => false;

    public BoolValue KeyPress(Exp.Instance? _, IValue?[] args) => false;

    public BoolValue KeyRelease(Exp.Instance? _, IValue?[] args) => false;

    public BoolValue MouseButtonDown(Exp.Instance? _, IValue?[] args) => false;

    public BoolValue MouseButtonPress(Exp.Instance? _, IValue?[] args) => false;

    public BoolValue MouseButtonRelease(Exp.Instance? _, IValue?[] args) => false;

    public (int, int) MousePositionInWindow { get; }

    public BoolValue GamepadButtonDown(Exp.Instance? _, IValue?[] args) => false;

    public Exp.Void DrawSprite(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;
    public Exp.Void DrawText(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;
    public Exp.Void SetFont(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;
    public Exp.Void SetColor(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;

    public Exp.Instance? PlaySound(Exp.Instance? _, IValue?[] args) => null;

    public Exp.Void PauseSound(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;

    public Exp.Void PauseAllSounds(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;

    public Exp.Void ResumeSound(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;

    public Exp.Void ResumeAllSounds(Exp.Instance? _, IValue?[] args) => Exp.Void.Return;
}
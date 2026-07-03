using ArcadeMaker.Core.Math.Shapes;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Resources.Serializeables;
using Exp;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArcadeMaker.Core.Runtime;

public class Instance : Exp.Instance
{
    public const int NUMBER_OF_ALARMS = 11;

    public ObjectModel Model { get; }
    public int FramesSinceLastImageIndex { get; set; }

    [ExpProperty]
    public TypeVariable X { get; }

    [ExpProperty]
    public TypeVariable Y { get; }

    internal NumberValue? speed = 0, hspeed = 0, vspeed = 0, direction = 0;

    [ExpProperty]
    public CustomVariable Speed { get; }

    [ExpProperty]
    public CustomVariable Hspeed { get; }

    [ExpProperty]
    public CustomVariable Vspeed { get; }

    [ExpProperty]
    public CustomVariable Direction { get; }

    [ExpProperty]
    public TypeVariable ImageIndex { get; }

    [ExpProperty]
    public TypeVariable ImageSpeed { get; }

    [ExpProperty]
    public TypeVariable ImageAngle { get; }

    [ExpProperty]
    public TypeVariable ImageXScale { get; }

    [ExpProperty]
    public TypeVariable ImageYScale { get; }

    public Rect? Mask { get; private set; }

    private double depth;
    public event EventHandler<double>? DepthChanged;
    [ExpProperty]
    public CustomVariable Depth { get; }

    [ExpProperty]
    public TypeVariable Solid { get; }

    public (int number, ObjectEvent? ev)[] Alarm { get; } = new (int, ObjectEvent)[NUMBER_OF_ALARMS];

    internal PathDrive? CurrentPathDrive { get; set; }

    public static readonly Dictionary<PropertyInfo, ExpPropertyAttribute> csProperties;
    static Instance()
    {
        // init csProperties
        csProperties = [];
        foreach (var pro in typeof(Instance).GetProperties())
        {
            var attr = pro.GetCustomAttribute<ExpPropertyAttribute>();
            if (attr != null)
                csProperties.Add(pro, attr);
        }
    }

    public Instance(ObjectModel model) : base(model.Class, addProperties: false)
    {
        this.Model = model;

        InitMask();

        // assign properties
        var isNumChecker = new Func<IValue?, bool>(v => v?.IsNumber == true);
        var isBoolChecker = new Func<IValue?, bool>(v => v?.IsBool == true);

        NumberValue zero = 0d.ToExp(), one = 1d.ToExp();
        BoolValue expFalse = false.ToExp();

        X = InitVar("x", zero, isNumChecker, ValueHelper.tnum);
        Y = InitVar("y", zero, isNumChecker, ValueHelper.tnum);
        Speed = new CustomVariable("speed", () => speed, (value) => { speed = value?.Number; SetHVSpeeds(); });
        Hspeed = new CustomVariable("hspeed", () => hspeed, (value) => { hspeed = value?.Number; SetSpeedAndDir(); });
        Vspeed = new CustomVariable("vspeed", () => vspeed, (value) => { vspeed = value?.Number; SetSpeedAndDir(); });
        Direction = new CustomVariable("direction", () => direction, (value) => { direction = value?.Number; SetHVSpeeds(); });
        Vars.AddRange([Speed, Hspeed, Vspeed, Direction]);
        ImageIndex = InitVar("imageIndex", zero, isNumChecker, ValueHelper.tnum);
        ImageSpeed = InitVar("imageSpeed", one, isNumChecker, ValueHelper.tnum);
        ImageAngle = InitVar("imageAngle", zero, isNumChecker, ValueHelper.tnum);
        ImageXScale = InitVar("imageXScale", one, isNumChecker, ValueHelper.tnum);
        ImageYScale = InitVar("imageYScale", one, isNumChecker, ValueHelper.tnum);
        depth = ((double)model.InitValues.Depth);
        Depth = new CustomVariable("depth", () => depth.ToExp(), SetDepth);
        Solid = InitVar("solid", model.InitValues.Solid.ToExp(), isBoolChecker, ValueHelper.tbool);

        // init alarms
        for (int alarmIndex = 0; alarmIndex < NUMBER_OF_ALARMS; alarmIndex++)
        {
            Alarm[alarmIndex].number = -1;
            Alarm[alarmIndex].ev = Model.AlarmEvents.FirstOrDefault(ev => ev.Param == alarmIndex);
        }

        AssignExtraProperties();
    }

    private void AssignExtraProperties()
    {
        foreach (var property in Model.ExtraProperties)
        {
            bool typeChecker(IValue? v)
            {
                if (v == null)
                    return property.Nullable;

                return property.Type switch
                {
                    VariableType.Bool => v.IsBool,
                    VariableType.Char => v.IsChar,
                    VariableType.Number => v.IsNumber,
                    VariableType.Array => v.IsInst && v.Inst.IsArray,
                    VariableType.String => v.IsString(),
                    _ => true // default
                };
            }
            string? typeName = null;
            switch (property.Type)
            {
                case VariableType.Bool:
                    typeName = ValueHelper.tbool; break;
                case VariableType.Char:
                    typeName = ValueHelper.tchar; break;
                case VariableType.Number:
                    typeName = ValueHelper.tnum; break;
                case VariableType.Array:
                    typeName = "array"; break;
                case VariableType.String:
                    typeName = "string"; break;
            }

            Vars.Add(typeName == null ? new Variable(property.Name, null, null, property.Private, property.Constant) : new TypeVariable(property.Name, null, typeChecker, typeName, property.Private, property.Constant, true));
        }
    }

    private TypeVariable InitVar(string name, IValue initVal, Func<IValue?, bool> checker, string typeName)
    {
        TypeVariable var = new(name, initVal, checker, typeName);
        Vars.Add(var);
        return var;
    }

    private void SetHVSpeeds()
    {
        if (Speed.Value == null)
            throw new NullReferenceException($"Property {nameof(Speed).StartWithLowerCase()} cannot be null.");
        if (Direction.Value == null)
            throw new NullReferenceException($"Property {nameof(Direction).StartWithLowerCase()} cannot be null.");

        var (h, v) = Math.Formulas.LengthDir(Speed.Value.Number, (int)Direction.Value.Number);

        hspeed = h.ToExp();
        vspeed = v.ToExp();
    }

    private void SetSpeedAndDir()
    {
        if (hspeed == null)
            throw new NullReferenceException($"Property {nameof(Hspeed).StartWithLowerCase()} cannot be null.");
        if (vspeed == null)
            throw new NullReferenceException($"Property {nameof(Vspeed).StartWithLowerCase()} cannot be null.");

        var (s, d) = Math.Formulas.SpeedsToVelocity(hspeed.Value, vspeed.Value);

        speed = s.ToExp();
        direction = d.ToExp();
    }

    public void SetDepth(IValue? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        depth = value.Number;
        DepthChanged?.Invoke(this, depth);
    }

    private void InitMask()
    {
        var instMask = Model.Sprite?.Mask;

        if (instMask == null)
            return;

        Mask = new MirrorRect(this)
        {
            Width = instMask.Right - instMask.Left + 1,
            Height = instMask.Bottom - instMask.Top + 1,
            OriginX = Model.Sprite!.OriginX - instMask.Left + 1,
            OriginY = Model.Sprite.OriginY - instMask.Top + 1
        };
    }

    // consider: [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void TickAlarms(Interpreter runner)
    {
        for (uint i = 0; i < NUMBER_OF_ALARMS; i++)
        {
            if (Alarm[i].number >= 0)
            {
                if (Alarm[i].number == 0)
                {
                    // trigger the event
                    foreach (var script in Alarm[i].ev?.Docs ?? [])
                        script.Run(runner, this);
                }

                Alarm[i].number--;
            }
        }
    }

    private class MirrorRect(Instance src) : Rect
    {
        public override double X { get => src.X.Value!.Number; set => src.X.Value = value.ToExp(); }
        public override double Y { get => src.Y.Value!.Number; set => src.Y.Value = value.ToExp(); }
        public override double Angle { get => src.ImageAngle.Value!.Number; set => src.ImageAngle.Value = value.ToExp(); }
    }
}
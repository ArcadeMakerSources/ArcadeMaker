using System;
using System.Collections.Generic;
using System.Text;
using Exp;
using ArcadeMaker.Core.Math;
using ArcadeMaker.Core.Math.Shapes;
using ArcadeMaker.Core.Models;
using Exp.Spans;
using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.Runtime;
using ArcadeMaker.Core.Resources;
using System.Drawing;

namespace ArcadeMaker.Core;

public partial interface IGame
{
    [ExpFunc(1, CustomName = "debug")]
    Exp.Void DebugLog(Exp.Instance? _, IValue?[] args)
    {
        Debug.WriteLine("".ToExpString() + (args[0]?.ToString() ?? "NULL"));

        return Exp.Void.Return;
    }

    [ExpFunc(1)]
    BoolValue KeyDown(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    BoolValue KeyPress(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    BoolValue KeyRelease(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    BoolValue MouseButtonDown(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    BoolValue MouseButtonPress(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    BoolValue MouseButtonRelease(Exp.Instance? _, IValue?[] args);

    [ExpFunc]
    IValue GetMouseX(Exp.Instance? _, IValue?[] args);

    [ExpFunc]
    IValue GetMouseY(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    BoolValue GamepadButtonDown(Exp.Instance? _, IValue?[] args);

    [ExpFunc(2, IsNonStaticFuncOfGameObjects = true)]
    IValue PointMeeting(Exp.Instance? expinst, IValue?[] args)
    {
        // arguments
        var inst = (Runtime.Instance)expinst!;
        var x = args[0].ThrowIfNull().Number;
        var y = args[1].ThrowIfNull().Number;

        // create a Rect
        var instMask = inst.Model.Sprite!.Mask;
        var instRect = new Rect
        {
            X = inst.X.Value!.Number,
            Y = inst.Y.Value!.Number,
            Width = instMask.Right - instMask.Left + 1,
            Height = instMask.Bottom - instMask.Top + 1,
            OriginX = inst.Model.Sprite.OriginX - instMask.Left + 1,
            OriginY = inst.Model.Sprite.OriginY - instMask.Top + 1,
            Angle = inst.ImageAngle.Value!.Number
        };

        return SeperatingAxisTheorem.IsPointInRectangle(x, y, instRect).ToExp();
    }

    [ExpFunc(3, IsNonStaticFuncOfGameObjects = true)]
    BoolValue PlaceMeeting(Exp.Instance? expinst, IValue?[] args)
    {
        var inst = (Runtime.Instance)expinst!;

        if (inst.Model.Sprite == null)
            return false;

        // get params
        if (args[2] == null)
            return false;

        var x = args[0].ThrowIfNull().Number;
        var y = args[1].ThrowIfNull().Number;

        // if 3rd argument is an instance, only check it
        if (args[2] is Runtime.Instance other)
            return PlaceMeeting(x, y, other);

        // if 3rd argument is a type, check all instances of that type
        else if (args[2]!.IsInst)
        {
            foreach (var i in GetActivatedRoom().Instances)
            {
                if (i.Model.Sprite != null && i.def.ExpType == args[2] && ((IValue)PlaceMeeting(x, y, i)).Bool)
                    return true;
            }
            return false;
        }
        else
            throw new ArgumentException("Invalid argument type for PlaceMeeting", paramName: nameof(args) + "[2]");

        BoolValue PlaceMeeting(double x, double y, Runtime.Instance other)
        {
            if (inst == other)
                return false;

            var instMask = inst.Model.Sprite!.Mask;
            var instRect = new Rect
            {
                X = x,
                Y = y,
                Width = instMask.Right - instMask.Left + 1,
                Height = instMask.Bottom - instMask.Top + 1,
                OriginX = inst.Model.Sprite.OriginX - instMask.Left + 1,
                OriginY = inst.Model.Sprite.OriginY - instMask.Top + 1,
                Angle = inst.ImageAngle.Value!.Number
            };

            var otherMask = other.Model.Sprite!.Mask;
            var otherRect = new Rect
            {
                X = other.X.Value!.Number,
                Y = other.Y.Value!.Number,
                Width = otherMask.Right - otherMask.Left + 1,
                Height = otherMask.Bottom - otherMask.Top + 1,
                OriginX = other.Model.Sprite.OriginX - otherMask.Left + 1,
                OriginY = other.Model.Sprite.OriginY - otherMask.Top + 1,
                Angle = other.ImageAngle.Value!.Number
            };

            bool result = SeperatingAxisTheorem.AreRectanglesIntersecting(instRect, otherRect);

            return result;
        }
    }

    [ExpFunc(1, IsNonStaticFuncOfGameObjects = true)]
    Runtime.Instance? InstanceMeeting(Exp.Instance? expinst, IValue?[] args)
    {
        foreach (var other in GetActivatedRoom().Instances)
        {
            if (PlaceMeeting(expinst, [args[0], args[1], other]))
                return other;
        }

        return null;
    }

    [ExpFunc(2, IsNonStaticFuncOfGameObjects = true)]
    BoolValue PlaceFree(Exp.Instance? expinst, IValue?[] args)
    {
        foreach (var other in GetActivatedRoom().Instances.Where(i => i.Solid.Value!.Bool))
        {
            if (PlaceMeeting(expinst, [args[0], args[1], other]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Returns the instance in the room which is the nearest to a given point.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args">(pointX, pointY, type (null for any)).</param>
    /// <returns>The nearest instance, or <c>null</c> if there is no any instance in the room.</returns>
    [ExpFunc(3)]
    Runtime.Instance? NearestInstance(Exp.Instance? _, IValue?[] args)
    {
        Extensions.ThrowIfNull(args[0], args[1]);

        KeyValuePair<Runtime.Instance?, double> nearest = new(null, -1);
        bool first = true;
        foreach (var inst in GetActivatedRoom().Instances)
        {
            if (args[2] == null || args[2] == inst.Model.Class.ExpType)
            {
                double distance = Math.Formulas.DistanceBetween(args[0]!.Number, args[1]!.Number, inst.X.Value!.Number, inst.Y.Value!.Number);
                if (first || distance < nearest.Value)
                    nearest = new(inst, distance);
                first = false;
            }
        }

        return nearest.Key;
    }

    /// <summary>
    /// Returns the instance in the room which is the furthest from a given point.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args">(pointX, pointY, type (null for any)).</param>
    /// <returns>The furthest instance, or <c>null</c> if there is no any instance in the room.</returns>
    [ExpFunc(3)]
    Runtime.Instance? FurthestInstance(Exp.Instance? _, IValue?[] args)
    {
        Extensions.ThrowIfNull(args[0], args[1]);

        KeyValuePair<Runtime.Instance?, double> furthest = new(null, -1);
        foreach (var inst in GetActivatedRoom().Instances)
        {
            if (args[2] == null || args[2] == inst.Model.Class.ExpType)
            {
                double distance = Math.Formulas.DistanceBetween(args[0]!.Number, args[1]!.Number, inst.X.Value!.Number, inst.Y.Value!.Number);
                if (distance > furthest.Value)
                    furthest = new(inst, distance);
            }
        }

        return furthest.Key;
    }

    [ExpFunc(1)]
    IValue InstanceCount(Exp.Instance? _, IValue?[] args)
    {
        if (args.Length == 0)
            throw new ArgumentException("Object type (or null) must be passed.");

        int count = 0;
        foreach (var i in GetActivatedRoom().Instances)
        {
            if (args[0] == null || args[0] == i.Model.Class.ExpType)
                count++;
        }

        return count.ToExp();
    }

    [ExpFunc(1)]
    Exp.Void ShowMessage(Exp.Instance? _, IValue?[] args);

    [ExpFunc(3)]
    Runtime.Instance CreateInstance(Exp.Instance? _, IValue?[] args)
    {
        var (x, y, type) = args.ValidateArguments<NumberValue, NumberValue, Exp.Instance>();
        ObjectModel model = Objects.FirstOrDefault(m => m.Class.ExpType == type) ?? throw new ArgumentException("Value of argument type must be a type of a game object.");
        Runtime.Instance inst = new(model);
        inst.X.Value = x;
        inst.Y.Value = y;
        GetActivatedRoom().AddInstance(inst);
        return inst;
    }

    [ExpFunc(4, 6)]
    Exp.Void DrawSprite(Exp.Instance? _, IValue?[] args);

    [ExpFunc(3)]
    Exp.Void DrawText(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    Exp.Void SetFont(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    Exp.Void SetColor(Exp.Instance? _, IValue?[] args);

    [ExpFunc(5)]
    Exp.Void DrawLine(Exp.Instance? _, IValue?[] args)
    {
        double x1 = args[0].ThrowIfNull().Number;
        double y1 = args[1].ThrowIfNull().Number;
        double x2 = args[2].ThrowIfNull().Number;
        double y2 = args[3].ThrowIfNull().Number;
        double thickness = args[4].ThrowIfNull().Number;

        DrawLine(x1, y1, x2, y2, thickness);

        return Exp.Void.Return;
    }

    [ExpFunc(2, 4)]
    Exp.Void DrawPath(Exp.Instance? _, IValue?[] args)
    {
        // get parameters
        Resources.Path path = Paths.FirstOrDefault(p => p.ID == (int)args[0].ThrowIfNull().Number) ?? throw new ArgumentException($"No path with ID {(int)args[4].ThrowIfNull().Number} was found.");

        double x = path.StartPositionX, y = path.StartPositionY;
        if (args.Length == 4)
        {
            x = args[1].ThrowIfNull().Number;
            y = args[2].ThrowIfNull().Number;
        }

        double thickness = args.Last().ThrowIfNull().Number;

        // draw path
        foreach (var point in path.Steps)
        {
            DrawLine(x, y, x + point.Width, y - point.Height, thickness);
            x += point.Width;
            y -= point.Height;
        }

        return Exp.Void.Return;
    }

    [ExpFunc(1, 2, 3, 4, 5)]
    Exp.Instance? PlaySound(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    Exp.Void PauseSound(Exp.Instance? _, IValue?[] args);

    [ExpFunc]
    Exp.Void PauseAllSounds(Exp.Instance? _, IValue?[] args);

    [ExpFunc(1)]
    Exp.Void ResumeSound(Exp.Instance? _, IValue?[] args);

    [ExpFunc]
    Exp.Void ResumeAllSounds(Exp.Instance? _, IValue?[] args);

    [ExpFunc(4, IsNonStaticFuncOfGameObjects = true)]
    Exp.Void StartPath(Exp.Instance? expinst, IValue?[] args)
    {
        Runtime.Instance inst = (Runtime.Instance)expinst!;

        // get path
        int id = (int)args[0].ThrowIfNull().Number;
        Resources.Path path = Paths.FirstOrDefault(p => p.ID == id) ?? throw new ArgumentException($"No path with ID {id} was found.");

        // get speed
        double speed = args[1].ThrowIfNull().Number;

        // get end action
        PathEndAction endAction = (PathEndAction)args[2].ThrowIfNull().Number;

        // if absolute == true, move to start point
        if (args[3].ThrowIfNull().Bool)
        {
            inst.X.Value = path.StartPositionX.ToExp();
            inst.Y.Value = path.StartPositionY.ToExp();
        }

        // create PathDrive instance
        inst.CurrentPathDrive = new(inst, path, speed, endAction, inst.X.Value!.Number, inst.Y.Value!.Number);

        return Exp.Void.Return;
    }

    [ExpFunc(IsNonStaticFuncOfGameObjects = true)]
    Exp.Void StopPath(Exp.Instance? expinst, IValue?[] args)
    {
        Runtime.Instance inst = (Runtime.Instance)expinst!;

        inst.CurrentPathDrive = null;
        inst.Speed.Value = 0d.ToExp();

        return Exp.Void.Return;
    }

    [ExpFunc(1, IsNonStaticFuncOfGameObjects = true)]
    IValue GetAlarm(Exp.Instance? expinst, IValue?[] args)
    {
        return ((Runtime.Instance)expinst!).Alarm[(int)args[0].ThrowIfNull().Number].number.ToExp();
    }

    [ExpFunc(2, IsNonStaticFuncOfGameObjects = true)]
    Exp.Void SetAlarm(Exp.Instance? expinst, IValue?[] args)
    {
        ((Runtime.Instance)expinst!).Alarm[(int)args[0].ThrowIfNull().Number].number = (int)args[1].ThrowIfNull().Number;
        return Exp.Void.Return;
    }

    [ExpFunc]
    IValue GetRoomWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Width.ToExp();

    [ExpFunc]
    IValue GetRoomHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Height.ToExp();

    [ExpFunc(1)]
    IValue GetViewX(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].X.ToExp();

    [ExpFunc(1)]
    IValue GetViewY(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Y.ToExp();

    [ExpFunc(1)]
    IValue GetViewWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Width.ToExp();

    [ExpFunc(1)]
    IValue GetViewHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Height.ToExp();

    [ExpFunc(1)]
    IValue GetViewPortX(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortX.ToExp();

    [ExpFunc(1)]
    IValue GetViewPortY(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortY.ToExp();

    [ExpFunc(1)]
    IValue GetViewPortWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortWidth.ToExp();

    [ExpFunc(1)]
    IValue GetViewPortHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortHeight.ToExp();
}
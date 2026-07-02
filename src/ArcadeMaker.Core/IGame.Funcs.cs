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
    /// <summary>
    /// Writes a value to the debug output.
    /// </summary>
    /// <param name="_">The calling EXP instance (may be null for global calls).</param>
    /// <param name="args">An array of arguments; the first element is converted to string and written to the debug output.</param>
    [ExpFunc(1, CustomName = "debug")]
    Exp.Void DebugLog(Exp.Instance? _, IValue?[] args)
    {
        Debug.WriteLine("".ToExpString() + (args[0]?.ToString() ?? "NULL"));

        return Exp.Void.Return;
    }

    /// <summary>
    /// Checks whether the specified keyboard key is currently down.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the key code to check (You can use the <see cref="ExpSrc.Controls.Keys"/> enum for this.</param>
    /// <returns>True if the key is currently down; otherwise false.</returns>
    [ExpFunc(1)]
    BoolValue KeyDown(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified keyboard key was pressed this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the key code to check (You can use the <see cref="ExpSrc.Controls.Keys"/> enum for this.</param>
    /// <returns>True if the key was pressed this frame; otherwise false.</returns>
    [ExpFunc(1)]
    BoolValue KeyPress(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified keyboard key was released this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the key code to check (You can use the <see cref="ExpSrc.Controls.Keys"/> enum for this.</param>
    /// <returns>True if the key was released this frame; otherwise false.</returns>
    [ExpFunc(1)]
    BoolValue KeyRelease(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified mouse button is currently down.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the mouse button index to check (You can use the <see cref="ExpSrc.Controls.MouseButton"/> enum for this.</param>
    /// <returns>True if the mouse button is currently down; otherwise false.</returns>
    [ExpFunc(1)]
    BoolValue MouseButtonDown(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified mouse button was pressed this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the mouse button index to check (You can use the <see cref="ExpSrc.Controls.MouseButton"/> enum for this.</param>
    /// <returns>True if the mouse button was pressed this frame; otherwise false.</returns>
    [ExpFunc(1)]
    BoolValue MouseButtonPress(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified mouse button was released this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the mouse button index to check (You can use the <see cref="ExpSrc.Controls.MouseButton"/> enum for this.</param>
    /// <returns>True if the mouse button was released this frame; otherwise false.</returns>
    [ExpFunc(1)]
    BoolValue MouseButtonRelease(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Gets the X-coordinate of the mouse in room coordinates.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The mouse X position as a <see cref="Exp.NumberValue"/>.</returns>
    [ExpFunc]
    IValue GetMouseX(Exp.Instance? _, IValue?[] args)
    {
        var room = GetActivatedRoom();
        var (wx, wy) = MousePositionInWindow;

        // if views enabled, return x in room
        foreach (var view in room.Model.Views)
        {
            if (!view.Visible)
                continue;

            // if it's within the view's port, return in room position
            if (wx >= view.PortX && wx <= view.PortX + view.PortWidth && wy >= view.PortY && wy <= view.PortY + view.PortHeight)
            {
                // calculate room position
                return (view.X + ((wx - view.PortX) * (view.Width / view.PortWidth))).ToExp();
            }
        }

        // if views disabled or mouse is not inside any view port, return window-relative x
        return wx.ToExp();
    }

    /// <summary>
    /// Gets the Y-coordinate of the mouse in room coordinates.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The mouse Y position as a <see cref="Exp.NumberValue"/>.</returns>
    [ExpFunc]
    IValue GetMouseY(Exp.Instance? _, IValue?[] args)
    {
        var room = GetActivatedRoom();
        var (wx, wy) = MousePositionInWindow;

        // if views enabled, return x in room
        foreach (var view in room.Model.Views)
        {
            if (!view.Visible)
                continue;

            // if it's within the view's port, return in room position
            if (wx >= view.PortX && wx <= view.PortX + view.PortWidth && wy >= view.PortY && wy <= view.PortY + view.PortHeight)
            {
                // calculate room position
                return (view.Y + ((wy - view.PortY) * (view.Height / view.PortHeight))).ToExp();
            }
        }

        // if views disabled or mouse is not inside any view port, return window-relative x
        return wy.ToExp();
    }

    /// <summary>
    /// Gets the mouse X-coordinate relative to the application window (not affected by views).
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The window-relative mouse X position as a <see cref="Exp.NumberValue"/>.</returns>
    [ExpFunc]
    IValue WindowGetMouseX(Exp.Instance? _, IValue?[] args) => MousePositionInWindow.x.ToExp();

    /// <summary>
    /// Gets the mouse Y-coordinate relative to the application window (not affected by views).
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The window-relative mouse Y position as a <see cref="Exp.NumberValue"/>.</returns>
    [ExpFunc]
    IValue WindowGetMouseY(Exp.Instance? _, IValue?[] args) => MousePositionInWindow.y.ToExp();

    /// <summary>
    /// Checks whether the specified gamepad button is currently down.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the button index to check (You can use the <see cref="ExpSrc.Controls.GamepadButton"/> enum for this.</param>
    /// <returns>True if the specified gamepad button is down; otherwise false.</returns>
    [ExpFunc(1)]
    BoolValue GamepadButtonDown(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Tests whether a given point lies within the bounding rectangle of the calling instance.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the point X and Y coordinates.</param>
    /// <returns>True if the point lies inside the instance's mask rectangle; otherwise false.</returns>
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

    /// <summary>
    /// Tests whether placing the calling instance at a given position would collide with another instance or type.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates and args[2]
    /// is either an instance to check against or an object type to test for collisions.</param>
    /// <returns>True if the placement would result in a collision; otherwise false.</returns>
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

    /// <summary>
    /// Returns an instance that would be collided with if the calling instance were placed at the specified point.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates and args[2] is the type to search for.</param>
    /// <returns>The first instance found that collides at the specified position, or null if none found.</returns>
    [ExpFunc(1, IsNonStaticFuncOfGameObjects = true)]
    Runtime.Instance? InstanceMeeting(Exp.Instance? expinst, IValue?[] args)
    {
        foreach (var other in GetActivatedRoom().Instances)
        {
            if (other.Model.Class.ExpType == args[2] && PlaceMeeting(expinst, [args[0], args[1], other]))
                return other;
        }

        return null;
    }

    /// <summary>
    /// Determines whether the specified position is free of solid instances for the calling instance.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates to test.</param>
    /// <returns>True if the position is free; otherwise false.</returns>
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
    /// Determines whether the calling instance is outside the bounds of the active room.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Unused.</param>
    /// <returns>True if the instance is outside the room; otherwise false.</returns>
    [ExpFunc(IsNonStaticFuncOfGameObjects = true)]
    BoolValue OutsideRoom(Exp.Instance? expinst, IValue?[] args)
    {
        var room = GetActivatedRoom().Model;
        var inst = (Runtime.Instance)expinst!;

        Rect roomRect = new Rect
        {
            X = 0,
            Y = 0,
            Width = room.Width,
            Height = room.Height,
            OriginX = 0,
            OriginY = 0,
            Angle = 0
        };

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

        return !Math.SeperatingAxisTheorem.AreRectanglesIntersecting(roomRect, instRect);
    }

    /// <summary>
    /// Returns the instance in the room which is the nearest to a given point.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args">(pointX, pointY, type (null for any)).</param>
    /// <returns>The nearest instance, or <c>null</c> if there is no any instance in the room.</returns>
    /// <summary>
    /// Returns the instance in the active room nearest to the provided point.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] and args[1] are the point X and Y coordinates and args[2] is an optional type to filter by (null for any).</param>
    /// <returns>The nearest instance or null if no matching instances exist.</returns>
    [ExpFunc(3)]
    Runtime.Instance? NearestInstance(Exp.Instance? _, IValue?[] args)
    {
        Extensions.ThrowIfNull(args[0], args[1]);

        KeyValuePair<Runtime.Instance?, double> nearest = new(null, -1);
        bool first = true;
        foreach (var inst in GetActivatedRoom().Instances)
        {
            if (args.Length >= 3 && (args[2] == null || args[2] == inst.Model.Class.ExpType))
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
    /// <summary>
    /// Returns the instance in the active room furthest from the provided point.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] and args[1] are the point X and Y coordinates and args[2] is an optional type to filter by (null for any).</param>
    /// <returns>The furthest instance or null if no matching instances exist.</returns>
    [ExpFunc(3)]
    Runtime.Instance? FurthestInstance(Exp.Instance? _, IValue?[] args)
    {
        Extensions.ThrowIfNull(args[0], args[1]);

        KeyValuePair<Runtime.Instance?, double> furthest = new(null, -1);
        foreach (var inst in GetActivatedRoom().Instances)
        {
            if (args.Length >= 3 && (args[2] == null || args[2] == inst.Model.Class.ExpType))
            {
                double distance = Math.Formulas.DistanceBetween(args[0]!.Number, args[1]!.Number, inst.X.Value!.Number, inst.Y.Value!.Number);
                if (distance > furthest.Value)
                    furthest = new(inst, distance);
            }
        }

        return furthest.Key;
    }

    /// <summary>
    /// Counts the number of instances of a given type (or all instances if null) in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the object type to count, or null to count all instances.</param>
    /// <returns>The number of matching instances as an <see cref="Exp.NumberValue"/>.</returns>
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

    /// <summary>
    /// Shows a message to the player.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the message to show.</param>
    [ExpFunc(1)]
    Exp.Void ShowMessage(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Creates a new instance of the specified object type at the given coordinates and adds it to the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] and args[1] are the spawn X and Y coordinates and args[2] is the object type to instantiate.</param>
    /// <returns>The newly created runtime instance.</returns>
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

    /// <summary>
    /// Sets the calling instance's direction and speed so it moves towards the specified point.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates and args[2] is the desired speed.</param>
    /// <returns>Void.</returns>
    [ExpFunc(3, IsNonStaticFuncOfGameObjects = true)]
    Exp.Void MoveTowardsPoint(Exp.Instance? expinst, IValue?[] args)
    {
        var inst = (Runtime.Instance)expinst!;

        // set direction
        var direction = Math.Formulas.AngleBetween(inst.X.Value!.Number, inst.Y.Value!.Number * -1, args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number * -1) - 90;
        inst.Direction.Value = direction.ToExp();

        // set speed
        if (inst.speed != args[2].ThrowIfNull().Number)
            inst.Speed.Value = args[2];

        return Exp.Void.Return;
    }

    /// <summary>
    /// Draws a specific subimage of a sprite.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[x, y, spriteId, imageIndex, [angle, alphaColor]].</param>
    [ExpFunc(4, 6)]
    Exp.Void DrawSprite(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Draws text to the screen using the current font and color settings.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[x, y, text?].</param>
    [ExpFunc(3)]
    Exp.Void DrawText(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Sets the current font for subsequent text drawing operations.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[fontId].</param>
    [ExpFunc(1)]
    Exp.Void SetFont(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Sets the current drawing color used for primitives and text.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[abgrColor]. You can use the <see cref="ExpSrc.Drawing.Color"/> enum for this.</param>
    [ExpFunc(1)]
    Exp.Void SetColor(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Draws a line between two points with the specified thickness.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[x1, y1, x2, y2, lineThickness].</param>
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

    /// <summary>
    /// Draws a predefined path resource starting at the path's start position or an optional provided position.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[pathId, [x, y], libeThickness].</param>
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

    /// <summary>
    /// Plays a sound.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[soundId, [looping], [volume], [pan], [pitch]].</param>
    /// <returns>A <see cref="Runtime.SoundPlaybackInstance{T}"/> instance can be used to control the playback.</returns>
    [ExpFunc(1, 2, 3, 4, 5)]
    Exp.Instance? PlaySound(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Pauses a currently playing sound instance.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[soundId].</param>
    [ExpFunc(1)]
    Exp.Void PauseSound(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Pauses all currently playing sounds.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    [ExpFunc]
    Exp.Void PauseAllSounds(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Resumes a paused sound instance.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[soundId].</param>
    [ExpFunc(1)]
    Exp.Void ResumeSound(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Resumes all paused sounds.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    [ExpFunc]
    Exp.Void ResumeAllSounds(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Starts the instance following a path resource.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] is the path ID, args[1] is the movement speed, args[2] is the PathEndAction and args[3] is a boolean indicating if the instance should be moved to the path start before.</param>
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

    /// <summary>
    /// Stops any active path movement for the instance and resets its speed to zero.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Unused.</param>
    [ExpFunc(IsNonStaticFuncOfGameObjects = true)]
    Exp.Void StopPath(Exp.Instance? expinst, IValue?[] args)
    {
        Runtime.Instance inst = (Runtime.Instance)expinst!;

        inst.CurrentPathDrive = null;
        inst.Speed.Value = 0d.ToExp();

        return Exp.Void.Return;
    }

    /// <summary>
    /// Gets the current value of the specified alarm for the given instance.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] is the alarm index to retrieve.</param>
    /// <returns>The alarm value as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1, IsNonStaticFuncOfGameObjects = true)]
    IValue GetAlarm(Exp.Instance? expinst, IValue?[] args)
    {
        return ((Runtime.Instance)expinst!).Alarm[(int)args[0].ThrowIfNull().Number].number.ToExp();
    }

    /// <summary>
    /// Sets the value of the specified alarm for the given instance.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] is the alarm index and args[1] is the value to set.</param>
    [ExpFunc(2, IsNonStaticFuncOfGameObjects = true)]
    Exp.Void SetAlarm(Exp.Instance? expinst, IValue?[] args)
    {
        ((Runtime.Instance)expinst!).Alarm[(int)args[0].ThrowIfNull().Number].number = (int)args[1].ThrowIfNull().Number;
        return Exp.Void.Return;
    }

    /// <summary>
    /// Gets the active room width.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The width of the active room as a <see cref="NumberValue"/>.</returns>
    [ExpFunc]
    IValue GetRoomWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Width.ToExp();

    /// <summary>
    /// Gets the active room height.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The height of the active room as a <see cref="NumberValue"/>.</returns>
    [ExpFunc]
    IValue GetRoomHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Height.ToExp();

    /// <summary>
    /// Gets the X position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view X position as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewX(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].X.ToExp();

    /// <summary>
    /// Gets the Y position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view Y position as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewY(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Y.ToExp();

    /// <summary>
    /// Gets the width of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view width as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Width.ToExp();

    /// <summary>
    /// Gets the height of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view height a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Height.ToExp();

    /// <summary>
    /// Gets the port X position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port X position as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewPortX(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortX.ToExp();

    /// <summary>
    /// Gets the port Y position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port Y position as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewPortY(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortY.ToExp();

    /// <summary>
    /// Gets the port width of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port width as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewPortWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortWidth.ToExp();

    /// <summary>
    /// Gets the port height of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port height as a <see cref="NumberValue"/>.</returns>
    [ExpFunc(1)]
    IValue GetViewPortHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortHeight.ToExp();
}
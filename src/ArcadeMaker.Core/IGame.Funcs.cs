using ArcadeMaker.Core.Exceptions;
using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.Math;
using ArcadeMaker.Core.Math.Shapes;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Resources;
using ArcadeMaker.Core.Runtime;
using Exp;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace ArcadeMaker.Core;

public partial interface IGame
{
    /// <summary>
    /// Writes a value to the debug output.
    /// </summary>
    /// <param name="_">The calling EXP instance (may be null for global calls).</param>
    /// <param name="args">An array of arguments; the first element is converted to string and written to the debug output.</param>
    [EngineFunc(1, CustomName = "debug")]
    [Param("output", ParamType.Any, "The output to print to the debug console.")]
    public Exp.Void DebugLog(Exp.Instance? _, IValue?[] args)
    {
        DebugConsole.WriteLine(this, args[0]);
        return Exp.Void.Return;
    }

    /// <summary>
    /// Blocks the game thread until an input is received from the debug console, and returns the received input.
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(Unused).</param>
    /// <returns>The received input.</returns>
    [EngineFunc(0, 1)]
    [Param("message", ParamType.Any, "A message to print to the debug console before picking the value.", Optional = true)]
    public IValue DebugReadLine(Exp.Instance? _, IValue?[] args)
    {
        if (args is { Length: > 0 })
            DebugLog(null, [args[0]]);

        return DebugConsole.ReadLine().ToExpString();
    }

    /// <summary>
    /// Blocks the game thread until an input number is received from the debug console, and returns it.
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(Unused).</param>
    /// <returns>The received input number.</returns>
    [EngineFunc(0, 1)]
    [Param("message", ParamType.Any, "A message to print to the debug console before picking the value.", Optional = true)]
    IValue DebugReadNumber(Exp.Instance? _, IValue?[] args)
    {
        if (args is { Length: > 0 })
            DebugLog(null, [args[0]]);

        double num = double.Parse(DebugConsole.ReadLine(line => double.TryParse(line, out num) ? null : "A number was expected."));
        return num.ToExp();
    }

    /// <summary>
    /// Returns an array of all the keyboard keys that are currently pressed.
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(Unused).</param>
    /// <returns>An <see cref="Exp.Instance"/> which its <see cref="Exp.Instance.ArrayValues"/> property contains all of the currently pressed keys.</returns>
    [EngineFunc]
    Exp.Instance GetPressedKeys(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Returns an array of values holding all of the keyboard keys that are currently being pressed.
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(Unused).</param>
    /// <returns>An <see cref="Exp.Instance"/> which its <see cref="Exp.Instance.ArrayValues"/> property contains all of the currently pressed keys.</returns>
    [EngineFunc]
    IValue? GetPressedKey(Exp.Instance? _, IValue?[] args)
    {
        var keys = GetPressedKeys(_, args);
        if (keys.ArrayValues?.Length > 0)
            return keys.ArrayValues[0];
        return null;
    }

    /// <summary>
    /// Checks whether the specified keyboard key is currently down.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the key code to check (You can use the <see cref="ExpSrc.Controls.Keys"/> enum for this.</param>
    /// <returns>True if the key is currently down; otherwise false.</returns>
    [EngineFunc(1)]
    [Param("key", ParamType.Key, "The key to test its state.")]
    BoolValue KeyDown(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified keyboard key was pressed this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the key code to check (You can use the <see cref="ExpSrc.Controls.Keys"/> enum for this.</param>
    /// <returns>True if the key was pressed this frame; otherwise false.</returns>
    [EngineFunc(1)]
    [Param("key", ParamType.Key, "The key to test its state.")]
    BoolValue KeyPress(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified keyboard key was released this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the key code to check (You can use the <see cref="ExpSrc.Controls.Keys"/> enum for this.</param>
    /// <returns>True if the key was released this frame; otherwise false.</returns>
    [EngineFunc(1)]
    [Param("key", ParamType.Key, "The key to test its state.")]
    BoolValue KeyRelease(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified mouse button is currently down.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the mouse button index to check (You can use the <see cref="ExpSrc.Controls.MouseButton"/> enum for this.</param>
    /// <returns>True if the mouse button is currently down; otherwise false.</returns>
    [EngineFunc(1)]
    [Param("button", ParamType.MouseButton, "The button to test its state.")]
    BoolValue MouseButtonDown(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified mouse button was pressed this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the mouse button index to check (You can use the <see cref="ExpSrc.Controls.MouseButton"/> enum for this.</param>
    /// <returns>True if the mouse button was pressed this frame; otherwise false.</returns>
    [EngineFunc(1)]
    [Param("button", ParamType.MouseButton, "The button to test its state.")]
    BoolValue MouseButtonPress(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Checks whether the specified mouse button was released this frame.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the mouse button index to check (You can use the <see cref="ExpSrc.Controls.MouseButton"/> enum for this.</param>
    /// <returns>True if the mouse button was released this frame; otherwise false.</returns>
    [EngineFunc(1)]
    [Param("button", ParamType.MouseButton, "The button to test its state.")]
    BoolValue MouseButtonRelease(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Gets the X-coordinate of the mouse in room coordinates.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The mouse X position as a <see cref="Exp.NumberValue"/>.</returns>
    [EngineFunc]
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
    [EngineFunc]
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
    [EngineFunc]
    IValue WindowGetMouseX(Exp.Instance? _, IValue?[] args) => MousePositionInWindow.x.ToExp();

    /// <summary>
    /// Gets the mouse Y-coordinate relative to the application window (not affected by views).
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The window-relative mouse Y position as a <see cref="Exp.NumberValue"/>.</returns>
    [EngineFunc]
    IValue WindowGetMouseY(Exp.Instance? _, IValue?[] args) => MousePositionInWindow.y.ToExp();

    /// <summary>
    /// Checks whether the specified gamepad button is currently down.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the gamepad number and args[1] is the button index to check (You can use the <see cref="ExpSrc.Controls.GamepadButton"/> enum for this.</param>
    /// <returns>True if the specified gamepad button is down; otherwise false.</returns>
    [EngineFunc(1)]
    [Param("gamepad", ParamType.Number, "The gamepad number to check.")]
    [Param("button", ParamType.GamepadButton, "The button to test its state.")]
    BoolValue GamepadButtonDown(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Tests whether a given point lies within the bounding rectangle of the calling instance.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the point X and Y coordinates.</param>
    /// <returns>True if the point lies inside the instance's mask rectangle; otherwise false.</returns>
    [EngineFunc(2, IsNonStaticFuncOfGameObjects = true)]
    [Param("x", ParamType.Number, "Point x.")]
    [Param("y", ParamType.Number, "Point y.")]
    IValue PointMeeting(Exp.Instance? expinst, IValue?[] args)
    {
        // arguments
        var inst = (Runtime.Instance)expinst!;
        var x = args[0].ThrowIfNull().Number;
        var y = args[1].ThrowIfNull().Number;

        // false if no mask
        if (inst.Mask == null)
            return false.ToExp();

        return SeparatingAxisTheorem.IsPointInRectangle(x, y, inst.Mask).ToExp();
    }

    /// <summary>
    /// Tests whether placing the calling instance at a given position would collide with another instance or type.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates and args[2]
    /// is either an instance to check against or an object type to test for collisions.</param>
    /// <returns>True if the placement would result in a collision; otherwise false.</returns>
    [EngineFunc(2, 3, 4, IsNonStaticFuncOfGameObjects = true)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    [Param("type", ParamType.Type + " / " + ParamType.GameObject, "The object type to check for collision with, or a reference for a spesific instance.", Optional = true)]
    [Param("angle", ParamType.Number, "The angle to test for collision at (default is current imageAngle value)", Optional = true)]
    BoolValue PlaceMeeting(Exp.Instance? expinst, IValue?[] args)
    {
        var inst = (Runtime.Instance)expinst!;

        if (inst.Sprite == null)
            return false;

        var x = args[0].ThrowIfNull().Number;
        var y = args[1].ThrowIfNull().Number;
        int angle = (int)(args.Length >= 4 ? args[3].ThrowIfNull() : inst.ImageAngle.Value!).Number;

        // if 3rd argument is an instance, only check it
        if (args.Length >= 3 && args[2] is Runtime.Instance other)
            return PlaceMeeting(x, y, angle, other);

        // if 3rd argument is a type, check all instances of that type
        else if (args[2]!.IsInst) // IsInst means Exp.Instance, which is what system::Type is
        {
            foreach (var i in GetActivatedRoom().Instances)
            {
                if ((args.Length == 2 || (i.Sprite != null && i.def.ExpType == args[2])) && ((IValue)PlaceMeeting(x, y, angle, i)).Bool)
                    return true;
            }
            return false;
        }
        else
            throw new ArgumentException("Invalid argument type for PlaceMeeting", paramName: nameof(args) + "[2]");

        BoolValue PlaceMeeting(double x, double y, int angle, Runtime.Instance other)
        {
            if (inst == other || inst.Mask is null || other.Mask is null)
                return false;

            double originalX = inst.Mask.X, originalY = inst.Mask.Y, originalAngle = inst.Mask.Angle;
            inst.Mask.X = x; // note that this actually updates the value of inst.X
            inst.Mask.Y = y; // same
            inst.Mask.Angle = angle; // same

            bool result = SeparatingAxisTheorem.AreRectanglesIntersecting(inst.Mask, other.Mask);

            inst.Mask.X = originalX;
            inst.Mask.Y = originalY;
            inst.Mask.Angle = originalAngle;

            return result;
        }
    }

    /// <summary>
    /// Returns an instance that would be collided with if the calling instance were placed at the specified point.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates and args[2] is the type to search for.</param>
    /// <returns>The first instance found that collides at the specified position, or null if none found.</returns>
    [EngineFunc(2, 3, 4, IsNonStaticFuncOfGameObjects = true)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    [Param("type", ParamType.Type, "The object type to check for collision with.", Optional = true)]
    [Param("angle", ParamType.Number, "The angle to test for collision at (default is current imageAngle value)", Optional = true)]
    Runtime.Instance? InstanceMeeting(Exp.Instance? expinst, IValue?[] args)
    {
        foreach (var other in GetActivatedRoom().Instances)
        {
            IValue?[] fArgs = args.Length >= 4 ? [args[0], args[1], other, args[3]] : [args[0], args[1], other];
            if ((args.Length == 2 || other.Model.Class.ExpType == args[2]) && PlaceMeeting(expinst, fArgs))
                return other;
        }

        return null;
    }

    /// <summary>
    /// Determines whether the specified position is free of solid instances for the calling instance.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates to test.</param>
    /// <returns>true if the position is free; otherwise, false.</returns>
    [EngineFunc(2, 3, IsNonStaticFuncOfGameObjects = true)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    [Param("angle", ParamType.Number, "The angle to test at (default is current imageAngle value)", Optional = true)]
    BoolValue PlaceFree(Exp.Instance? expinst, IValue?[] args)
    {
        foreach (var other in GetActivatedRoom().Instances)
        {
            if (!other.Solid.Value!.Bool)
                continue;

            args = args.Length >= 3 ? [args[0], args[1], other, args[2]] : [args[0], args[1], other];
            if (PlaceMeeting(expinst, [args[0], args[1], other]))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Determines whether the specified position is free of solid instances.
    /// </summary>
    /// <param name="expinst">(Unused).</param>
    /// <param name="args">(x, y).</param>
    /// <returns>true if the position is free; otherwise, false.</returns>
    [EngineFunc(2)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    BoolValue PointFree(Exp.Instance? expinst, IValue?[] args)
    {
        foreach (var other in GetActivatedRoom().Instances)
        {
            if (!other.Solid.Value!.Bool)
                continue;

            if (PointMeeting(other, [args[0], args[1]]).Bool)
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
    [EngineFunc(IsNonStaticFuncOfGameObjects = true)]
    BoolValue OutsideRoom(Exp.Instance? expinst, IValue?[] args)
    {
        var inst = (Runtime.Instance)expinst!;
        var roomBounds = GetActivatedRoom().Model.Bounds;

        if (inst.Mask != null)
            return !SeparatingAxisTheorem.AreRectanglesIntersecting(roomBounds, inst.Mask);

        double x = inst.X.Value!.Number, y = inst.Y.Value!.Number;
        return x >= roomBounds.X && x <= roomBounds.X + roomBounds.Width &&
               y >= roomBounds.Y && y <= roomBounds.Y + roomBounds.Height;
    }

    /// <summary>
    /// Returns the instance in the room which is the nearest to a given point.
    /// </summary>
    /// <param name="_"></param>
    /// <param name="args">(pointX, pointY, type (null for any)).</param>
    /// <returns>The nearest instance, or <c>null</c> if there is no any instance in the room.</returns>
    [EngineFunc(2, 3, 4)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    [Param("type", ParamType.Number, "The type to get its nearest instance.", Optional = true)]
    [Param("ignore", ParamType.GameObject, "An instance to ignore.", Optional = true)]
    Runtime.Instance? NearestInstance(Exp.Instance? _, IValue?[] args)
    {
        args[0].ThrowIfNull();
        args[1].ThrowIfNull();
        IValue? type = args.Length >= 3 ? args[2] : null;
        IValue? instToIgnore = args.Length >= 4 ? args[3] : null;

        KeyValuePair<Runtime.Instance?, double> nearest = new(null, -1);
        bool first = true;
        foreach (var inst in GetActivatedRoom().Instances)
        {
            if (inst != instToIgnore && (type == null || type == inst.Model.Class.ExpType))
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
    [EngineFunc(2, 3, 4)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    [Param("type", ParamType.Number, "The type to get its furthest instance.", Optional = true)]
    [Param("ignore", ParamType.GameObject, "An instance to ignore.", Optional = true)]
    Runtime.Instance? FurthestInstance(Exp.Instance? _, IValue?[] args)
    {
        Extensions.ThrowIfNull(args[0], args[1]);
        IValue? type = args.Length >= 3 ? args[2] : null;
        IValue? instToIgnore = args.Length >= 4 ? args[3] : null;

        KeyValuePair<Runtime.Instance?, double> furthest = new(null, -1);
        foreach (var inst in GetActivatedRoom().Instances)
        {
            if (inst != instToIgnore && (type == null || type == inst.Model.Class.ExpType))
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
    [EngineFunc(0, 1)]
    [Param("type", ParamType.Type, "The type to count.", Optional = true)]
    IValue InstanceCount(Exp.Instance? _, IValue?[] args)
    {
        int count = 0;
        foreach (var i in GetActivatedRoom().Instances)
        {
            if (args.Length == 0 || args[0] == i.Model.Class.ExpType)
                count++;
        }

        return count.ToExp();
    }

    /// <summary>
    /// Shows a message to the player.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the message to show.</param>
    [EngineFunc(1)]
    [Param("message?", ParamType.Any, "The message to show to the user.")]
    Exp.Void ShowMessage(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Gets the text currently stored in the system clipboard, or null if the clipboard holds no text.
    /// </summary>
    [EngineFunc]
    IValue? GetClipboardText(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Copies the given text to the system clipboard.
    /// </summary>
    [EngineFunc(1)]
    [Param("text?", ParamType.String, "The text to copy to the clipboard.")]
    Exp.Void SetClipboardText(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Sets the calling instance's direction and speed so it moves towards the specified point.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] and args[1] are the target X and Y coordinates and args[2] is the desired speed.</param>
    /// <returns>Void.</returns>
    [EngineFunc(3, IsNonStaticFuncOfGameObjects = true)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    [Param("speed", ParamType.Number, "Movement speed.")]
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
    [EngineFunc(4, 5, 6)]
    [Param("x", ParamType.Number, "Drawing position's x.")]
    [Param("y", ParamType.Number, "Drawing position's y.")]
    [Param("spriteID", ParamType.Number, "The ID of the sprite which contains the image to draw. Use 'Sprites' enum for that, e.g. Sprites.SprEnemy")]
    [Param("imageIndex", ParamType.Number, "The index of the image to draw.")]
    [Param("angle", ParamType.Number, "The angle to draw the image in.", Optional = true)]
    [Param("alphaColor", ParamType.Number, "The alpha color mask.", Optional = true)]
    Exp.Void DrawSprite(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Draws text to the screen using the current font and color settings.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[x, y, text?].</param>
    [EngineFunc(3)]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position y.")]
    [Param("text?", ParamType.Any, "The text to draw.")]
    Exp.Void DrawText(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Returns the width of the specified text when drawing with the current / specified font.
    /// </summary>
    /// <returns>The width of the specified text when drawing with the current / specified font.</returns>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">(text?, [fontID]).</param>
    [EngineFunc(1, 2)]
    [Param("text?", ParamType.Any, "The text to measure its width.")]
    [Param("fontID", ParamType.Number, "The ID of the font to calculate on.", Optional = true)]
    IValue GetTextWidth(Exp.Instance? _, IValue?[] args) => ((double)GetTextSize(args[0]?.ToString(), args.Length >= 2 ? ((int?)args[1]?.Number) : null).width).ToExp();

    /// <summary>
    /// Returns the height of the specified text when drawing with the current / specified font.
    /// </summary>
    /// <returns>The height of the specified text when drawing with the current / specified font.</returns>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">(text?, [fontID]).</param>
    [EngineFunc(1, 2)]
    [Param("text?", ParamType.Any, "The text to measure its height.")]
    [Param("fontID", ParamType.Number, "The ID of the font to calculate on.", Optional = true)]
    IValue GetTextHeight(Exp.Instance? _, IValue?[] args) => ((double)GetTextSize(args[0]?.ToString(), args.Length >= 2 ? ((int?)args[1]?.Number) : null).height).ToExp();

    (float width, float height) GetTextSize(string? text, int? fontId);

    /// <summary>
    /// Sets the current font for subsequent text drawing operations.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[fontId].</param>
    [EngineFunc(1)]
    [Param("fontID", ParamType.Number, "The ID of the font to set to. Use 'Fonts' enum for that, e.g. Fonts.FntTitle.")]
    Exp.Void SetFont(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Sets the current drawing color used for primitives and text.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[abgrColor]. You can use the <see cref="ExpSrc.Drawing.Color"/> enum for this.</param>
    [EngineFunc(1)]
    [Param("color", ParamType.Number, $"The color to set to, in ABGR format. You can use {nameof(ExpSrc.Drawing.Color)} enum for that.")]
    Exp.Void SetColor(Exp.Instance? _, IValue?[] args);


    /// <summary>
    /// Returns the numeric value of the given color in ABGR format.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[r, g, b, a?]. Each channel is 0–255. Alpha defaults to 255 (fully opaque) if omitted.</param>
    [EngineFunc(3, 4)]
    [Param("r", ParamType.Number, "The red value.")]
    [Param("g", ParamType.Number, "The green value.")]
    [Param("b", ParamType.Number, "The blue value.")]
    [Param("a", ParamType.Number, "The alpha value.", Optional = true)]
    public IValue ColorFromRGB(Exp.Instance? _, IValue?[] args)
    {
        byte r = (byte)args[0].ThrowIfNull().Number;
        byte g = (byte)args[1].ThrowIfNull().Number;
        byte b = (byte)args[2].ThrowIfNull().Number;
        byte a = args.Length >= 4
            ? (byte)args[3].ThrowIfNull().Number
            : (byte)255;

        // Premultiply RGB by alpha
        var alphaFactor = a / 255.0;
        byte pr = (byte)System.Math.Round(r * alphaFactor);
        byte pg = (byte)System.Math.Round(g * alphaFactor);
        byte pb = (byte)System.Math.Round(b * alphaFactor);

        uint abgrValue =
            ((uint)a << 24) |
            ((uint)pb << 16) |
            ((uint)pg << 8) |
            pr;

        return ((double)abgrValue).ToExp();
    }

    /// <summary>
    /// Draws a line between two points.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">(x1, y1, x2, y2, [lineThickness]).</param>
    [EngineFunc(4, 5)]
    [Param("x1", ParamType.Number, "Position 1 x.")]
    [Param("y1", ParamType.Number, "Position 1 y.")]
    [Param("x2", ParamType.Number, "Position 2 x.")]
    [Param("y2", ParamType.Number, "Position 2 y.")]
    [Param("thickness", ParamType.Number, "Line thickness.", Optional = true)]
    Exp.Void DrawLine(Exp.Instance? _, IValue?[] args)
    {
        double x1 = args[0].ThrowIfNull().Number;
        double y1 = args[1].ThrowIfNull().Number;
        double x2 = args[2].ThrowIfNull().Number;
        double y2 = args[3].ThrowIfNull().Number;
        double thickness = args.Length >= 5 ? args[4].ThrowIfNull().Number : 1;

        DrawLine(x1, y1, x2, y2, thickness);

        return Exp.Void.Return;
    }

    /// <summary>
    /// Draws a rectangle.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">(x1, y1, x2, y2, [outline]).</param>
    [EngineFunc(4, 5, 6)]
    [Param("x1", ParamType.Number, "Position 1 x.")]
    [Param("y1", ParamType.Number, "Position 1 y.")]
    [Param("x2", ParamType.Number, "Position 2 x.")]
    [Param("y2", ParamType.Number, "Position 2 y.")]
    [Param("outline", ParamType.Bool, "Whether to fill a rectangle or only draw the outline.", Optional = true)]
    [Param("thickness", ParamType.Number, "Outline thickness.", Optional = true)]
    Exp.Void DrawRect(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Draws an ellipse.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">(x1, y1, x2, y2, [outline]).</param>
    [EngineFunc(4, 5)]
    [Param("x1", ParamType.Number, "Position 1 x.")]
    [Param("y1", ParamType.Number, "Position 1 y.")]
    [Param("x2", ParamType.Number, "Position 2 x.")]
    [Param("y2", ParamType.Number, "Position 2 y.")]
    [Param("outline", ParamType.Bool, "Whether to fill an ellipse or only draw the outline.", Optional = true)]
    [Param("thickness", ParamType.Number, "Outline thickness.", Optional = true)]
    Exp.Void DrawEllipse(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Draws a path starting at the path's start position or an optional provided position.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[pathId, [x, y], lineThickness].</param>
    [EngineFunc(2, 4)]
    [Param("pathID", ParamType.Number, "The ID of the path to draw. Use 'Paths' enum for that.")]
    [Param("x", ParamType.Number, "Position x.")]
    [Param("y", ParamType.Number, "Position x.")]
    [Param("lineThickness", ParamType.Number, "Line thickness.")]
    Exp.Void DrawPath(Exp.Instance? _, IValue?[] args)
    {
        // get parameters
        Resources.Path path = Paths.GetById((int)args[0].ThrowIfNull().Number);

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
    [EngineFunc(1, 2, 3, 4, 5)]
    [Param("soundID", ParamType.Number, "The ID of the sound to play. Use 'Sounds' enum for that, e.g. Sounds.SndShooting.")]
    [Param("looping", ParamType.Number, "Indicates whether the playback should automatically restart after it's finished.", Optional = true)]
    [Param("volume", ParamType.Number, "The volume to play in.", Optional = true)]
    [Param("pan", ParamType.Number, "The pan to play in.", Optional = true)]
    [Param("pitch", ParamType.Number, "The pitch to play in.", Optional = true)]
    Exp.Instance? PlaySound(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Pauses a currently playing sound instance.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[soundId].</param>
    [EngineFunc(1)]
    [Param("soundID", ParamType.Number, "The ID of the sound to pause. Use 'Sounds' enum for that, e.g. Sounds.SndShooting.")]
    Exp.Void PauseSound(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Pauses all currently playing sounds.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    [EngineFunc]
    Exp.Void PauseAllSounds(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Resumes a paused sound instance.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">[soundId].</param>
    [EngineFunc(1)]
    [Param("soundID", ParamType.Number, "The ID of the sound to resume. Use 'Sounds' enum for that, e.g. Sounds.SndShooting.")]
    Exp.Void ResumeSound(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Resumes all paused sounds.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    [EngineFunc]
    Exp.Void ResumeAllSounds(Exp.Instance? _, IValue?[] args);

    /// <summary>
    /// Starts the instance following a path resource.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] is the path ID, args[1] is the movement speed, args[2] is the PathEndAction and args[3] is a boolean indicating if the instance should be moved to the path start before.</param>
    [EngineFunc(4, IsNonStaticFuncOfGameObjects = true)]
    [Param("pathID", ParamType.Number, "The ID of the path to follow. Use 'Paths' enum for that, e.g. Paths.PthRoad.")]
    [Param("speed", ParamType.Number, "Movement speed.")]
    [Param("endAction", ParamType.PathEndAction, "What should happen when the path is completed.")]
    [Param("absolute", ParamType.Bool, "Indicates wether the object should be taken to the path start position before the beginning, or start it from its current position.")]
    Exp.Void StartPath(Exp.Instance? expinst, IValue?[] args)
    {
        Runtime.Instance inst = (Runtime.Instance)expinst!;

        // get path
        int id = (int)args[0].ThrowIfNull().Number;
        Resources.Path path = Paths.GetById(id);

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
    [EngineFunc(IsNonStaticFuncOfGameObjects = true)]
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
    [EngineFunc(1, IsNonStaticFuncOfGameObjects = true)]
    [Param("index", ParamType.Number, "The alarm index to retrieve.")]
    IValue GetAlarm(Exp.Instance? expinst, IValue?[] args)
    {
        return ((Runtime.Instance)expinst!).Alarm[(int)args[0].ThrowIfNull().Number].number.ToExp();
    }

    /// <summary>
    /// Sets the value of the specified alarm for the given instance.
    /// </summary>
    /// <param name="expinst">The calling runtime instance.</param>
    /// <param name="args">Arguments where args[0] is the alarm index and args[1] is the value to set.</param>
    [EngineFunc(2, IsNonStaticFuncOfGameObjects = true)]
    [Param("index", ParamType.Number, "The alarm index to set.")]
    [Param("value", ParamType.Number, "The value to set to.")]
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
    [EngineFunc]
    IValue GetRoomWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Width.ToExp();

    /// <summary>
    /// Gets the active room height.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Unused.</param>
    /// <returns>The height of the active room as a <see cref="NumberValue"/>.</returns>
    [EngineFunc]
    IValue GetRoomHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Height.ToExp();

    /// <summary>
    /// Gets the X position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view X position as a <see cref="NumberValue"/>.</returns>
    [Param("index", ParamType.Number, "The index of view to get its x.")]
    [EngineFunc(1)]
    IValue GetViewX(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].X.ToExp();

    /// <summary>
    /// Gets the Y position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view Y position as a <see cref="NumberValue"/>.</returns>
    [EngineFunc(1)]
    [Param("index", ParamType.Number, "The index of view to get its y.")]
    IValue GetViewY(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Y.ToExp();

    /// <summary>
    /// Gets the width of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view width as a <see cref="NumberValue"/>.</returns>
    [EngineFunc(1)]
    [Param("index", ParamType.Number, "The index of view to get its width.")]
    IValue GetViewWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Width.ToExp();

    /// <summary>
    /// Gets the height of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view height a <see cref="NumberValue"/>.</returns>
    [EngineFunc(1)]
    [Param("index", ParamType.Number, "The index of view to get its height.")]
    IValue GetViewHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].Height.ToExp();

    /// <summary>
    /// Gets the port X position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port X position as a <see cref="NumberValue"/>.</returns>
    [EngineFunc(1)]
    [Param("index", ParamType.Number, "The index of view to get its port x.")]
    IValue GetViewPortX(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortX.ToExp();

    /// <summary>
    /// Sets the specific instance that the given view should follow.
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(viewIndex, instance?)</param>
    /// <returns>Void.</returns>
    /// <exception cref="Exceptions.EngineException"></exception>
    [EngineFunc(2)]
    [Param("viewIndex", ParamType.Number, "The index of view to set its following target.")]
    [Param("instance?", ParamType.GameObject, "The target instance to follow.")]
    IValue SetViewFollowingTarget(Exp.Instance? _, IValue?[] args)
    {
        // get target argument
        Runtime.Instance? target;
        if (args[1] == null)
            target = null;
        else
        {
            if (args[1] is not { Inst: Runtime.Instance runtimeInst })
                throw new Exceptions.EngineException("An instance of a game object was expected.");
            else
                target = runtimeInst;
        }

        // get view
        RoomView view = GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number];

        // set view's target
        view.SpecificInstanceToFollow = target;

        return Exp.Void.Return;
    }

    /// <summary>
    /// Gets the port Y position of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port Y position as a <see cref="NumberValue"/>.</returns>
    [EngineFunc(1)]
    [Param("index", ParamType.Number, "The index of view to get its port y.")]
    IValue GetViewPortY(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortY.ToExp();

    /// <summary>
    /// Gets the port width of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port width as a <see cref="NumberValue"/>.</returns>
    [EngineFunc(1)]
    [Param("index", ParamType.Number, "The index of view to get its port width.")]
    IValue GetViewPortWidth(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortWidth.ToExp();

    /// <summary>
    /// Gets the port height of the specified view in the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the view index.</param>
    /// <returns>The view port height as a <see cref="NumberValue"/>.</returns>
    [EngineFunc(1)]
    [Param("index", ParamType.Number, "The index of view to get its port height.")]
    IValue GetViewPortHeight(Exp.Instance? _, IValue?[] args) => GetActivatedRoom().Model.Views[(int)args[0].ThrowIfNull().Number].PortHeight.ToExp();

    /// <summary>
    /// Gets the ID of the resource (sprite, path, etc.) with the given name.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] is the resource name.</param>
    /// <returns>The ID of the resource with the given name, or null if there's no resource with this name.</returns>
    [EngineFunc(1)]
    [Param("name", ParamType.String, "The name of the resource to find its ID.")]
    IValue? GetResByName(Exp.Instance? _, IValue?[] args)
    {
        string name = args[0].ThrowIfNull().ToString()!;
        IEnumerable<ISetsID>[] resLists = [Sprites, Sounds, Paths, Rooms];

        foreach (var ls in resLists)
        {
            if (ls.FirstOrDefault(res => res.Name == name) is { } res)
                return res.ID.ToExp();
        }

        return null;
    }

    /// <summary>
    /// Gets the index of the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">(unused).</param>
    /// <returns>The index of the currently activated room as a <see cref="NumberValue"/>.</returns>
    [EngineFunc]
    IValue GetRoomIndex(Exp.Instance? _, IValue?[] args) => Rooms.IndexOf(GetActivatedRoom().Model).ToExp();
}
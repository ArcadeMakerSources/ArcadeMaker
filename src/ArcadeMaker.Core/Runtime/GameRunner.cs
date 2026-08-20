using System;
using System.Collections.Generic;
using System.Text;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Exceptions;
using ArcadeMaker.Core.Runtime;
using Exp;
using Exp.Spans;
using System.Reflection;
using ArcadeMaker.Core.ExpSrc;
using ArcadeMaker.Core.Math;
using Exp.Converting;

namespace ArcadeMaker.Core.Runtime;

public sealed class GameRunner<TGame> where TGame : IGame // we COULD use a non-generic class, but this approach allows the JIT to optimize the code by skipping the vtable lookup for the IGame interface, which is a bit faster. it's also important to mark the classes that implement IGame as 'sealed' (not sure if this comment is actually true...).
{
    public TGame Game { get; }
    public Interpreter Interpreter { get; }

    public GameRunner(TGame game, bool removeEmptyEvents = true)
    {
        ArgumentNullException.ThrowIfNull(game);

        this.Game = game;
        game.Scripts.AddRange(ExpSrc.ExpSrc.GetScripts());
        Interpreter = new();

        ExpSrc.ExpSrc.AddFuncsToInterpreter(this);
        ExpSrc.ExpSrc.AddClassesToInterpreter(Interpreter);

        // build
        ExpError[]? eventsErrors = null;
        Interpreter.Build(ScriptDocument.FromString("", "main.script"), game.Objects.Map(model => model.Class), game.Scripts.ToArray());
        if (removeEmptyEvents)
            game.Objects.ForEach(obj => obj.Events.ForEach(ev => ev.Docs!.ForEach(doc => doc.TryPrepare(Interpreter, out eventsErrors))));
        if (eventsErrors?.Length >= 1)
        {
            // TODO: do something...
        }

        ExpSrc.ExpSrc.CreatePropertiesInitializers(this);

        if (removeEmptyEvents)
        {
            foreach (var model in Game.Objects)
            {
                model.RemoveEmptyEvents();
            }
        }
    }

    public void FireStep()
    {
        if (Game.CurrentRoom == null)
            return;

        // run all step events for the current room
        var roominsts = Game.CurrentRoom.Instances;
        //var node = roominsts.First;
        for (int i = 0; i < roominsts.Count; i++) // if we use foreach here, modifications to the list of instances (like destroying an instance and removing it from the list) would cause an exception, but with this for loop it won't
        {
            var instance = roominsts[i];
            //node = node.Next;

            // run other events that belong to step (like KeyDown)
            // run KeyDown events
            foreach (var keyDownEv in instance.Model.KeyDownEvents)
            {
                if (Game.KeyDown(null, [((int)keyDownEv.Param).ToExp()]).Bool)
                {
                    foreach (var script in keyDownEv.Docs)
                        script.Run(Interpreter, instance);
                }
            }

            // run KeyPress events
            foreach (var ev in instance.Model.KeyPressEvents)
            {
                if (Game.KeyPress(null, [((int)ev.Param).ToExp()]).Bool)
                {
                    foreach (var script in ev.Docs)
                        script.Run(Interpreter, instance);
                }
            }

            // run KeyRelease events
            foreach (var ev in instance.Model.KeyReleaseEvents)
            {
                if (Game.KeyRelease(null, [((int)ev.Param).ToExp()]).Bool)
                {
                    foreach (var script in ev.Docs)
                        script.Run(Interpreter, instance);
                }
            }

            // run MouseDown events
            foreach (var ev in instance.Model.MouseDownEvents)
            {
                // also check collision with mouse
                if (Game.MouseButtonDown(null, [((int)ev.Param).ToExp()]).Bool && Game.PointMeeting(instance, [Game.GetMouseX(null, []), Game.GetMouseY(null, [])]).Bool)
                {
                    foreach (var script in ev.Docs)
                        script.Run(Interpreter, instance);
                }
            }

            // run MousePress events
            foreach (var ev in instance.Model.MousePressEvents)
            {
                // also check collision with mouse
                if (Game.MouseButtonPress(null, [((int)ev.Param).ToExp()]).Bool && Game.PointMeeting(instance, [Game.GetMouseX(null, []), Game.GetMouseY(null, [])]).Bool)
                {
                    foreach (var script in ev.Docs)
                        script.Run(Interpreter, instance);
                }
            }

            // run MouseRelease events
            foreach (var ev in instance.Model.MouseReleaseEvents)
            {
                // also check collision with mouse
                if (Game.MouseButtonRelease(null, [((int)ev.Param).ToExp()]).Bool && Game.PointMeeting(instance, [Game.GetMouseX(null, []), Game.GetMouseY(null, [])]).Bool)
                {
                    foreach (var script in ev.Docs)
                        script.Run(Interpreter, instance);
                }
            }

            // run collision events
            foreach (var ev in instance.Model.CollisionEvents)
            {
                Runtime.Instance? other = Game.InstanceMeeting(instance, [instance.X.Value, instance.Y.Value, ev.Model.Class.ExpType]);
                if (other != null)
                {
                    foreach (var script in ev.Docs)
                        script.Run(Interpreter, instance, other);
                }
            }

            // tick alarms
            instance.TickAlarms(Interpreter);

            // run outside room events
            if (instance.Model.OutsideRoomEvent != null)
            {
                if (Game.OutsideRoom(instance, []).Bool)
                {
                    foreach (var script in instance.Model.OutsideRoomEvent.Docs)
                        script.Run(Interpreter, instance);
                }
            }

            if (instance.Model.StepEvent != null)
                foreach (var script in instance.Model.StepEvent.Docs)
                    script.Run(Interpreter, instance);

            // move path
            if (instance.CurrentPathDrive != null)
            {
                if (!instance.CurrentPathDrive.Move(out double hsp, out double vsp, out bool updated))
                    instance.CurrentPathDrive = null;
                else if (updated)
                {
                    instance.Hspeed.Value = hsp.ToExp();
                    instance.Vspeed.Value = vsp.ToExp();

                    // call onPathStepFinished
                    if (instance.CurrentPathDrive.PathStepIndex > 1 && instance.OnPathStepFinished.Value is FuncPntr fn)
                        fn.Call(Interpreter, [instance.CurrentPathDrive.Path.ID.ToExp()]);
                }
            }

            instance.X.Value = (instance.X.Value!.Number + (instance.hspeed ?? 0)).ToExp(); // add hspeed to x
            instance.Y.Value = (instance.Y.Value!.Number + (instance.vspeed ?? 0)).ToExp(); // add vspeed to y
            UpdateImageIndex(instance);
        }

        // move following views
        foreach (var view in Game.CurrentRoom.Model.Views)
        {
            if (!view.Visible || view.Following == null)
                continue;

            Instance? inst = view.SpecificInstanceToFollow;
            if (inst is not { WasDestroyed: false })
                inst = view.SpecificInstanceToFollow = roominsts.FirstOrDefault(i => i.Model == view.Following);
            if (inst != null)
            {
                double targetX = inst.X.Value!.Number - view.Follow_HBorder, targetY = inst.Y.Value!.Number - view.Follow_VBorder;
                if (view.X != targetX || view.Y != targetY)
                {
                    if (view.Follow_HSpeed > 0)
                        targetX += System.Math.Sign(targetX - view.X) * System.Math.Min(view.Follow_HSpeed, System.Math.Abs(targetX - view.X));
                    if (view.Follow_VSpeed > 0)
                        targetY += System.Math.Sign(targetY - view.Y) * System.Math.Min(view.Follow_VSpeed, System.Math.Abs(targetY - view.Y));
                    
                    // don't let the following view get out of the room
                    if (targetX < 0)
                        targetX = 0;
                    else if (targetX + view.Width > Game.CurrentRoom.Model.Width)
                        targetX = Game.CurrentRoom.Model.Width - view.Width;
                    if (targetY < 0)
                        targetY = 0;
                    else if (targetY + view.Height > Game.CurrentRoom.Model.Height)
                        targetY = Game.CurrentRoom.Model.Height - view.Height;

                    view.SetPosition(targetX, targetY);
                }
            }
        }

        // scroll backgrounds
        foreach (var bg in Game.CurrentRoom.Backgrounds)
        {
            bg.X += bg.HorSpeed;
            bg.Y += bg.VerSpeed;
        }
    }

    private static void UpdateImageIndex(Instance instance)
    {
        if (instance.Sprite != null && instance.ImageSpeed.Value!.Number > 0 && instance.Sprite.NumberOfImages >= 2 && ++instance.FramesSinceLastImageIndex >= instance.ImageSpeed.Value?.Number)
        {
            instance.FramesSinceLastImageIndex = 0;
            double nextIndex = instance.ImageIndex.Value!.Number + 1 >= instance.Sprite.NumberOfImages ? 0 : instance.ImageIndex.Value.Number + 1;
            instance.ImageIndex.Value = nextIndex.ToExp();
        }
    }

    public void RunDrawEvent(Runtime.Instance instance)
    {
        // this must be called after validating that instance.OverridesDrawEvent is true
        foreach (var script in instance.Model.DrawEvent!.Docs)
            script.Run(Interpreter, instance, Game.CurrentViewIndex.ToExp());
    }

    /// <summary>
    /// Draws an instance of a game object.
    /// </summary>
    /// <param name="inst">The instance to draw.</param>
    /// <param name="args">[].</param>
    /// <returns>void.</returns>
    [EngineFunc(CustomName = "drawSelf", IsNonStaticFuncOfGameObjects = true)]
    public Exp.Void DrawInstance(Exp.Instance? inst, IValue?[] args)
    {
        Game.DrawInstance((Runtime.Instance)inst!);
        return Exp.Void.Return;
    }

    /// <summary>
    /// Destroys an instance of a game object.
    /// </summary>
    /// <param name="expinst">The instance to destroy.</param>
    /// <param name="args">[].</param>
    /// <returns>void.</returns>
    [EngineFunc(IsNonStaticFuncOfGameObjects = true)]
    public Exp.Void Destroy(Exp.Instance? expinst, IValue?[] args)
    {
        var inst = (Runtime.Instance)expinst!;

        inst.Destroy(Interpreter);

        return Exp.Void.Return;
    }

    /// <summary>
    /// Creates a new instance of the specified object type at the given coordinates and adds it to the active room.
    /// </summary>
    /// <param name="_">The calling EXP instance (unused).</param>
    /// <param name="args">Arguments where args[0] and args[1] are the spawn X and Y coordinates and args[2] is the object type to instantiate.</param>
    /// <returns>The newly created runtime instance.</returns>
    [EngineFunc(3)]
    [Param("x", ParamType.Number, "The x position to create the new instance at.")]
    [Param("y", ParamType.Number, "The y position to create the new instance at.")]
    [Param("type", ParamType.Type, "The type of object to create.")]
    public Runtime.Instance CreateInstance(Exp.Instance? _, IValue?[] args)
    {
        ObjectModel model = Game.Objects.FirstOrDefault(m => m.Class.ExpType == args[2].ThrowIfNull()) ?? throw new ArgumentException("Value of argument type must be a type of a game object.");
        Runtime.Instance inst = new(Game, model);
        inst.X.Value = args[0];
        inst.Y.Value = args[1];
        Game.GetActivatedRoom().AddInstance(inst);

        // run create event
        inst.FireCreateEvent(Interpreter);

        return inst;
    }

    public void Run(bool invokeInit = true)
    {
        // initialize the game
        try
        {
            if (invokeInit)
                Game.Init();
        }
        catch (Exception ex)
        {
            throw new LoadingException("An error occurred while loading the game.", ex);
        }

        // resolve collision events
        foreach (var obj in Game.Objects)
        {
            foreach (var collisionEv in obj.CollisionEvents)
                collisionEv.Resolve(Game);
        }

        // init interpreter
        Interpreter.Init();

        // make sure game has any rooms
        if (Game.Rooms.Count == 0)
            throw new Exception("A game must have at least 1 room.");

        // go to the first room in the list
        GoToRoom(Game.Rooms[0]);
    }

    public void GoToRoom(RoomInstance room)
    {
        if (!Game.Rooms.Contains(room.Model))
            throw new Exception("The specified room is not part of the game.");

        // if there's an existing room ( =it's not the beginning of the game), destroy all instances
        if (Game.CurrentRoom != null)
        {
            while (Game.CurrentRoom.Instances.Count >= 1)
            {
                Destroy(Game.CurrentRoom.Instances[0], []);
            }
        }

        Game.CurrentRoom = room;
        Game.SetCaption(room.Model.Caption);
        Game.BackColor = room.Model.BackgroundColor;

        // set window size: room size or the minimum region that would contain all visible views ports, when views enabled
        int winWidth = room.Model.Width, winHeight = room.Model.Height;
        var visibleViews = room.Model.Views.Where(v => v.Visible);
        if (visibleViews.Any())
        {
            winWidth = 1;
            winHeight = 1;

            foreach (var view in visibleViews)
            {
                winWidth  = System.Math.Max(winWidth,  view.PortX + view.PortWidth );
                winHeight = System.Math.Max(winHeight, view.PortY + view.PortHeight);
            }
        }
        Game.SetWindowsSize(winWidth, winHeight);

        // run all create events for the new room
        foreach (var instance in room.Instances.ToArray()) // ToArray() to prevent collection modification while iterating
        {
            instance.FireCreateEvent(Interpreter);
        }
    }

    public void GoToRoom(RoomModel roomModel)
    {
        if (!Game.Rooms.Contains(roomModel))
            throw new Exception("The specified room is not part of the game.");

        var roomInstance = new RoomInstance(Game, roomModel);
        GoToRoom(roomInstance);
    }

    /// <summary>
    /// Goes to another room and destroys the objects in the current one.
    /// </summary>
    /// <param name="_">Unused.</param>
    /// <param name="args">[roomId].</param>
    /// <returns>void.</returns>
    /// <exception cref="ArgumentException"></exception>
    [EngineFunc(1)]
    [Param("roomID", ParamType.Number, "The ID of the room to go to. You can use 'Rooms.my_room' for this.")]
    public Exp.Void GoToRoom(Exp.Instance? _, IValue?[] args)
    {
        // find the room by the ID
        int ID = (int)args[0].ThrowIfNull().Number;
        RoomModel model = Game.Rooms.GetById(ID);

        // go to it
        GoToRoom(model);

        return Exp.Void.Return;
    }

    /// <summary>
    /// Goes to next room.
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(Unused).</param>
    /// <returns></returns>
    /// <exception cref="NoActivatedRoomException"></exception>
    [EngineFunc]
    public Exp.Void GoToNextRoom(Exp.Instance? _, IValue?[] args)
    {
        if (Game.CurrentRoom == null)
            throw new NoActivatedRoomException();

        int currentIndex = Game.Rooms.IndexOf(Game.CurrentRoom.Model);

        if (currentIndex + 1 >= Game.Rooms.Count)
            this.Interpreter.ThrowRuntime("Current room is the last room.", RuntimeException.INVALID_OPERATION);
        else
            GoToRoom(Game.Rooms[currentIndex + 1]);

        return Exp.Void.Return;
    }

    /// <summary>
    /// Destroys all instances in the room, and then restarts it.
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(Unused).</param>
    /// <returns></returns>
    [EngineFunc]
    public Exp.Void RestartRoom(Exp.Instance? _, IValue?[] args)
    {
        GoToRoom(Game.GetActivatedRoom().Model);
        return Exp.Void.Return;
    }
}
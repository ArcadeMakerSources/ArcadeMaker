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

namespace ArcadeMaker.Core.Runtime
{
    public class GameRunner
    {
        public IGame Game { get; }
        public Interpreter Interpreter { get; }

        public GameRunner(IGame game)
        {
            ArgumentNullException.ThrowIfNull(game);

            this.Game = game;
            game.Scripts.AddRange(GetScripts());
            Interpreter = new();

            AddFuncsToInterpreter();

            Interpreter.Build(ScriptDocument.FromString("", "main.script"), game.Objects.Map(model => model.Class), game.Scripts.ToArray());

            CreatePropertiesInitializers();
        }

        private void CreatePropertiesInitializers()
        {
            List<ExpError> InitializersErrors = [];

            foreach (ObjectModel model in Game.Objects)
            {
                // create a script that initializes the property for an instance
                string initializerScript = "";

                foreach (var extrap in model.ExtraProperties)
                {
                    initializerScript += $"{extrap.Name} = {extrap.InitValueCode} /* */\n"; // add /* */ to prevent issues with properties with code that ends with a comment
                }

                // add the initializer script to the create event of the object
                InstanceScriptDocument initializerDoc = new(model.Name + ".PropertiesInitializer", model.Class, initializerScript);
                initializerDoc.TryPrepare(Interpreter, out var errors);
                InitializersErrors.AddRange(errors);
                model.EventScripts.Create.Insert(0, initializerDoc);
            }

            if (InitializersErrors.Count > 0)
                throw new BuildFailureException(InitializersErrors);
        }

        public static IEnumerable<ScriptDocument> GetScripts()
        {
            List<ScriptDocument> docs = [];

            // get the types of all the enums that should be copied to the interpreter
            List<Type> enums = [];
            enums.AddRange(ExpSrc.ExpSrc.GetEnums());

            // convert them to Exp code
            docs.AddRange(enums.Select(e => ScriptDocument.FromString(GetEnumCode(e), e.Name + ".exp")));

            return docs;

            static string GetEnumCode(Type type) // converts an enum type to a string of Exp code that defines the same enum
            {
                string code = $"namespace {ExpSrc.ExpSrc.EngineNamespace}:\n\nenum {type.Name}\n{{\n";

                string[] names = Enum.GetNames(type);
                int i = 0;
                foreach (var name in names)
                {
                    code += $"    {name.StartWithLowerCase()} = {(int)Enum.Parse(type, name)}{(i++ < names.Length - 1 ? "," : "")}\n";
                }

                code += "}";
                return code;
            }
        }

        public void AddFuncsToInterpreter()
        {
            List<ExternFunc> funcs = [];
            var objClasses = Game.Objects.Map(model => model.Class);

            // static classes to add
            ClassDefSpan instanceStaticClass = new("Instance", [], []) { Namespace = ExpSrc.ExpSrc.EngineNamespace };
            instanceStaticClass.Funcs = instanceStaticClass.Funcs.Append(new ConstructorDefSpan([], [], instanceStaticClass, Interpreter) { Private = true }).ToArray();
            ClassDefSpan soundsStaticClass = new("Sounds", [], []) { Namespace = ExpSrc.ExpSrc.EngineNamespace };
            soundsStaticClass.Funcs = instanceStaticClass.Funcs.Append(new ConstructorDefSpan([], [], soundsStaticClass, Interpreter) { Private = true }).ToArray();
            soundsStaticClass.Vars.AddRange(Game.Sounds.Map(s => new Variable(s.Name, s.ID.ToExp(), cons: true)));
            ClassDefSpan pathsStaticClass = new("Paths", [], []) { Namespace = ExpSrc.ExpSrc.EngineNamespace };
            pathsStaticClass.Funcs = instanceStaticClass.Funcs.Append(new ConstructorDefSpan([], [], pathsStaticClass, Interpreter) { Private = true }).ToArray();
            pathsStaticClass.Vars.AddRange(Game.Paths.Map(p => new Variable(p.Name, p.ID.ToExp(), cons: true)));
            ClassDefSpan roomsStaticClass = new("Rooms", [], []) { Namespace = ExpSrc.ExpSrc.EngineNamespace };
            roomsStaticClass.Funcs = roomsStaticClass.Funcs.Append(new ConstructorDefSpan([], [], roomsStaticClass, Interpreter) { Private = true }).ToArray();
            roomsStaticClass.Vars.AddRange(Game.Rooms.Map(r => new Variable(r.Name, r.ID.ToExp(), cons: true)));
            Interpreter.definations.AddRange([instanceStaticClass, soundsStaticClass, pathsStaticClass, roomsStaticClass, SoundPlaybackInstance.Class]);

            // add all methods with [ExpFunc] attribute
            void AddMarkedFuncs(object instance, Type? type = null)
            {
                foreach (var methodInfo in (type ?? instance.GetType()).GetMethods())
                {
                    var attr = methodInfo.GetCustomAttribute<ExpFuncAttribute>();
                    if (attr != null)
                    {
                        // create Func<...> from methodInfo
                        var invoker = (Func<Exp.Instance?, IValue?[], IValue?>)Delegate.CreateDelegate(typeof(Func<Exp.Instance?, IValue?[], IValue?>), instance, methodInfo);
                        string invokerName = attr.CustomName ?? methodInfo.Name.StartWithLowerCase();

                        // add to interpreter
                        if (attr.IsNonStaticFuncOfGameObjects)
                            objClasses.ForEach(objCls => Interpreter.AddExternFunc(new(invoker, attr.ParamsCounts, invokerName), objCls));
                        else
                            Interpreter.AddExternFunc(new(invoker, attr.ParamsCounts, invokerName, ExpSrc.ExpSrc.GameNamespace));
                    }
                }
            }
            AddMarkedFuncs(Game, typeof(IGame));
            AddMarkedFuncs(this);

            // manually add non-static functions that cannot be marked with [ExpFunc]
            Interpreter.AddExternFunc(new(Game.PauseSound, 0, "pause"), SoundPlaybackInstance.Class);
            Interpreter.AddExternFunc(new(Game.ResumeSound, 0, "resume"), SoundPlaybackInstance.Class);

            // add "all()" functions and "i" / "first" getters
            foreach (var cls in objClasses)
            {
                IValue? All(Exp.Instance? _, IValue?[] args)
                {
                    if (Game.CurrentRoom == null)
                        return new Exp.Instance(ClassDefSpan.ExpArrayDef, []);

                    List<Exp.Instance> all = [];

                    foreach (var inst in Game.CurrentRoom.Instances)
                    {
                        if (inst.Model.Class == cls)
                            all.Add(inst);
                    }

                    return new Exp.Instance(ClassDefSpan.ExpArrayDef, [.. all]);
                }

                IValue? GetSingleInst()
                {
                    if (Game.CurrentRoom == null)
                        return null;

                    Instance? i = null;

                    foreach (var inst in Game.CurrentRoom.Instances)
                    {
                        if (inst.Model.Class == cls)
                        {
                            if (i == null)
                                i = inst;
                            else
                                Interpreter.ThrowRuntime($"There is more than 1 instance of object '{cls.Name}' in current room.", RuntimeException.INVALID_OPERATION);
                        }
                    }

                    return i;
                }

                Interpreter.AddExternFunc(new(All, 0, "all" /* name must be specified in local functions! */), cls, true);
                cls.Vars.Add(new CustomVariable("i", GetSingleInst, null, false));
                cls.Vars.Add(new CustomVariable("first", () => Game.CurrentRoom?.Instances.FirstOrDefault(i => i.Model.Class == cls), null, false));
            }
        }

        public void FireStep()
        {
            if (Game.CurrentRoom == null)
                return;

            // run all step events for the current room
            var roominsts = Game.CurrentRoom.Instances;
            for (int i = 0; i < roominsts.Count; i++) // if we use foreach here, modifications to the list of instances (like destroying an instance and removing it from the list) will cause an exception, but with this for loop it won't
            {
                var instance = roominsts[i];
                if (instance.Model.EventScripts.Step != null)
                    foreach (var script in instance.Model.EventScripts.Step)
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

                Instance? inst = roominsts.FirstOrDefault(i => i.Model == view.Following);
                if (inst != null)
                {
                    double targetX = inst.X.Value!.Number - view.Follow_HBorder, targetY = inst.Y.Value!.Number - view.Follow_VBorder;
                    if (view.X != targetX || view.Y != targetY)
                    {
                        if (view.Follow_HSpeed > 0)
                            targetX += System.Math.Sign(targetX - view.X) * System.Math.Min(view.Follow_HSpeed, System.Math.Abs(targetX - view.X));
                        if (view.Follow_VSpeed > 0)
                            targetY += System.Math.Sign(targetY - view.Y) * System.Math.Min(view.Follow_VSpeed, System.Math.Abs(targetY - view.Y));
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
            if (instance.Model.Sprite != null && instance.ImageSpeed.Value!.Number > 0 && instance.Model.Sprite.NumberOfImages >= 2 && ++instance.FramesSinceLastImageIndex >= instance.ImageSpeed.Value?.Number)
            {
                instance.FramesSinceLastImageIndex = 0;
                double nextIndex = instance.ImageIndex.Value!.Number + 1 >= instance.Model.Sprite.NumberOfImages ? 0 : instance.ImageIndex.Value.Number + 1;
                instance.ImageIndex.Value = nextIndex.ToExp();
            }
        }

        public void FireDraw()
        {
            // run all draw events for the current room
            foreach (var instance in Game.GetActivatedRoom().SortedInstances)
            {
                if (instance.Model.EventScripts.Draw?.Length >= 1)
                    foreach (var script in instance.Model.EventScripts.Draw)
                        script.Run(Interpreter, instance, Game.CurrentViewIndex.ToExp());
                else
                    Game.DrawInstance(instance);
            }
        }

        [ExpFunc(CustomName = "drawSelf", IsNonStaticFuncOfGameObjects = true)]
        public Exp.Void DrawInstance(Exp.Instance? inst, IValue?[] args)
        {
            Game.DrawInstance((Runtime.Instance)inst!);
            return Exp.Void.Return;
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

            Game.CurrentRoom = room;
            Game.SetWindowsSize(room.Model.Width, room.Model.Height);
            Game.SetCaption(room.Model.Caption);

            // run all create events for the new room
            foreach (var instance in room.Instances)
            {
                foreach (var script in instance.Model.EventScripts.Create)
                    script.Run(Interpreter, instance);
            }
        }

        public void GoToRoom(RoomModel roomModel)
        {
            if (!Game.Rooms.Contains(roomModel))
                throw new Exception("The specified room is not part of the game.");

            var roomInstance = new RoomInstance(roomModel);
            GoToRoom(roomInstance);
        }

        [ExpFunc(1)]
        public Exp.Void GoToRoom(Exp.Instance? _, IValue?[] args)
        {
            // find the room by the ID
            double ID = args[0].ThrowIfNull().Number;
            RoomModel model = Game.Rooms.FirstOrDefault(r => ID == r.ID) ?? throw new ArgumentException($"There is no room with ID {ID}.");

            // go to it
            GoToRoom(model);

            return Exp.Void.Return;
        }

        [ExpFunc]
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

        [ExpFunc]
        public Exp.Void RestartRoom(Exp.Instance? _, IValue?[] args)
        {
            GoToRoom(Game.GetActivatedRoom().Model);
            return Exp.Void.Return;
        }
    }
}
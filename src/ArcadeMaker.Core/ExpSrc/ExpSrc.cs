using ArcadeMaker.Core.Exceptions;
using ArcadeMaker.Core.Math;
using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Runtime;
using Exp;
using Exp.Converting;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc
{
    public static class ExpSrc
    {
        public const string EngineNamespace = "ArcadeMaker";
        public const string GameNamespace = "game";

        public const string CURRENT_VIEW_INDEX_ARG_NAME = "currentViewIndex";

        public static HashSet<string> GlobalUsings { get; } = ["system", EngineNamespace];


        internal static void CreatePropertiesInitializers<T>(GameRunner<T> runner) where T : IGame
        {
            List<ExpError> InitializersErrors = [];

            foreach (ObjectModel model in runner.Game.Objects)
            {
                if (model.ExtraProperties.Length == 0)
                    continue;

                // create a script that initializes the property for an instance.
                // don't add new lines between and after usings, so that the line number of an error in the
                // property initializer will match the property index
                string initializerScript = $"using {Interpreter.STD_NAMESPACE} using {EngineNamespace} using {GameNamespace} ";

                foreach (var extrap in model.ExtraProperties)
                {
                    initializerScript += $"{extrap.Name} = {extrap.InitValueCode} /* */\n"; // add /* */ to prevent issues with properties with code that ends with a comment
                }

                // create the initializer script document
                InstanceScriptDocument initializerDoc = new(model.Name + ".PropertiesInitializer", model.Class, initializerScript);
                initializerDoc.TryPrepare(runner.Interpreter, out var errors);
                InitializersErrors.AddRange(errors);

                //// if create event is not set, create it (UPDATE: now we save it to ObjectModel.PropertiesInitializer)
                //var createEv = model.GetEvent(ObjectEvent.EventType.Create);
                //if (createEv == null)
                //{
                //    createEv = new(ObjectEvent.EventType.Create, []);
                //    createEv.CreateDocs(model.Class);
                //    model.Events.Add(createEv);
                //    model.CreateEvent = createEv;
                //}

                // save
                model.PropertiesInitializer = initializerDoc;
            }

            if (InitializersErrors.Count > 0)
                throw new BuildFailureException(InitializersErrors);
        }

        public static IEnumerable<ScriptDocument> GetScripts()
        {
            List<ScriptDocument> docs = [];

            // get the types of all the enums that should be copied to the interpreter
            List<Type> enums = [];
            enums.AddRange(GetEnums());

            // convert them to Exp code
            docs.AddRange(enums.Select(e => ScriptDocument.FromString(GetEnumCode(e), e.Name + ".exp")));

            return docs;

            static string GetEnumCode(Type type) // converts an enum type to a string of Exp code that defines the same enum
            {
                string code = $"namespace {EngineNamespace}:\n\nenum {type.Name}\n{{\n";

                string[] names = Enum.GetNames(type);
                int i = 0;
                foreach (var name in names)
                {
                    code += $"    {name.StartWithLowerCase()} = {(type.GetEnumUnderlyingType() == typeof(uint) ? (uint)Enum.Parse(type, name) : (int)Enum.Parse(type, name))}{(i++ < names.Length - 1 ? "," : "")}\n";
                }

                code += "}";
                return code;
            }
        }

        /// <summary>
        /// Adds all of the classes that implement <see cref="Exp.Converting.IConvertable"/> in this assembly.
        /// </summary>
        /// <exception cref="LoadingException"></exception>
        internal static void AddClassesToInterpreter(Interpreter interpreter)
        {
            // iterate on all classes in this assembly
            foreach (var cls in typeof(IGame).Assembly.GetTypes())
            {
                // if the class does not implement IConvertable, continue
                if (!cls.GetInterfaces().Contains(typeof(IConvertable)))
                    continue;

                // get MethodInfo for Convert.ToClass<cls>(...)
                MethodInfo converter = typeof(Exp.Converting.Convert).GetMethod(nameof(Exp.Converting.Convert.ToClass), genericParameterCount: 1, [typeof(Interpreter)])!;
                MethodInfo constructedConverter = converter.MakeGenericMethod(cls);

                // get the class
                ClassDefSpan classDef;
                try
                {
                    classDef = (ClassDefSpan)constructedConverter.Invoke(null, [interpreter])!;
                }
                catch (Exception ex)
                {
                    throw new LoadingException($"Could not convert class '{cls.Name}' to Exp class. An inner exception is attached.", ex);
                }
                interpreter.definations.Add(classDef);
            }
        }

        internal static void AddFuncsToInterpreter<T>(GameRunner<T> runner) where T : IGame
        {
            List<ExternFunc> funcs = [];
            var objClasses = runner.Game.Objects.Map(model => model.Class);

            // static classes to add
            ClassDefSpan instanceStaticClass = new("Instance", [], []) { Namespace = EngineNamespace };
            instanceStaticClass.Funcs = instanceStaticClass.Funcs.Append(new ConstructorDefSpan([], [], instanceStaticClass, runner.Interpreter) { Private = true }).ToArray();
            ClassDefSpan spritesStaticClass = new("Sprites", [], []) { Namespace = EngineNamespace };
            spritesStaticClass.Funcs = spritesStaticClass.Funcs.Append(new ConstructorDefSpan([], [], spritesStaticClass, runner.Interpreter) { Private = true }).ToArray();
            spritesStaticClass.Vars.AddRange(runner.Game.Sprites.Map(s => new Variable(s.Name, s.ID.ToExp(), cons: true)));
            ClassDefSpan soundsStaticClass = new("Sounds", [], []) { Namespace = EngineNamespace };
            soundsStaticClass.Funcs = instanceStaticClass.Funcs.Append(new ConstructorDefSpan([], [], soundsStaticClass, runner.Interpreter) { Private = true }).ToArray();
            soundsStaticClass.Vars.AddRange(runner.Game.Sounds.Map(s => new Variable(s.Name, s.ID.ToExp(), cons: true)));
            ClassDefSpan pathsStaticClass = new("Paths", [], []) { Namespace = EngineNamespace };
            pathsStaticClass.Funcs = instanceStaticClass.Funcs.Append(new ConstructorDefSpan([], [], pathsStaticClass, runner.Interpreter) { Private = true }).ToArray();
            pathsStaticClass.Vars.AddRange(runner.Game.Paths.Map(p => new Variable(p.Name, p.ID.ToExp(), cons: true)));
            ClassDefSpan roomsStaticClass = new("Rooms", [], []) { Namespace = EngineNamespace };
            roomsStaticClass.Funcs = roomsStaticClass.Funcs.Append(new ConstructorDefSpan([], [], roomsStaticClass, runner.Interpreter) { Private = true }).ToArray();
            roomsStaticClass.Vars.AddRange(runner.Game.Rooms.Map(r => new Variable(r.Name, r.ID.ToExp(), cons: true)));
            ClassDefSpan fontsStaticClass = new("Fonts", [], []) { Namespace = EngineNamespace };
            fontsStaticClass.Funcs = fontsStaticClass.Funcs.Append(new ConstructorDefSpan([], [], fontsStaticClass, runner.Interpreter) { Private = true }).ToArray();
            fontsStaticClass.Vars.AddRange(runner.Game.FontsData.Map(f => new Variable(f.Name, f.ID.ToExp(), cons: true)));
            runner.Interpreter.definations.AddRange([spritesStaticClass, instanceStaticClass, soundsStaticClass, pathsStaticClass, roomsStaticClass, fontsStaticClass, SoundPlaybackInstance.Class]);

            // add all methods with [EngineFunc] attribute
            void AddMarkedFuncs(object? instance, Type? type = null)
            {
                foreach (var methodInfo in (type ?? instance?.GetType() ?? throw new ArgumentNullException()).GetMethods())
                {
                    var attr = methodInfo.GetCustomAttribute<EngineFuncAttribute>();
                    if (attr != null)
                    {
                        // create Func<...> from methodInfo
                        var invoker = (Func<Exp.Instance?, IValue?[], IValue?>)Delegate.CreateDelegate(typeof(Func<Exp.Instance?, IValue?[], IValue?>), instance, methodInfo);
                        string invokerName = attr.CustomName ?? methodInfo.Name.StartWithLowerCase();

                        // add to interpreter
                        if (attr.IsNonStaticFuncOfGameObjects)
                            objClasses.ForEach(objCls => runner.Interpreter.AddExternFunc(new(invoker, attr.ParamsCounts, invokerName), objCls));
                        else
                            runner.Interpreter.AddExternFunc(new(invoker, attr.ParamsCounts, invokerName, EngineNamespace));
                    }
                }
            }
            AddMarkedFuncs(runner.Game, typeof(IGame));
            AddMarkedFuncs(runner);
            AddMarkedFuncs(null, typeof(Formulas));

            // manually add non-static functions that cannot be marked with [EngineFunc]
            runner.Interpreter.AddExternFunc(new(runner.Game.PauseSound, 0, "pause"), SoundPlaybackInstance.Class);
            runner.Interpreter.AddExternFunc(new(runner.Game.ResumeSound, 0, "resume"), SoundPlaybackInstance.Class);

            // add "all()" functions and "i" / "first" getters
            foreach (var cls in objClasses)
            {
                IValue? All(Exp.Instance? _, IValue?[] args)
                {
                    if (runner.Game.CurrentRoom == null)
                        return new Exp.Instance(ClassDefSpan.ExpArrayDef, []);

                    List<Exp.Instance> all = [];

                    foreach (var inst in runner.Game.CurrentRoom.Instances)
                    {
                        if (inst.Model.Class == cls)
                            all.Add(inst);
                    }

                    return new Exp.Instance(ClassDefSpan.ExpArrayDef, [.. all]);
                }

                IValue? GetSingleInst()
                {
                    if (runner.Game.CurrentRoom == null)
                        return null;

                    Runtime.Instance? i = null;

                    foreach (var inst in runner.Game.CurrentRoom.Instances)
                    {
                        if (inst.Model.Class == cls)
                        {
                            if (i == null)
                                i = inst;
                            else
                                runner.Interpreter.ThrowRuntime($"There is more than 1 instance of object '{cls.Name}' in current room.", RuntimeException.INVALID_OPERATION);
                        }
                    }

                    return i;
                }

                runner.Interpreter.AddExternFunc(new(All, 0, "all" /* name must be specified in local functions! */), cls, true);
                cls.Vars.Add(new CustomVariable("i", GetSingleInst, null, false));
                cls.Vars.Add(new CustomVariable("first", () => runner.Game.CurrentRoom?.Instances.FirstOrDefault(i => i.Model.Class == cls), null, false));
            }
        }

        public static InstanceScriptDocument CreateInstanceScriptDocument(string name, ClassDefSpan def, string script, params string[] args)
        {
            InstanceScriptDocument doc = new(name, def, script, args);
            doc.Namespace = GameNamespace;
            doc.Usings.AddRange(GlobalUsings);
            return doc;
        }

        public static IEnumerable<Type> GetEnums(Assembly? assembly = null)
        {
            List<Type> types = [];

            // get all enums in this assembly marked with [ExpEnum]
            foreach (var type in (assembly ?? typeof(ExpSrc).Assembly).GetTypes())
            {
                if (type.IsEnum && type.GetCustomAttribute<ExpEnumAttribute>() != null)
                    types.Add(type);
            }

            return types;
        }

        public static List<ExternEngineItem> AllExternFuncsAndProperties { get => field ??= GetAllExternFuncsAndProperties(); }
        private static List<ExternEngineItem> GetAllExternFuncsAndProperties()
        {
            List<ExternEngineItem> all = [];

            void AddMarkedFuncs(object? instance, Type? type = null)
            {
                Type finalType = type ?? instance?.GetType() ?? throw new ArgumentNullException();

                // methods
                foreach (var methodInfo in finalType.GetMethods())
                {
                    var attr = methodInfo.GetCustomAttribute<EngineFuncAttribute>();
                    if (attr != null)
                        all.Add(new ExternEngineFunc(attr.CustomName ?? methodInfo.Name.StartWithLowerCase(), XmlDocReader.GetMethodSummary(methodInfo), methodInfo));
                }

                // properties
                foreach (var propertyInfo in finalType.GetProperties())
                {
                    var attr = propertyInfo.GetCustomAttribute<ExpPropertyAttribute>();
                    if (attr != null)
                        all.Add(new ExternEngineProperty(propertyInfo.Name.StartWithLowerCase(), "", propertyInfo));
                }
            }

            AddMarkedFuncs(null, typeof(IGame));
            AddMarkedFuncs(null, typeof(GameRunner<>));
            AddMarkedFuncs(null, typeof(Runtime.Instance));

            return all;
        }
    }
}

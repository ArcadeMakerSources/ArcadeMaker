using ArcadeMaker.Core.Models;
using ArcadeMaker.Core.Runtime;
using Exp;
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
                    var attr = methodInfo.GetCustomAttribute<ExpFuncAttribute>();
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

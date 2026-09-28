using Exp.Spans;
using System;
using System.Reflection;

namespace Exp.Builtins;

[AttributeUsage(AttributeTargets.Method)]
class InitializesAttribute(string? ns, string cls, string? property = null) : Attribute
{
    internal string? Namespace => ns;
    internal string ClassName => cls;
    internal string? PropertyName => property;
    public bool InitOnBuildTime { get; init; }

    internal static void InitAll(Interpreter interpreter, Assembly assembly)
    {
        // find all init methods
        foreach (Type type in assembly.GetTypes())
        {
            foreach (MethodInfo method in type.GetMethods())
            {
                if (method.GetCustomAttribute<InitializesAttribute>() is { } attr)
                {
                    if (!ValidateInitializerSignature(interpreter, method))
                        continue;

                    var cls =
                        interpreter.
                        definations.
                        OfType<ClassDefSpan>().
                        FirstOrDefault(c => c.Namespace == attr.Namespace && c.Name == attr.ClassName);

                    if (cls?.Vars.FirstOrDefault(p => p.Name == (attr.PropertyName ?? method.Name.StartWithLowerCase())) is not ClassStaticVar property)
                        interpreter.Error($"No matching property was found to initializer method '{method.DeclaringType!.FullName}.{method.Name}'.");
                    else
                    {
                        if (attr.InitOnBuildTime)
                            Init(interpreter, property, method);
                        else
                        {
                            interpreter.staticPropsToInit.Add(property, new Operations.CustomReadingOperation<IValue>(() => method.Invoke(null, null) as IValue));
                        }
                    }
                }
            }
        }
    }

    private static bool ValidateInitializerSignature(Interpreter interpreter, MethodInfo method)
    {
        bool valid = true;

        if (!method.IsStatic)
        {
            interpreter.Error($"Initializer method '{method.DeclaringType!.FullName}.{method.Name}(...)' must be static.");
            valid = false;
        }
        if (method.GetParameters().Length >= 1)
        {
            interpreter.Error($"Initializer method '{method.DeclaringType!.FullName}.{method.Name}(...)' must be parameterless.");
            valid = false;
        }
        if (method.ReturnType != typeof(IValue) && !method.ReturnType.GetInterfaces().Contains(typeof(IValue)))
        {
            interpreter.Error($"Initializer method '{method.DeclaringType!.FullName}.{method.Name}(...)' must return {nameof(IValue)}.");
            valid = false;
        }

        return valid;
    }

    private static void Init(Interpreter interpreter, IClassMember property, MethodInfo initializer)
    {
        if (property is Variable var)
        {
            var.SetSkippingConstant(initializer.Invoke(null, null) as IValue);
        }
        else
        {
            interpreter.ThrowRuntime($"Could not init {property.Def.GetExpTypeName(false)}.{property.Name} becuase it is not a variable.", RuntimeException.INIT_ERR, property as Span);
        }
    }
}

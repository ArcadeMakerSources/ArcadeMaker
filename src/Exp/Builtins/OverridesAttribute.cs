using System;
using System.Collections.Generic;
using System.Reflection;
using Exp.Converting;
using Exp.Operations;
using Exp.Spans;

namespace Exp.Builtins;

[AttributeUsage(AttributeTargets.Method)]
class OverridesAttribute(string? ns, string? cls, string? func = null) : Attribute
{
    public string? Namespace => ns;
    public string? Class => cls;
    public string? Func { get => func; private set => func = value; }

    public static void ApplyForAll(Interpreter interpreter)
    {
        // find all C# methods with [Overrides] tag
        foreach (Type type in typeof(OverridesAttribute).Assembly.GetTypes())
        {
            foreach (var method in type.GetMethods())
            {
                if (method.GetCustomAttribute<OverridesAttribute>() is { } attr)
                {
                    attr.Func ??= method.Name.StartWithLowerCase();

                    // method found, now find the target exp func
                    var matchFuncs =
                        interpreter.
                        FuncsThatMustBeImplementedExternally.
                        Where(f => IsMatch(f, attr));

                    if (!matchFuncs.Any())
                    {
                        // if no matching exp func found, make a build-time error
                        string msg = $"Method {type.FullName}.{method.Name} is marked with [Overrides] but no matching Exp function found ";
                        msg += $"(Namespace: {attr.Namespace ?? "<NONE>"}, Class: {attr.Class ?? "<NONE>"}, Function: {attr.Func}).";
                        interpreter.Error(msg);
                    }
                    else
                    {
                        foreach (FuncDefSpan func in matchFuncs.ToArray()) // .ToArray() to prevent "Collection was modified" exception
                        {
                            // apply for this function
                            try
                            {
                                func.Operations = [new ExternFuncInvocationOperation(func, Converting.Convert.ToFunc(method, attr.Namespace, true))];
                            }
                            catch (Exception ex)
                            {
                                interpreter.Error($"Failed to override {func.GetExpTypeName(false)}: {ex.Message}");
                            }
                            interpreter.FuncsThatMustBeImplementedExternally.Remove(func);
                        }
                    }

                    static bool IsMatch(FuncDefSpan func, OverridesAttribute attr)
                    {
                        if (func.Name != attr.Func)
                            return false;

                        if (func.Def != null)
                            return func.Def.Namespace == attr.Namespace && func.Def.Name == attr.Class;
                        return func.Namespace == attr.Namespace && attr.Class == null;
                    }
                }
            }
        }

        // if there are still functions without C# implementations, make a build-time error
        foreach (FuncDefSpan func in interpreter.FuncsThatMustBeImplementedExternally)
            interpreter.Error($"No extern implementation found for function {func.GetExpTypeName(false)}.");
    }
}

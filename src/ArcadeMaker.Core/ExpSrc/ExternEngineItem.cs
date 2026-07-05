using Exp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc;

public abstract record class ExternEngineItem(string Name, string? Desc);

public record class ExternEngineFunc(string Name, string? Desc, MethodInfo Method) : ExternEngineItem(Name, Desc)
{
    public ParamAttribute[] Params { get; } = GetParams(Method);

    private static ParamAttribute[] GetParams(MethodInfo method)
    {
        ParamAttribute[] prms = [.. method.GetCustomAttributes<ParamAttribute>()];

        // throw if there are duplicate parameter names
        HashSet<string> names = [];
        foreach (var param in prms)
        {
            if (!names.Add(param.Name))
                throw new Exception($"Duplicate parameter name: '{param.Name}'.");
        }

        return prms;
    }
}

public record class ExternEngineProperty(string Name, string? Desc, PropertyInfo Property) : ExternEngineItem(Name, Desc);
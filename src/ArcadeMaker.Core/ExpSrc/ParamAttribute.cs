using ArcadeMaker.Core.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ParamAttribute(string name, string type, string? description = null) : Attribute
{
    public string Name => name;
    public string Type => type;
    public string? Description => description;
    public bool Optional { get; init; }
    public bool Nullable => Name.EndsWith('?');
}

public static class ParamType
{
    // primitives
    public const string Bool = Exp.ValueHelper.tbool;
    public const string Char = Exp.ValueHelper.tchar;
    public const string Number = Exp.ValueHelper.tnum;
    public const string FuncPntr = Exp.ValueHelper.tfunc;

    // basics
    public const string String = "string";
    public const string Array = "Array";
    public const string Type = "Type";

    // engine
    public const string Key = nameof(Controls.Keys);
    public const string MouseButton = nameof(Controls.MouseButton);
    public const string GamepadButton = nameof(Controls.GamepadButton);
    public const string PathEndAction = nameof(Resources.PathEndAction);
    public const string GameObject = "Game Object";

    // others
    public const string Any = "any";
}
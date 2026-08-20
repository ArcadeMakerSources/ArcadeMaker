using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exp.Converting;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ExpFuncAttribute(params int[] paramsCounts) : Attribute
{
    public int[] ParamsCounts => paramsCounts;
    public string? CustomName { get; init; }
}

public class ExpClassFuncAttribute(params int[] paramsCounts) : ExpFuncAttribute(paramsCounts)
{
    public bool Static { get; init; }
}
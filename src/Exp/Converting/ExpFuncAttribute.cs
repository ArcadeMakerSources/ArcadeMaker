using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exp.Converting;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ExpFuncAttribute(params int[] paramsCount) : Attribute
{
    public int[] ParamsCounts => paramsCount;
    public string? CustomName { get; init; }
}
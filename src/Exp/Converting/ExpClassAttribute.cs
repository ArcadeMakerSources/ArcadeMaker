using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exp.Converting;

[AttributeUsage(AttributeTargets.Class)]
public class ExpClassAttribute : Attribute
{
    public string? CustomName { get; init; }
    public string? Namespace { get; init; }
}

public interface IConvertable
{
    static abstract ClassDefSpan Class { get; set; }
    static virtual string? Namespace { get; }
}
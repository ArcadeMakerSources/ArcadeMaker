using System;
using System.Collections.Generic;
using Exp;
using Exp.Converting;
using Exp.Spans;

namespace ArcadeMaker.Core.ExpSrc.General;

public class TouchLocation : Exp.Instance, IConvertable
{
    public static ClassDefSpan? Class { get; set; }
    public static string Namespace => ExpSrc.EngineNamespace;

    internal readonly double x, y;
    internal readonly TouchState state;
    public CustomVariable X { get; }
    public CustomVariable Y { get; }
    public CustomVariable State { get; }

    public TouchLocation(double x, double y, double state) : base(Class!, addProperties: false)
    {
        (this.x, this.y, this.state) = (x, y, (TouchState)state);
        X = new("x", () => x.ToExp(), null);
        Y = new("y", () => y.ToExp(), null);
        State = new("state", () => ((double)state).ToExp(), null);
        Vars.AddRange([X, Y, State]);
    }

    [ExpCtor(3)]
    public static TouchLocation Create(Exp.Instance? _, IValue?[] args) => new(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number, args[2].ThrowIfNull().Number);
}

[ExpEnum]
public enum TouchState
{
   Moved = 1,
   Pressed = 2,
   Released = 3
}
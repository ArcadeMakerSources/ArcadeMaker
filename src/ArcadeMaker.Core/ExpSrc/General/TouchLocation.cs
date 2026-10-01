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

    internal readonly int id;
    internal readonly double x, y;
    internal readonly TouchState state;
    public CustomVariable Id { get; }
    public CustomVariable X { get; }
    public CustomVariable Y { get; }
    public CustomVariable State { get; }

    public TouchLocation(int id, double x, double y, double state) : base(Class!, addProperties: false)
    {
        (this.id, this.x, this.y, this.state) = (id, x, y, (TouchState)state);
        Id = new("id", () => id.ToExp(), null);
        X = new("x", () => x.ToExp(), null);
        Y = new("y", () => y.ToExp(), null);
        State = new("state", () => ((double)state).ToExp(), null);
        Vars.AddRange([X, Y, State]);
    }
}

[ExpEnum]
public enum TouchState
{
   Moved = 1,
   Pressed = 2,
   Released = 3
}
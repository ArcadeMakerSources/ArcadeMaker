using Exp;
using Exp.Converting;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc.General;

internal class PathPoint : Exp.Instance, IConvertable
{
    public static ClassDefSpan? Class { get; set; }
    public static string Namespace => ExpSrc.EngineNamespace;

    internal readonly double x, y, speed;
    public CustomVariable X { get; }
    public CustomVariable Y { get; }
    public CustomVariable Speed { get; }
    internal PathPoint(double x, double y, double speed) : base(Class!, addProperties: false)
    {
        (this.x, this.y, this.speed) = (x, y, speed);
        X = new("x", () => x.ToExp(), null);
        Y = new("y", () => y.ToExp(), null);
        Speed = new("speed", () => speed.ToExp(), null);
        Vars.AddRange([X, Y, Speed]);
    }

    [ExpCtor(3)]
    public static PathPoint Create(Exp.Instance? _, IValue?[] args) => new(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number, args[2].ThrowIfNull().Number);
}
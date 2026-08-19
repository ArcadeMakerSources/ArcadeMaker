using Exp;
using Exp.Converting;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc.General;

[ExpClass(Namespace = ExpSrc.EngineNamespace)]
internal class PathPoint : Exp.Instance, IConvertable
{
    internal readonly double x, y, speed;
    public CustomVariable X { get; }
    public CustomVariable Y { get; }
    public CustomVariable Speed { get; }
    public static ClassDefSpan? Class { get; set; }

    internal PathPoint(double x, double y, double speed) : base(Class, addProperties: false)
    {
        (this.x, this.y, this.speed) = (x, y, speed);
        X = new("x", () => x.ToExp(), null);
        Y = new("y", () => y.ToExp(), null);
        Speed = new("speed", () => speed.ToExp(), null);
        Vars.AddRange([X, Y, Speed]);
    }

    [ExpCtor(3)]
    public static PathPoint Create(Exp.Instance? _, IValue?[] args) => new(args[0].Number, args[1].Number, args[2].Number);

    [ExpFunc(1)]
    public static IValue XPlusY(Exp.Instance calling, IValue?[] args)
    {
        var point = (PathPoint)calling;
        return (point.x + point.y).ToExp();
    }
}
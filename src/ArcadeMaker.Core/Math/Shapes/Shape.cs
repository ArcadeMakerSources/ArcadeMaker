using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Math.Shapes;

public abstract class Shape
{
    public virtual double X { get; set; }
    public virtual double Y { get; set; }
    public virtual int OriginX { get; set; }
    public virtual int OriginY { get; set; }

    /// <summary>
    /// The angle in degrees.
    /// </summary>
    public virtual double Angle { get; set; }
}
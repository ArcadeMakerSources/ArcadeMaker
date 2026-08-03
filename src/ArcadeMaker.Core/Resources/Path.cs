using System;
using System.Collections.Generic;
using System.Text;
using ArcadeMaker.Core.ExpSrc;

namespace ArcadeMaker.Core.Resources;

public class Path : ISetsID
{
    public readonly record struct Step
    {
        public double Width { get; }
        public double Height { get; }
        public double Speed { get; }
        public double Direction { get; }
        public Step(double width, double height, double speed)
        {
            this.Width = width;
            this.Height = height;
            this.Speed = speed;

            // calculate direction
            Direction = Math.Formulas.AngleBetween(0, 0, Width, Height) - 90;
        }
    }

    public string Name { get; }
    public int ID { get; } = Core.ID.Generate();

    public double StartPositionX { get; }
    public double StartPositionY { get; }
    public Step[] Steps { get; }

    public Path(string name, double startX, double startY, Step[] steps)
    {
        this.Name = name;
        StartPositionX = startX;
        StartPositionY = startY;
        this.Steps = steps;
    }
}

[ExpEnum]
public enum PathEndAction
{
    Stop,
    Restart,
    Reverse
}
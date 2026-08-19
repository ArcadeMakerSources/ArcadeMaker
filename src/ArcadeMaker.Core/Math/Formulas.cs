using ArcadeMaker.Core.ExpSrc;
using Exp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Math;

namespace ArcadeMaker.Core.Math;

public static class Formulas
{
    [EngineFunc(4)]
    [Param("x1", ParamType.Number, "The x of point 1.")]
    [Param("y1", ParamType.Number, "The y of point 1.")]
    [Param("x2", ParamType.Number, "The x of point 2.")]
    [Param("y2", ParamType.Number, "The y of point 2.")]
    public static IValue DistanceBetween(Exp.Instance? _, IValue?[] args) => DistanceBetween(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number, args[2].ThrowIfNull().Number, args[3].ThrowIfNull().Number).ToExp();
    public static double DistanceBetween(double x1, double y1, double x2, double y2) => Sqrt(Pow(x2 - x1, 2) + Pow(y2 - y1, 2));
    public static double AngleBetween(double x1, double y1, double x2, double y2) => RadiansToDegrees(Atan2(x2 - x1, y2 - y1));

    // ---------------------------------------------------------------------------------------------------------------------------------
    // see https://gamedev.stackexchange.com/questions/172137/how-to-get-the-javascript-equivalent-of-gamemakers-hspeed-and-vspeed-given

    /// <summary>
    /// Gets the x of a position "len" pixels from the starting point and in direction "dir".
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(len, dir).</param>
    /// <returns>The x of a position "len" pixels from the starting point and in direction "dir".</returns>
    [EngineFunc(2)]
    [Param("len", ParamType.Number, "The length away of the point to return.")]
    [Param("dir", ParamType.Number, "The direction of the point to return.")]
    public static IValue LengthDirX(Exp.Instance? _, IValue?[] args) => LengthDirX(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();
    public static double LengthDirX(double length, double direction) => length * Cos(DegreesToRadians(direction));

    /// <summary>
    /// Gets the y of a position "len" pixels from the starting point and in direction "dir".
    /// </summary>
    /// <param name="_">(Unused).</param>
    /// <param name="args">(len, dir).</param>
    /// <returns>The y of a position "len" pixels from the starting point and in direction "dir".</returns>
    [EngineFunc(2)]
    [Param("len", ParamType.Number, "The length away of the point to return.")]
    [Param("dir", ParamType.Number, "The direction of the point to return.")]
    public static IValue LengthDirY(Exp.Instance? _, IValue?[] args) => LengthDirY(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();
    public static double LengthDirY(double length, double direction) => length * Sin(DegreesToRadians(direction));
    public static (double hspeed, double vspeed) LengthDir(double length, int direction) => (LengthDirX(length, direction), LengthDirY(length, direction));
    public static (double speed, double direction) SpeedsToVelocity(double hspeed, double vspeed)
    {
        return (
            Sqrt(hspeed * hspeed + vspeed * vspeed),
            RadiansToDegrees(Atan2(vspeed, hspeed))
        );
    }
    // ---------------------------------------------------------------------------------------------------------------------------------

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double RadiansToDegrees(double radians) => radians * (180 / PI);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double DegreesToRadians(double degrees) => degrees * (PI / 180);

    /// <summary>
    /// The linear mapping (or "min-max normalization") formula, used to convert a number from an original scale range to a new target range while maintaining the relative ratio.
    /// </summary>
    /// <param name="originalRangeMin">Minimum of the original range.</param>
    /// <param name="originalRangeMax">Maximum of the original range.</param>
    /// <param name="targetRangeMin">Minimum of the target range.</param>
    /// <param name="targetRangeMax">Maximum of the target range.</param>
    /// <param name="value">The number to convert.</param>
    /// <returns>A number in the target range, relative to the original number.</returns>
    public static double LinearMapping(double originalRangeMin, double originalRangeMax, double targetRangeMin, double targetRangeMax, double value)
    {
        return (((value - originalRangeMin) * (targetRangeMax - targetRangeMin)) / (originalRangeMax - originalRangeMin)) + targetRangeMin;
    }
}
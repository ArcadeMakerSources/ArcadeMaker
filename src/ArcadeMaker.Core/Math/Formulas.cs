using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static System.Math;

namespace ArcadeMaker.Core.Math;

public static class Formulas
{
    public static double DistanceBetween(double x1, double y1, double x2, double y2) => Sqrt(Pow(x2 - x1, 2) + Pow(y2 - y1, 2));
    public static double AngleBetween(double x1, double y1, double x2, double y2) => RadiansToDegrees(Atan2(x2 - x1, y2 - y1));

    // ---------------------------------------------------------------------------------------------------------------------------------
    // see https://gamedev.stackexchange.com/questions/172137/how-to-get-the-javascript-equivalent-of-gamemakers-hspeed-and-vspeed-given
    public static double LengthDirX(double speed, double direction) => speed * Cos(DegreesToRadians(direction));
    public static double LengthDirY(double speed, double direction) => speed * Sin(DegreesToRadians(direction));
    public static (double hspeed, double vspeed) LengthDir(double speed, int direction) => (LengthDirX(speed, direction), LengthDirY(speed, direction));
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
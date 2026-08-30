using System;
using Exp.Converting;
using Smath = System.Math;

namespace Exp.Builtins;

/// <summary>
/// <inheritdoc cref="Smath"/>
/// </summary>
static class Math
{
    private const string ns = "math";

    private static readonly NumberValue _pi  = Smath.PI.ToExp();
    private static readonly NumberValue _e   = Smath.E.ToExp();
    private static readonly NumberValue _tau = Smath.Tau.ToExp();

    [ExpFunc(Namespace = ns)]
    public static IValue Pi(Instance? _, IValue?[] args) => _pi;

    [ExpFunc(Namespace = ns)]
    public static IValue E(Instance? _, IValue?[] args) => _e;

    [ExpFunc(Namespace = ns)]
    public static IValue Tau(Instance? _, IValue?[] args) => _tau;

    [ExpFunc(1, Namespace = ns)]
    public static IValue Sqrt(Instance? _, IValue?[] args) => Smath.Sqrt(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Sin(Instance? _, IValue?[] args) => Smath.Sin(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Cos(Instance? _, IValue?[] args) => Smath.Cos(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Tan(Instance? _, IValue?[] args) => Smath.Tan(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Atan(Instance? _, IValue?[] args) => Smath.Atan(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue Atan2(Instance? _, IValue?[] args) => Smath.Atan2(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Tanh(Instance? _, IValue?[] args) => Smath.Tanh(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Acos(Instance? _, IValue?[] args) => Smath.Acos(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Acosh(Instance? _, IValue?[] args) => Smath.Acosh(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Abs(Instance? _, IValue?[] args) => Smath.Abs(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue Min(Instance? _, IValue?[] args) => Smath.Min(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue Max(Instance? _, IValue?[] args) => Smath.Max(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue MinMagnitude(Instance? _, IValue?[] args) => Smath.MinMagnitude(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue MaxMagnitude(Instance? _, IValue?[] args) => Smath.MaxMagnitude(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Log(Instance? _, IValue?[] args) => Smath.Log(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Log2(Instance? _, IValue?[] args) => Smath.Log2(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Log10(Instance? _, IValue?[] args) => Smath.Log10(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue ILogB(Instance? _, IValue?[] args) => Smath.ILogB(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Exp(Instance? _, IValue?[] args) => Smath.Exp(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue Pow(Instance? _, IValue?[] args) => Smath.Pow(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Sinh(Instance? _, IValue?[] args) => Smath.Sinh(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Asin(Instance? _, IValue?[] args) => Smath.Asin(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Asinh(Instance? _, IValue?[] args) => Smath.Asinh(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue BigMul(Instance? _, IValue?[] args) => ((double)Smath.BigMul((long)args[0].ThrowIfNull().Number, (long)args[1].ThrowIfNull().Number)).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Cbrt(Instance? _, IValue?[] args) => Smath.Cbrt(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Ceiling(Instance? _, IValue?[] args) => Smath.Ceiling(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(3, Namespace = ns)]
    public static IValue Clamp(Instance? _, IValue?[] args) => Smath.Clamp(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number, args[2].ThrowIfNull().Number).ToExp();

    [ExpFunc(2, Namespace = ns)]
    public static IValue CopySign(Instance? _, IValue?[] args) => Smath.CopySign(args[0].ThrowIfNull().Number, args[1].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Floor(Instance? _, IValue?[] args) => Smath.Floor(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue Round(Instance? _, IValue?[] args) => Smath.Round(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue ReciprocalEstimate(Instance? _, IValue?[] args) => Smath.ReciprocalEstimate(args[0].ThrowIfNull().Number).ToExp();

    [ExpFunc(1, Namespace = ns)]
    public static IValue ReciprocalSqrtEstimate(Instance? _, IValue?[] args) => Smath.ReciprocalSqrtEstimate(args[0].ThrowIfNull().Number).ToExp();
}
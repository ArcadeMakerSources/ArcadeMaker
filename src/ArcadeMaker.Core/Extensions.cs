using Exp;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace ArcadeMaker.Core;

public static class Extensions
{
    public static void ValidateArgsNumber(this IValue?[] args, int num)
    {
        if (args.Length != num)
        {
            throw new ArgumentException($"This function takes {num} arguments, but {args.Length} were passed.");
        }
    }

    public static void ValidateArgsRange(this IValue?[] args, params int[] options)
    {
        if (!options.Contains(args.Length))
        {
            string opStr = "";
            options.ForEach((x, i) => opStr += opStr + (i == options.Length - 2 ? " or " : (i == options.Length - 1 ? "" : ", ")));
            throw new ArgumentException($"This function may take {opStr} parameters, but {args.Length} were passed.");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IValue[] ThrowIfNull([NotNull] params IValue?[] values)
    {
        int i = 0;
        foreach (var v in values)
        {
            i++;
            if (v == null)
                throw new ArgumentNullException($"Argument {i} cannot be null.");
        }

        return values!;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IValue ThrowIfNull([NotNull] this IValue? value) => ThrowIfNull(values: value)[0]!;

    public static void Add<TKey, TValue>(this Dictionary<TKey, List<TValue>> dictionary, TKey key, TValue value)
    {
        if (dictionary.TryGetValue(key, out var ls))
            ls.Add(value);
        else
            dictionary.Add(key, [value]);
    }
}
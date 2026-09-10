using System;
using System.Linq;
using System.Collections.Generic;
using Exp.Spans;

namespace Exp;

public static class CSBasicTypes
{
    internal static Instance AsExtern(this object obj)
    {
        if (obj == null)
            return null;

        return new ExternTypeInstance(obj);
    }

    internal static Array MinArray(ArrayInstance arr)
    {
        if (arr == null || !arr.IsArray)
            Interpreter.Activated.Error($"Invalid argument: {nameof(arr)} must be an array.");
        if (arr.ArrayValues.Length == 0)
            return arr.ArrayValues;

        Type highest = (arr.Vars[0]?.Value as Instance)?.Vars[1]?.Value as Type; // exp Array.csType.instance

        if (highest == null)
        {
            // determine the item that extends the fewest number of types
            int hdepth = -1, i = -1;
            foreach (var item in arr.ArrayValues)
            {
                i++;

                int DepthOf(Type type)
                {
                    int d = 0;
                    while (type != typeof(object))
                    {
                        d++;
                        type = type.BaseType;
                    }
                    return d;
                }

                Type t = item.GetType();
                int d = DepthOf(t);
                if (d < hdepth || hdepth < 0)
                {
                    highest = t;
                    hdepth = d;
                }
            }
        }

        // validate that all items extend this item
        foreach (var item in arr.ArrayValues)
        {
            bool Extends(Type c, Type p)
            {
                if (p != null && c == p)
                    return true;
                while (c != null)
                {
                    c = c.BaseType;
                    if (p != null && c == p)
                        return true;
                }
                return false;
            }

            if (!Extends(item.GetType(), highest))
                return arr.ArrayValues;
        }

        var res = Array.CreateInstance(highest, arr.ArrayValues.Length);
        Array.Copy(arr.ArrayValues, 0, res, 0, res.Length);
        return res;
    }
}

public class Test : IDisposable
{
    public void Dispose() { }
    public static void Met(double d) { }
    public void Met(object[] i) => $"Great o[] {i.Length}".Print();
    public static void Met(Test[] i) => $"Great t[] {i.Length}".Print();
    //public Test() { }
    public double D { get; set; } = 355;
    public static object Reado { get; } = 777;
    public static object Writeo { set => $"setted to {value}".Println(); }
    public override string ToString() => $"[Amazing Test where d = {D}]";
}

public class GTest<T>
{
    public void Met(T obj) => obj.Println();
    public static object Testing(object[] args)
    {
        return null;
        //var (c1, s1) = args.ValidateArguments<char, string>();

        //return !string.IsNullOrEmpty(s1) && (s1[0] == c1);
    }

    static void F(int a) { }
    static void F(int a, int b) { }
}

public static class StatCls;
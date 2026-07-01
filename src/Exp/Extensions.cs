using Exp.Operations;
using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Exp;

public static class Extensions
{
    internal static T FirstOrNull<T>(this IEnumerable<T> ts, Func<T, bool> cond = null) where T : class => cond == null ? ts.FirstOrDefault() : ts.FirstOrDefault(cond);

    public static bool IsDigit(this char c)
    {
        return c >= '0' && c <= '9';
    }

    public static T Print<T>(this T s, object plus = null)
    {
        Console.Write(s.ToString() + (plus ?? ""));
        return s;
    }

    public static T Println<T>(this T s, object plus = null)
    {
        if (s is TextSpan[] spans)
        {
            Println("[");
            foreach (var span in spans)
                span.Print(", ");
            Println("]");
        }
        else
            Console.WriteLine(s.ToString() + (plus ?? ""));
        return s;
    }

    public static T[] PackAsArray<T>(this T item, bool packNull)
    {
        if (item == null && packNull)
            return [item];
        return item == null ? null : [item];
    }

    public static List<TResult> Map<T, TResult>(this IEnumerable<T> collection, Func<T, TResult> mapper)
    {
        List<TResult> ls = [];
        foreach (var item in collection)
            ls.Add(mapper(item));
        return ls;
    }

    public static int CountOf(this string src, char c, int startIndex = 0, int endIndex = -1, bool ignoreCase = false)
    {
        if (src.Length == 0)
            return 0;

        if (endIndex < 0)
            endIndex = src.Length - 1;
        if (startIndex < 0 || startIndex >= src.Length || endIndex < startIndex || endIndex >= src.Length)
            throw new IndexOutOfRangeException("start or end index is out of range");
        int count = 0;
        for (int i = startIndex; i <= endIndex; i++)
        {
            char cc = src[i];
            if (ignoreCase ? cc.EqualsIgnoreCase(c) : cc == c)
                count++;
        }
        return count;
    }

    public static bool EqualsIgnoreCase(this char c, char value)
    {
        return c.ToString().Equals(value.ToString(), StringComparison.CurrentCultureIgnoreCase);
    }

    public static string StartWithLowerCase(this string str)
    {
        ArgumentNullException.ThrowIfNull(str);

        if (str.Length == 0)
            return str;

        return char.ToLower(str[0]) + (str.Length >= 2 ? str.Substring(1) : "");
    }

    public static string ToString<T>(this IEnumerable<T> arr, string seperator)
    {
        string s = "";
        int len = arr.Count();
        if (arr != null)
        {
            int i = 0;
            foreach (var item in arr)
                s += item + (++i < len ? seperator : "");
        }
        return s;
    }

    internal static Instance ToExpArray(this IValue[] arr)
    {
        ArgumentNullException.ThrowIfNull(arr, nameof(arr));

        var objs = new IValue[arr.Length];
        Array.Copy(arr, objs, arr.Length);
        return new Instance(ClassDefSpan.ExpArrayDef, objs);
    }

    public static Instance ToExpString(this string str)
    {
        return Interpreter.StringToExpString(str);
    }

    public static BoolValue ToExp(this bool b) => b;
    public static CharValue ToExp(this char c) => c;
    public static NumberValue ToExp(this double n) => n;
    public static NumberValue ToExp(this int n) => n;

    public static bool IsString(this IValue val) => val.IsInst && val.Inst.def == ClassDefSpan.ExpStringDef;

    public static void AddRange<T>(this HashSet<T> hs, IEnumerable<T> range)
    {
        ArgumentNullException.ThrowIfNull(hs);
        ArgumentNullException.ThrowIfNull(range);

        foreach (var x in range)
            hs.Add(x);
    }

    public static IEnumerable<T> AppendRange<T>(this IEnumerable<T> collection, IEnumerable<T> range)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(range);

        foreach (var x in range)
            collection = collection.Append(x);

        return collection;
    }

    public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
    {
        foreach (T obj in collection)
            action(obj);
    }

    public static void ForEach<T>(this IEnumerable<T> collection, Action<T, int> action)
    {
        int counter = 0;
        foreach (T obj in collection)
            action(obj, counter++);
    }

    public static List<T> Remove<T>(this T[] arr, Func<T, bool> where)
    {
        ArgumentNullException.ThrowIfNull(arr, nameof(arr));
        ArgumentNullException.ThrowIfNull(where, nameof(where));

        List<T> ls = [];
        foreach (var x in arr)
            if (!where(x))
                ls.Add(x);
        return ls;
    }

    internal static Type GetTypeByName(string name)
    {
        Type type = Type.GetType(name);
        if (type != null)
            return type;

        // Get all loaded assemblies in the current AppDomain.
        // Reversing the order gives priority to the most recently loaded types.
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse())
        {
            // Try to get the type from the assembly.
            // The name parameter should be the full name (including namespace).
            type = assembly.GetType(name);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    internal static bool IsString(this object obj)
    {
        return obj.IsString(out var _);
    }

    internal static bool IsString(this object obj, out Instance str)
    {
        str = null;
        if (obj is Instance inst && inst.def == ClassDefSpan.ExpStringDef)
        {
            str = inst;
            return true;
        }
        return false;
    }

    internal static bool ExpStringEquals(this Instance s, Instance o)
    {
        if (s.def != ClassDefSpan.ExpStringDef || o.def != ClassDefSpan.ExpStringDef)
            return false;

        IValue[] a = s.Vars[0].Value.Inst.ArrayValues, b = o.Vars[0].Value.Inst.ArrayValues;

        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i].Char != b[i].Char)
                return false;
        }

        return true;
    }

    public static string GetExpTypeName(this object obj, bool allowNull)
    {
        if (!allowNull)
            ArgumentNullException.ThrowIfNull(obj);
        else if (obj == null)
            return "NULL";

        if (obj is IValue val and not FuncDefSpan)
        {
            if (val.IsBool)
                return ValueHelper.tbool;
            if (val.IsChar)
                return ValueHelper.tchar;
            if (val.IsNumber)
                return ValueHelper.tnum;
            if (val.IsInst)
                return ((IDefination)val.Inst.def).FullName;
            if (val.IsFunc)
                return ValueHelper.tfunc;
            if (val is SpecialValue special)
                obj = special.Value;
        }
        if (obj is IDefination def)
            return def.FullName;
        if (obj == Void.Return)
            return obj.ToString();

        return obj.GetType().GetExpTypeName();
    }

    internal static string GetExpTypeName(this Type type)
    {
        if (type == typeof(BoolValue))
            return ValueHelper.tbool;
        if (type == typeof(CharValue))
            return ValueHelper.tchar;
        if (type == typeof(NumberValue))
            return ValueHelper.tnum;
        return $"{type} (C#)";
    }

    internal static IEnumerable<Instance> GetAttrInfoOf(this ICanSetAttr item, AttributeDefSpan attr) => item.AttrInfo.Where(i => i?.Vars[2].Value.ToString() == attr.Name);

    internal static bool HasTag(this ICanSetAttr item, AttributeDefSpan attr, out Instance info)
    {
        info = item?.AttrInfo?.FirstOrDefault(i => i != null && i.Vars[2].Value.ToString() == attr.Name);
        return info != null;
    }

    internal static bool HasTag(this ICanSetAttr item, AttributeDefSpan attr) => item.HasTag(attr, out var _);

    public static string ReadLib(string fileNameWithoutExt)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string resourceName = "Exp.libs." + fileNameWithoutExt + ".txt";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                throw new FileNotFoundException("Could not find Exp library file '" + fileNameWithoutExt + ".txt'.\nMake sure the file exists in Exp/libs and its build action is set to 'Embedded Resource'.");
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd().Replace("\r", "");
            }
        }
    }

    private static T GetArgument<T>(object[] args, int index)
    {
        if (args[index] is T value)
            return value;
        throw new ArgumentException($"Argument {index} must be of type {typeof(T).FullName}.");
    }

    public static bool IsLiterallyValidName(this string name)
    {
        static bool IsAbcOr_(char c) => (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c == '_');

        bool valid = !string.IsNullOrWhiteSpace(name) &&
                     IsAbcOr_(name[0]) &&
                     name.All(c => IsAbcOr_(c) || (c >= '0' && c <= '9')) &&
                     !Filter.Keywords.Contains(name);

        return valid;
    }

    private static void ValidateCount(object[] args, int expected)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length < expected)
            throw new ArgumentException($"This function takes {expected} arguments.");
    }

    public static T1 ValidateArguments<T1>(this object[] args)
    {
        ValidateCount(args, 1);
        return (GetArgument<T1>(args, 0));
    }

    public static (T1, T2) ValidateArguments<T1, T2>(this object[] args)
    {
        ValidateCount(args, 2);
        return (GetArgument<T1>(args, 0), GetArgument<T2>(args, 1));
    }

    public static (T1, T2, T3) ValidateArguments<T1, T2, T3>(this object[] args)
    {
        ValidateCount(args, 3);
        return (GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2));
    }

    public static (T1, T2, T3, T4) ValidateArguments<T1, T2, T3, T4>(this object[] args)
    {
        ValidateCount(args, 4);
        return (GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2), GetArgument<T4>(args, 3));
    }

    public static (T1, T2, T3, T4, T5) ValidateArguments<T1, T2, T3, T4, T5>(this object[] args)
    {
        ValidateCount(args, 5);
        return (GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2), GetArgument<T4>(args, 3), GetArgument<T5>(args, 4));
    }

    public static (T1, T2, T3, T4, T5, T6) ValidateArguments<T1, T2, T3, T4, T5, T6>(this object[] args)
    {
        ValidateCount(args, 6);
        return (GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2), GetArgument<T4>(args, 3), GetArgument<T5>(args, 4), GetArgument<T6>(args, 5));
    }

    public static (T1, T2, T3, T4, T5, T6, T7) ValidateArguments<T1, T2, T3, T4, T5, T6, T7>(this object[] args)
    {
        ValidateCount(args, 7);
        return (GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2), GetArgument<T4>(args, 3), GetArgument<T5>(args, 4), GetArgument<T6>(args, 5), GetArgument<T7>(args, 6));
    }

    public static (T1, T2, T3, T4, T5, T6, T7, T8) ValidateArguments<T1, T2, T3, T4, T5, T6, T7, T8>(this object[] args)
    {
        ValidateCount(args, 8);
        return (
            GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2), GetArgument<T4>(args, 3),
            GetArgument<T5>(args, 4), GetArgument<T6>(args, 5), GetArgument<T7>(args, 6), GetArgument<T8>(args, 7)
        );
    }

    public static (T1, T2, T3, T4, T5, T6, T7, T8, T9) ValidateArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9>(this object[] args)
    {
        ValidateCount(args, 9);
        return (
            GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2), GetArgument<T4>(args, 3),
            GetArgument<T5>(args, 4), GetArgument<T6>(args, 5), GetArgument<T7>(args, 6), GetArgument<T8>(args, 7), GetArgument<T9>(args, 8)
        );
    }

    public static (T1, T2, T3, T4, T5, T6, T7, T8, T9, T10) ValidateArguments<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(this object[] args)
    {
        ValidateCount(args, 10);
        return (
            GetArgument<T1>(args, 0), GetArgument<T2>(args, 1), GetArgument<T3>(args, 2), GetArgument<T4>(args, 3),
            GetArgument<T5>(args, 4), GetArgument<T6>(args, 5), GetArgument<T7>(args, 6), GetArgument<T8>(args, 7),
            GetArgument<T9>(args, 8), GetArgument<T10>(args, 9)
        );
    }

    public static void ValidateArguments(this object[] args, params Type[] types)
    {
        ArgumentNullException.ThrowIfNull(args, nameof(args));
        ArgumentNullException.ThrowIfNull(types, nameof(types));

        if (types.Length != args.Length)
            throw new ArgumentException($"{args.Length} arguments were given, but validation list contains {types.Length} types.");

        int i = 0;
        foreach (Type type in types)
        {
            if (args[i].GetType() != type)
                throw new ArgumentException($"Argument {i} must be of type {type.FullName}.");
            i++;
        }
    }
}
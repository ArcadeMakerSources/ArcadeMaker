using Exp.Converting;
using Exp;
using System.IO;

namespace Exp.Builtins;

/// <summary>
/// An IO API based on Linux' terminal commands, like <c>mkdir(dirName)</c>.
/// </summary>
static class IO
{
    private const string ns = "io";

    // --- errors ---------------------------
    private const string ERR_IO_OPERATION_FAILED = "IO_OPERATION_FAILED";
    // --------------------------------------

    internal class Parameters
    {
        internal string CD { get; set; } = AppContext.BaseDirectory;
        internal static Parameters OfActivated => Interpreter.Activated.IOParams;
    }

    private static string GetCd(string? arg = null)
    {
        if (arg == null)
            return Parameters.OfActivated.CD;

        if (arg.Contains(':')) // reset cd with new value
            return arg;
        else                   // combine current cd with the given path
            return Parameters.OfActivated.CD + Path.DirectorySeparatorChar + arg;
    }

    private static string ValueAsString(IValue? val)
    {
        if (!val.ThrowIfNull().IsString())
            Interpreter.Activated.ThrowRuntime("string expected.", RuntimeException.INVALID_ARGUMENT);
        return Interpreter.ExpStringToString((Instance)val!);
    }

    private static void TryIO(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Interpreter.Activated.ThrowRuntime(ex.GetType().Name + ": " + ex.Message, ERR_IO_OPERATION_FAILED);
        }
    }

    private static T TryIO<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (Exception ex)
        {
            Interpreter.Activated.ThrowRuntime(ex.GetType().Name + ": " + ex.Message, ERR_IO_OPERATION_FAILED);
            throw null;
        }
    }

    [ExpFunc(0, 1, Namespace = ns)]
    public static IValue Cd(Instance? _, IValue?[] args)
    {
        // parameterless: return current cd
        if (args.Length == 0)
            return GetCd().ToExpString();

        Parameters ioparams = Parameters.OfActivated;

        // 1 parameter: set cd
        string cd = ValueAsString(args[0]);

        // backward symbol
        if (cd == "..")
            ioparams.CD = Path.GetDirectoryName(GetCd())!;
        // value to add / reset
        else
        {
            string oldCd = ioparams.CD;

            ioparams.CD = GetCd(cd);

            // validate
            if (!Directory.Exists(ioparams.CD))
            {
                string newCd = ioparams.CD;
                ioparams.CD = oldCd;
                Interpreter.Activated.ThrowRuntime($"The directory \"{newCd}\" does not exist.", RuntimeException.INVALID_OPERATION);
            }
        }

        return Void.Return;
    }

    [ExpFunc(1, Namespace = ns)]
    public static Void Mkdir(Instance? _, IValue?[] args)
    {
        string loc = GetCd(ValueAsString(args[0]));

        TryIO(() => Directory.CreateDirectory(loc));
        return Void.Return;
    }

    [ExpFunc(Namespace = ns)]
    public static Instance Pwd(Instance? _, IValue?[] args) => Parameters.OfActivated.CD.ToExpString();

    [ExpFunc(Namespace = ns)]
    public static Instance Ls(Instance? _, IValue?[] args)
    {
        List<string> names = [];
        TryIO(() =>
        {
            names.AddRange(Directory.GetFiles(Parameters.OfActivated.CD).Map(f => Path.GetFileName(f)));
            names.AddRange(Directory.GetDirectories(Parameters.OfActivated.CD).Map(d => Path.GetFileName(d)));
        });
        return new(Spans.ClassDefSpan.ExpArrayDef, [.. names.Map(n => n.ToExpString())]);
    }

    [ExpFunc(1, Namespace = ns)]
    public static Instance Cat(Instance? _, IValue?[] args) => TryIO(File.ReadAllText(GetCd(ValueAsString(args[0]))).ToExpString);

    [ExpFunc(2, Namespace = ns)]
    public static Void Echo(Instance? _, IValue?[] args)
    {
        string content = ValueAsString(args[0]);
        string path = GetCd(ValueAsString(args[1]));

        TryIO(() => File.WriteAllText(path, content));

        return Void.Return;
    }
}
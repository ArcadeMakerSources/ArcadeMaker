using Exp.Converting;

namespace Exp.Builtins.STD;

static class Casting
{
    [ExpFunc(1, CustomName = "cton", Namespace = Interpreter.STD_NAMESPACE)]
    public static IValue CharToNum(Instance? _, IValue?[] args) => ((double)args[0].ThrowIfNull().Char).ToExp();

    [ExpFunc(1, CustomName = "ntoc", Namespace = Interpreter.STD_NAMESPACE)]
    public static CharValue NumToChar(Instance? _, IValue?[] args) => ((char)args[0].ThrowIfNull().Number).ToExp();
}

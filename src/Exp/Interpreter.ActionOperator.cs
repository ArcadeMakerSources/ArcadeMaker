namespace Exp;

public partial class Interpreter
{
    internal enum ActionOperator
    {
        Reset,
        Add,
        Subtract,
        PlusPlus,
        MinusMinus,
        MinusMinusThen,
        PlusPlusThen
    }
}

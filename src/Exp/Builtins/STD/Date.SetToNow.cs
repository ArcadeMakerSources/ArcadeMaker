using System;

namespace Exp.Builtins.STD;

static partial class Impl
{
    [Overrides(Interpreter.STD_NAMESPACE, "Date", "setToNow")]
    public static Void SetDateToNow(Instance instance, IValue?[] args)
    {
        DateTime now = DateTime.Now;

        instance.Vars[0].Value = ((double)now.Year).ToExp();
        instance.Vars[1].Value = ((double)now.Month).ToExp();
        instance.Vars[2].Value = ((double)now.Day).ToExp();
        instance.Vars[3].Value = ((double)now.Hour).ToExp();
        instance.Vars[4].Value = ((double)now.Minute).ToExp();
        instance.Vars[5].Value = ((double)now.Second).ToExp();
        instance.Vars[6].Value = ((double)now.Millisecond).ToExp();
        instance.Vars[7].Value = ((double)now.Nanosecond).ToExp();

        return Void.Return;
    }
}

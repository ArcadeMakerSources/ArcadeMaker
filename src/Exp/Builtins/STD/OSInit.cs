using System;

namespace Exp.Builtins.STD;

class OSInit
{
    [Initializes(Interpreter.STD_NAMESPACE, "OS", InitOnBuildTime = true)]
    public static BoolValue IsWindows() => OperatingSystem.IsWindows();

    [Initializes(Interpreter.STD_NAMESPACE, "OS", InitOnBuildTime = true)]
    public static BoolValue IsMacOS() => OperatingSystem.IsMacOS();

    [Initializes(Interpreter.STD_NAMESPACE, "OS", InitOnBuildTime = true)]
    public static BoolValue IsMacCatalyst() => OperatingSystem.IsMacCatalyst();

    [Initializes(Interpreter.STD_NAMESPACE, "OS", InitOnBuildTime = true)]
    public static BoolValue IsLinux() => OperatingSystem.IsLinux();

    [Initializes(Interpreter.STD_NAMESPACE, "OS", InitOnBuildTime = true)]
    public static BoolValue IsAndroid() => OperatingSystem.IsAndroid();

    [Initializes(Interpreter.STD_NAMESPACE, "OS", InitOnBuildTime = true)]
    public static BoolValue IsIOS() => OperatingSystem.IsIOS();
    
    [Initializes(Interpreter.STD_NAMESPACE, "OS", InitOnBuildTime = true)]
    public static BoolValue IsBrowser() => OperatingSystem.IsBrowser();
}

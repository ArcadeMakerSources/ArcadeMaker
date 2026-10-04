using System;
using System.Collections.Generic;

namespace ArcadeMaker.Core.Runtime;

public static class DebugConsole
{
    public const string ANDROID_DEBUG_MESSAGE_PREFIX = "[ArcadeMaker>]";

    public static event EventHandler<(object? text, TimeSpan time)>? OnDebugOutput;
    internal static readonly ManualResetEventSlim waitForDebugInput = new(false);
    private static string? lastDebugInput;
    public static Func<string?, string?>? InputValidator { get; private set; }

    internal static void WriteLine(IGame? game, object? output) => OnDebugOutput?.Invoke(game, (output, DateTime.Now.TimeOfDay));

    internal static string ReadLine(Func<string?, string?>? inputValidator = null)
    {
        InputValidator = inputValidator;
        waitForDebugInput.Wait();
        waitForDebugInput.Reset();
        InputValidator = null;
        return lastDebugInput!;
    }

    public static void SendDebugInput(string input)
    {
        lastDebugInput = input;
        waitForDebugInput.Set();
    }

    public static void SendDebugOutput(string? output)
    {
        WriteLine(null, output);
    }
}
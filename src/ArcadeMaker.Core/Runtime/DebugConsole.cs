using System;
using System.Collections.Generic;

namespace ArcadeMaker.Core.Runtime;

public static class DebugConsole
{
    public static event EventHandler<object?>? OnDebugOutput;
    internal static readonly ManualResetEventSlim waitForDebugInput = new(false);
    private static string? lastDebugInput;
    public static Func<string?, string?>? InputValidator { get; private set; }
    internal static void WriteLine(IGame game, object? output) => OnDebugOutput?.Invoke(game, output);
    internal static string ReadLine(Func<string?, string?>? inputValidator = null)
    {
        DebugConsole.InputValidator = inputValidator;
        waitForDebugInput.Wait();
        waitForDebugInput.Reset();
        DebugConsole.InputValidator = null;
        return lastDebugInput!;
    }

    public static void SendDebugInput(string input)
    {
        lastDebugInput = input;
        waitForDebugInput.Set();
    }
}
using System;
using ArcadeMaker.Core.Runtime;
using System.Diagnostics;

namespace ArcadeMaker.IDE.Debugging.Deployment;

/// <summary>
/// Allows deploying to Android devices, as long as "Developer Mode" is enabled in the Android device and it's connected to the PC,
/// and the adb tool is correctly installed in the IDE, and the debugger app is installed in the device.
/// </summary>
/// <param name="deviceName">The name of the device.</param>
/// <param name="serial">The serial string that specifies the device.</param>
class AndroidAdbDeployer(string deviceName, string serial) : IDeployer
{
    private const string DEBUGGER_APP_PACKAGE_NAME = "com.arcademaker.debugger";
    private const string REMOTE_TARGET_GAME_DATA_FILE_PATH = $"/storage/emulated/0/Android/data/{DEBUGGER_APP_PACKAGE_NAME}/gamedata.ampb";

    private Process? _logcatProcess;

    public void LaunchDebugger()
    {
        const string activityName = $"{DEBUGGER_APP_PACKAGE_NAME}.MainActivity";

        string adbArguments = $"-s {serial} shell am start -n {DEBUGGER_APP_PACKAGE_NAME}/{activityName}";

        ProcessStartInfo startInfo = new()
        {
            FileName = DeviceManager.AdbExePath,
            Arguments = adbArguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ExecuteCommand(startInfo);
        InterceptDebugLogs();
    }

    public void SetGameDataFileContent(string localFilePath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = DeviceManager.AdbExePath,
            Arguments = $"push \"{localFilePath}\" \"{REMOTE_TARGET_GAME_DATA_FILE_PATH}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ExecuteCommand(startInfo);
    }

    private void InterceptDebugLogs()
    {
        if (_logcatProcess?.HasExited == false)
            return;

        ProcessStartInfo startInfo = new()
        {
            FileName = DeviceManager.AdbExePath,
            Arguments = "logcat -s mono-stdout:V DOTNET:V",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using Process process = _logcatProcess = new() { StartInfo = startInfo };

            void OnOutput(object? s, DataReceivedEventArgs e)
            {
                if (e.Data is null)
                    return;
                
                int prefixIndex = e.Data.IndexOf(DebugConsole.ANDROID_DEBUG_MESSAGE_PREFIX);
                if (prefixIndex >= 0)
                    DebugConsole.SendDebugOutput(e.Data.Substring(prefixIndex + DebugConsole.ANDROID_DEBUG_MESSAGE_PREFIX.Length));
            }
            void OnDispose(object? s, EventArgs e)
            {
                process.OutputDataReceived -= OnOutput;
                process.Disposed -= OnDispose;
            }

            process.OutputDataReceived += OnOutput;
            process.Disposed += OnDispose;
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            DebugConsole.SendDebugOutput($"--- An {ex.GetType().Name} was thrown from Android's debug logcat process, debug logs might not being shown ---");
        }
    }

    private static void ExecuteCommand(ProcessStartInfo startInfo)
    {
        using Process process = Process.Start(startInfo) ?? throw new Exception("No process resource is started.");
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
            throw new Exception(error);
    }

    public override string ToString() => deviceName;
}

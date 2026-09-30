using System;
using System.Diagnostics;

namespace ArcadeMaker.IDE.Debugging.Deployment;

/// <summary>
/// Allows deploying to Android devices, as long as "Developer Mode" is enabled in the Android device and it's connected to the PC,
/// and the adb tool is correctly installed in the IDE, and the debugger app is installed in the device.
/// </summary>
/// <param name="deviceName">The name of the device.</param>
/// <param name="serial">The serial string that specifies the device.</param>
/// <param name="debuggerAppPackageName">The app package name of the debugger app.</param>
class AndroidAdbDeployer(string deviceName, string serial, string debuggerAppPackageName) : IDeployer
{
    private readonly string _remoteTargetGameDataFilePath = $"/storage/emulated/0/Android/data/{debuggerAppPackageName}/gamedata.ampb";
    public string DebuggerAppPackageName { get; } = debuggerAppPackageName;

    public void LaunchDebugger()
    {
        const string activityName = "com.arcademaker.debugger.MainActivity";

        string adbArguments = $"-s {serial} shell am start -n {DebuggerAppPackageName}/{activityName}";

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
    }

    public void SetGameDataFileContent(string localFilePath)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = DeviceManager.AdbExePath,
            Arguments = $"push \"{localFilePath}\" \"{_remoteTargetGameDataFilePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        ExecuteCommand(startInfo);
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

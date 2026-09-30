using System;
using System.Diagnostics;

namespace ArcadeMaker.IDE.Debugging.Deployment;

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

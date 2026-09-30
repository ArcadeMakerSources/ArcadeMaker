using System;
using System.Diagnostics;

namespace ArcadeMaker.IDE.Debugging.Deployment;

static class DeviceManager
{
    private static readonly WindowsDeployer _windowsDeployer = new();

    public static string AdbExePath => Path.Combine(AppContext.BaseDirectory, "adb\\adb.exe");

    public static async Task<IDeployer[]> GetDevicesAsync(List<string> errorLs)
    {
        List<IDeployer> deployers = [_windowsDeployer];

        if (File.Exists(AdbExePath))
            deployers.AddRange(await GetAndroidDevicesAsync(errorLs));

        return deployers.ToArray();
    }

    private static async Task<AndroidAdbDeployer[]> GetAndroidDevicesAsync(List<string> errorLs)
    {
        List<AndroidAdbDeployer> deviceSerials = [];

        ProcessStartInfo startInfo = new()
        {
            FileName = AdbExePath,
            Arguments = "devices",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using Process process = Process.Start(startInfo) ?? throw new Exception("No process resource is started.");

            // read the output line by line
            using (StreamReader reader = process.StandardOutput)
            {
                string? line;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("List of devices"))
                        continue;

                    // split by whitespace tabs or spaces
                    string[] parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 2)
                    {
                        string serial = parts[0];
                        string status = parts[1];

                        // only add the device if it is fully authorized and ready
                        if (status == "device")
                        {
                            string name = await GetAndroidDeviceMarketNameAsync(serial, errorLs);
                            deviceSerials.Add(new(name + " (Android)", serial, "com.arcademaker.debugger"));
                        }
                        else if (status == "unauthorized")
                        {
                            // TODO: handle this (user needs to approve request on phone screen)
                        }
                        else
                            System.Diagnostics.Debug.Assert(false, status);
                    }
                }
            }
            process.WaitForExit();
        }
        catch (Exception ex)
        {
            errorLs.Add("[Android: Get devices] " + ex.Message);
        }

        return deviceSerials.ToArray();
    }

    private static async Task<string> GetAndroidDeviceMarketNameAsync(string serial, List<string> errorLs)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = AdbExePath,
            Arguments = $"-s {serial} shell getprop ro.product.marketname",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            Process process = Process.Start(startInfo) ?? throw new Exception("No process resource is started.");
            string marketName = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return marketName.Trim();
        }
        catch (Exception ex)
        {
            errorLs.Add("[Android: Get device name] " + ex.Message);
            return "[Unknown Device Name]";
        }
    }
}

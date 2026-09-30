using System;
using System.Diagnostics;

namespace ArcadeMaker.IDE.Debugging.Deployment;

static class DeviceManager
{
    private static readonly WindowsDeployer _windowsDeployer = new();

    public static string AdbExePath => Path.Combine(AppContext.BaseDirectory, "adb\\adb.exe");

    public static IDeployer[] GetDevices()
    {
        List<IDeployer> deployers = [_windowsDeployer];

        deployers.AddRange(GetAndroidDevices());

        return deployers.ToArray();
    }

    private static AndroidAdbDeployer[] GetAndroidDevices()
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

        using (Process process = Process.Start(startInfo) ?? throw new Exception("No process resource is started"))
        {
            // read the output line by line
            using (StreamReader reader = process.StandardOutput)
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
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
                            string name = GetAndroidDeviceModelName(serial);
                            deviceSerials.Add(new(name, serial, "com.arcademaker.debugger"));
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

        return deviceSerials.ToArray();
    }

    private static string GetAndroidDeviceModelName(string serial)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = AdbExePath,
            Arguments = $"-s {serial} shell getprop ro.product.model",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo) ?? throw new Exception("No process resource is started."))
        {
            string modelName = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            return modelName.Trim();
        }
    }
}

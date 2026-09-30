using System;

namespace ArcadeMaker.IDE.Debugging.Deployment;

class WindowsDeployer : IDeployer
{
    private string? _gameDataFilePath;

    public void LaunchDebugger()
    {
        if (_gameDataFilePath is null)
            throw new Exception("No game data file path was set.");

        Engines.MonoGame.Platforms.WindowsDX.Program.Main([_gameDataFilePath]);
    }

    public void SetGameDataFileContent(string localFilePath)
    {
        _gameDataFilePath = localFilePath;
    }

    public override string ToString() => "This PC";
}

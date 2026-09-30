using System;

namespace ArcadeMaker.IDE.Debugging.Deployment;

interface IDeployer
{
    void LaunchDebugger();

    void SetGameDataFileContent(string localFilePath);
}

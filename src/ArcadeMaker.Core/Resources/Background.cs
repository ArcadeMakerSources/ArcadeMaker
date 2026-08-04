using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Resources;

public class Background(string name, string filePath) : ISetsID
{
    public int ID { get; } = Core.ID.Generate();

    public string Name => name;
    public string FilePath => filePath;
}
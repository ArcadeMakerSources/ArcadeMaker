using Exp;
using System.Xml.Serialization;
using ArcadeMaker.Core.Resources.Serializeables;
using ArcadeMaker.IDE.Items;

namespace ArcadeMaker.IDE.Editors.Object.ObjectProperties;

public class IDEObjectProperty(GameObject? obj) : Core.Resources.Serializeables.ObjectProperty
{
    [XmlIgnore]
    public GameObject? Object => obj;
    public IDEObjectProperty() : this(null) { } // for serializing

    public bool IsValidName(string name)
    {
        return Exp.Extensions.IsLiterallyValidName(name) && !Object.ExtraProperties.Any(p => p != this && p.Name == name) && !Core.Runtime.Instance.csProperties.Any(pair => pair.Key.Name.StartWithLowerCase() == name);
    }

    public static string GetDefaultInitVal(VariableType type)
    {
        if (type == VariableType.Bool)
            return "false";
        else if (type == VariableType.Char)
            return "'a'";
        else if (type == VariableType.Number)
            return "0";
        else if (type == VariableType.Array)
            return "[]";
        else if (type == VariableType.String)
            return "\"\"";
        else
            return "null";
    }
}

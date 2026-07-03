using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace ArcadeMaker.Core.ExpSrc;

public static class XmlDocReader
{
    public static string? GetMethodSummary(MethodInfo method)
    {
        // 1. Locate the XML file next to the method's assembly
        string assemblyPath = method.DeclaringType!.Assembly.Location;
        string xmlPath = Path.ChangeExtension(assemblyPath, ".xml");

        if (!File.Exists(xmlPath))
        {
            return "XML documentation file not found.";
        }

        try
        {
            // 2. Load the XML file into memory
            XDocument doc = XDocument.Load(xmlPath);

            // 3. Construct the compiler-generated member ID string for the method
            string memberId = GetMemberId(method);

            // 4. Query the specific member element and its summary child node
            var memberNode = doc.Descendants("member")
                                .FirstOrDefault(m => m.Attribute("name")?.Value == memberId);

            string? summary = memberNode?.Element("summary")?.Value;

            // Clean up leading/trailing whitespaces and tabs typical in multi-line comments
            return summary?.Trim();
        }
        catch (Exception ex)
        {
            return $"Error reading documentation: {ex.Message}";
        }
    }

    private static string GetMemberId(MethodInfo method)
    {
        string typeName = method.DeclaringType!.FullName!.Replace('+', '.');
        string methodName = method.Name;

        // Retrieve types of parameters to match the method signature overloads
        var parameters = method.GetParameters();
        if (parameters.Length == 0)
        {
            return $"M:{typeName}.{methodName}";
        }

        string paramTypes = string.Join(",", parameters.Select(p => p.ParameterType.FullName));
        return $"M:{typeName}.{methodName}({paramTypes})";
    }
}
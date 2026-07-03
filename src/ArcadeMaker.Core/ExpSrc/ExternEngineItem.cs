using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc;

public abstract record class ExternEngineItem(string Name, string? Desc);

public record class ExternEngineFunc(string Name, string? Desc, MethodInfo Method) : ExternEngineItem(Name, Desc);

public record class ExternEngineProperty(string Name, string? Desc, PropertyInfo Method) : ExternEngineItem(Name, Desc);
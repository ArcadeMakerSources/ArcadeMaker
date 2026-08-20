using Exp.Spans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Exp.Converting;

public interface IConvertable
{
    static abstract ClassDefSpan? Class { get; set; }
    static abstract string? Namespace { get; }
}
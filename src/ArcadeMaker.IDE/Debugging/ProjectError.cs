using Exp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.IDE.Debugging;

internal record ProjectError(string In, string Message, string File, int Line, params IEnumerable<ProjectError.Solution> Solutions)
{
    internal const string Source_Exp    = "Exp";
    internal const string Source_Engine = "Engine";

    internal ProjectError(ExpError err) : this(Source_Exp, err.Message, err.Doc, err.Line, []) { }

    internal abstract class Solution
    {
        internal string ButtonText { get; }

        internal abstract void Apply();

        internal Solution(string buttonText)
        {
            this.ButtonText = buttonText;
        }
    }
}
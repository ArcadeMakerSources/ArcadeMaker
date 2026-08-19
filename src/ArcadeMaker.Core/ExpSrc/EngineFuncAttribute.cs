using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.ExpSrc;

[AttributeUsage(AttributeTargets.Method)]
internal class EngineFuncAttribute(params int[] paramsCounts) : Exp.Converting.ExpFuncAttribute(paramsCounts)
{
    public bool IsNonStaticFuncOfGameObjects { get; init; }
}
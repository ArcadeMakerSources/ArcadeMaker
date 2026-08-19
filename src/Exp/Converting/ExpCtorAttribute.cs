using System;
using System.Collections.Generic;
using System.Text;

namespace Exp.Converting;

[AttributeUsage(AttributeTargets.Method)]
public class ExpCtorAttribute(params int[] paramOptions) : Attribute
{
    public int[] ParamOptions => paramOptions;
}
using Exp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ArcadeMaker.Core.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExpPropertyAttribute : Attribute
    {
        public Type? ValueType { get; protected set; }
    }

    public class ExpPropertyAttribute<T> : ExpPropertyAttribute where T : IValue
    {
        public ExpPropertyAttribute()
        {
            base.ValueType = typeof(T);
        }
    }
}
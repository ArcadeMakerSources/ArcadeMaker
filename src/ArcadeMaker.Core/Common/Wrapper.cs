using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace ArcadeMaker.Core.Common;

public class Wrapper<T>()
{
    public T? Value { get; set; }

    public Wrapper(T? value) : this()
    {
        this.Value = value;
    }

    public static implicit operator Wrapper<T>(T value) => new(value);
    public static implicit operator T?(Wrapper<T> wrapper) => wrapper.Value;
}
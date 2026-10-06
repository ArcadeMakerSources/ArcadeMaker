using System;
using System.Net;
using System.Web;

namespace Exp.Builtins.Web;

static partial class Impl
{
    private const string ns = "web";

    [Overrides(ns, "HttpRequest")]
    public static Instance? Wait(Instance req, IValue?[] args)
    {
        using HttpClient client = new();

        string url = req.Vars.First(v => v.Name == "url").Value?.ToString()!;
        HttpMethod method = req.Vars.First(v => v.Name == "method").Value?.Number switch
        {
            1 => HttpMethod.Get,
            2 => HttpMethod.Query,
            3 => HttpMethod.Head,
            4 => HttpMethod.Post,
            5 => HttpMethod.Put,
            6 => HttpMethod.Delete,
            7 => HttpMethod.Connect,
            8 => HttpMethod.Options,
            9 => HttpMethod.Trace,
            10 => HttpMethod.Patch,
            _ => Interpreter.Activated.ThrowRuntime<HttpMethod>("Invalid request method.", RuntimeException.INVALID_ARGUMENT)
        };
        
        using HttpRequestMessage  reqMsg   = new(method, url);
        using HttpResponseMessage response = client.Send(reqMsg);
        using Stream stream = response.Content.ReadAsStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd().ToExpString();
    }
}
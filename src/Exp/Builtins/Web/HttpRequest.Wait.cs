using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Exp;

namespace Exp.Builtins.Web;

static partial class Impl
{
    private const string ns = "web";

    [Overrides(ns, "HttpRequest")]
    public static Instance? Wait(Instance req, IValue?[] args)
    {
        using HttpClient client = new();

        string url = req.Vars.First(v => v.Name == "url").Value!.ToString()!;
        string? body = req.Vars.First(v => v.Name == "body").Value?.ToString();
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
        var headers = GetHeaders(req.Vars.First(v => v.Name == "headers").Value?.Inst);

        using StringContent? content = body == null ? null : new(body);
        using HttpRequestMessage  reqMsg   = new(method, url) { Content = content };
        foreach (var header in headers)
            reqMsg.Headers.Add(header.Key, header.Value);
        
        using HttpResponseMessage response = client.Send(reqMsg);
        using Stream stream = response.Content.ReadAsStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd().ToExpString();

        static Dictionary<string, string?[]> GetHeaders(Instance? expDictionary)
        {
            Dictionary<string, string?[]> result = [];
            if (expDictionary == null)
                return result;

            IValue?[]? keys   = expDictionary.Vars.First(v => v.Name == "keys").Value?.Inst.ArrayValues;
            IValue?[]? values = expDictionary.Vars.First(v => v.Name == "values").Value?.Inst.ArrayValues;
            if (keys == null || values == null)
                Interpreter.Activated.ThrowRuntime("web::HttpRequest.headers may only be a std::Dictionary.", RuntimeException.INVALID_ARGUMENT);

            int i = -1;
            foreach (IValue? key in keys)
            {
                i++;

                if (i >= values.Length)
                    Interpreter.Activated.ThrowRuntime("web::HttpRequest.headers had more keys than values.", RuntimeException.INVALID_OPERATION);
                else
                {
                    IValue? expVal = values[i];
                    string[] value;
                    if (expVal is ArrayInstance arr)
                        value = arr.ArrayValues.Select(arrval => arrval?.ToString() ?? "NULL").ToArray();
                    else
                        value = [expVal?.ToString() ?? "NULL"];
                    result.Add(key?.ToString() ?? "NULL", value);
                }
            }

            return result;
        }
    }
}
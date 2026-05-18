using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

static class JsonSerializer
{
    static T Deserialize<T>(Stream json)
    {
        var typeInfo = (JsonTypeInfo<T>)RuntimeJsonContext.Default.GetTypeInfo(typeof(T))!;
        return System.Text.Json.JsonSerializer.Deserialize(json, typeInfo)!;
    }

    internal static async Task<T> DeserializeAsync<T>(Stream json) => await Task.Run(() => Deserialize<T>(json));
}

[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string[]>>))]
[JsonSerializable(typeof(Promotion[]))]
[JsonSourceGenerationOptions(
    IncludeFields = true,
    PropertyNameCaseInsensitive = true,
    GenerationMode = JsonSourceGenerationMode.Metadata)]
partial class RuntimeJsonContext : JsonSerializerContext;

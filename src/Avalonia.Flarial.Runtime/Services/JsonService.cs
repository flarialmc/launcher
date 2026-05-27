using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

[JsonSerializable(typeof(Promotion[]))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string[]>>))]
sealed partial class JsonServiceContext : JsonSerializerContext;

static class JsonService
{
    static readonly JsonServiceContext s_context = new();

    public static T Read<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, s_context.Options)!;

    public static async Task<T> ReadAsync<T>(Stream stream) => (await JsonSerializer.DeserializeAsync<T>(stream, s_context.Options))!;
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

[JsonSerializable(typeof(Promotion[]))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSerializable(typeof(Dictionary<string, string>))]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string[]>>))]
sealed partial class JsonService : JsonSerializerContext;

public static class JsonServiceExtensions
{
    extension(JsonSerializerContext context)
    {
        public T Read<T>(Stream stream) => JsonSerializer.Deserialize<T>(stream, context.Options)!;

        public void Write<T>(Stream stream, T value) => JsonSerializer.Serialize(stream, value, context.Options);

        public async Task<T> ReadAsync<T>(Stream stream) => (await JsonSerializer.DeserializeAsync<T>(stream, context.Options))!;
    }
}
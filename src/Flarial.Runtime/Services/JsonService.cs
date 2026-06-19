using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

[JsonSerializable(typeof(Promotion[]), TypeInfoPropertyName = "E9E95F2BEF924613A6325AEE63DDBD01")]
[JsonSerializable(typeof(Dictionary<string, bool>), TypeInfoPropertyName = "A76FD112BCA540469477ACCC7E390017")]
[JsonSerializable(typeof(Dictionary<string, string>), TypeInfoPropertyName = "DA7263A0B74B493AA6B40C53B3BC33B0")]
[JsonSerializable(typeof(Dictionary<string, Dictionary<string, string[]>>), TypeInfoPropertyName = "C825DA81D1D848FE88B8AB77A0BFA706")]
sealed partial class JsonService : JsonSerializerContext;

public static class JsonServiceExtensions
{
    extension(JsonSerializerContext context)
    {
        public T Read<T>(Stream stream) => (T)JsonSerializer.Deserialize(stream, typeof(T), context)!;

        public void Write<T>(Stream stream, T value) => JsonSerializer.Serialize(stream, value, typeof(T), context);

        public async Task<T> ReadAsync<T>(Stream stream) => (T)(await JsonSerializer.DeserializeAsync(stream, typeof(T), context))!;
    }
}
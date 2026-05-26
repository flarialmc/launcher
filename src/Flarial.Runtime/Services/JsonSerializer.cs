using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class JsonSerializer
{
    static readonly DataContractJsonSerializerSettings s_settings;
    static readonly ConcurrentDictionary<Type, DataContractJsonSerializer> s_serializers;

    static JsonSerializer()
    {
        s_serializers = [];
        s_settings = new()
        {
            UseSimpleDictionaryFormat = true,
            MaxItemsInObjectGraph = int.MaxValue,
            EmitTypeInformation = EmitTypeInformation.Never
        };
    }

    static DataContractJsonSerializer Get<T>() => s_serializers.GetOrAdd(typeof(T), static _ => new(_, s_settings));

    public static T Deserialize<T>(Stream stream) => (T)Get<T>().ReadObject(stream);

    public static void Serialize<T>(Stream stream, T value) => Get<T>().WriteObject(stream, value);

    public static async Task<T> DeserializeAsync<T>(Stream stream) => await Task.Run(() => Deserialize<T>(stream));

    public static async Task SerializeAsync<T>(Stream stream, T value) => await Task.Run(() => Serialize(stream, value));
}
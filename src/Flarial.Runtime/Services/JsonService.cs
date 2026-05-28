using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class JsonService
{
    static readonly DataContractJsonSerializerSettings s_settings;
    static readonly ConcurrentDictionary<Type, DataContractJsonSerializer> s_serializers;

    static JsonService()
    {
        s_serializers = [];
        s_settings = new()
        {
            UseSimpleDictionaryFormat = true,
            MaxItemsInObjectGraph = int.MaxValue,
            EmitTypeInformation = EmitTypeInformation.Never
        };
    }

    static DataContractJsonSerializer Get<T>() => s_serializers.GetOrAdd(typeof(T), static type => new(type, s_settings));

    public static T Read<T>(Stream stream) => (T)Get<T>().ReadObject(stream);

    public static void Write<T>(Stream stream, T value) => Get<T>().WriteObject(stream, value);

    public static async Task<T> ReadAsync<T>(Stream stream) => await Task.Run(() => Read<T>(stream));
}
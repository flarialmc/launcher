using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

static class JsonSerializer
{
    static readonly DataContractJsonSerializerSettings s_settings;
    static readonly ConcurrentDictionary<Type, DataContractJsonSerializer> s_serializers;

    static JsonSerializer()
    {
        s_serializers = [];
        s_settings = new() { UseSimpleDictionaryFormat = true, MaxItemsInObjectGraph = int.MaxValue, EmitTypeInformation = EmitTypeInformation.Never };
    }

    static DataContractJsonSerializer Get<T>() => s_serializers.GetOrAdd(typeof(T), static _ => new(_, s_settings));

    internal static T Deserialize<T>(Stream stream) => (T)Get<T>().ReadObject(stream);

    internal static async Task<T> DeserializeAsync<T>(Stream stream) => await Task.Run(() => Deserialize<T>(stream));
}
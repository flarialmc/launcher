using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Flarial.Launcher.Runtime.Services;

sealed class JsonSerializer<T>
{
    JsonSerializer() { }

    static JsonSerializer()
    {
        s_serializers = [];
        s_lock = ((ICollection)s_serializers).SyncRoot;
        s_settings = new()
        {
            UseSimpleDictionaryFormat = true,
            MaxItemsInObjectGraph = int.MaxValue,
            EmitTypeInformation = EmitTypeInformation.Never
        };
    }

    static readonly object s_lock;
    static readonly Dictionary<Type, JsonSerializer<T>> s_serializers;
    static readonly DataContractJsonSerializerSettings s_settings = new();

    readonly DataContractJsonSerializer _serializer = new(typeof(T), s_settings);

    internal T Deserialize(Stream stream) => (T)_serializer.ReadObject(stream);

    internal static JsonSerializer<T> Get()
    {
        lock (s_lock)
        {
            var type = typeof(T);

            if (!s_serializers.TryGetValue(type, out var serializer))
            {
                serializer = new();
                s_serializers.Add(type, serializer);
            }

            return serializer;
        }
    }
}
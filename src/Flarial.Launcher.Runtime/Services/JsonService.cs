using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace Flarial.Launcher.Runtime.Services;

sealed class JsonService<T>
{
    JsonService() { }

    static readonly Dictionary<Type, JsonService<T>> s_services = [];
    static readonly DataContractJsonSerializerSettings s_settings = new()
    {
        UseSimpleDictionaryFormat = true,
        MaxItemsInObjectGraph = int.MaxValue,
        EmitTypeInformation = EmitTypeInformation.Never,
    };

    internal T ReadStream(Stream stream) => (T)_serializer.ReadObject(stream);
    readonly DataContractJsonSerializer _serializer = new(typeof(T), s_settings);

    internal static JsonService<T> GetJson()
    {
        lock (((ICollection)s_services).SyncRoot)
        {
            var type = typeof(T);

            if (!s_services.TryGetValue(type, out var service))
                s_services.Add(type, service = new());

            return service;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;

namespace Flarial.Launcher.Runtime.System;

static class JsonService
{
    static readonly Dictionary<Type, DataContractJsonSerializer> s_serializers = [];
    static readonly DataContractJsonSerializerSettings s_settings = new() { UseSimpleDictionaryFormat = true };

    internal static DataContractJsonSerializer Get<T>()
    {
        lock (s_serializers)
        {
            var type = typeof(T);

            if (!s_serializers.TryGetValue(type, out var serializer))
            {
                serializer = new(type, s_settings);
                s_serializers.Add(type, serializer);
            }

            return serializer;
        }
    }
}
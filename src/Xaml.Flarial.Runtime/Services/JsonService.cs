using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Flarial.Runtime.Services;

public static class JsonService
{
    static readonly DataContractJsonSerializerSettings s_settings = new()
    {
        IgnoreExtensionDataObject = true,
        UseSimpleDictionaryFormat = true,
        MaxItemsInObjectGraph = int.MaxValue,
        EmitTypeInformation = EmitTypeInformation.Never
    };

    static class Serializer<T>
    {
        internal static readonly DataContractJsonSerializer _ = new(typeof(T), s_settings);
    }

    public static T Read<T>(Stream stream) => (T)Serializer<T>._.ReadObject(stream);

    public static void Write<T>(Stream stream, T value) => Serializer<T>._.WriteObject(stream, value);

    public static async Task<T> ReadAsync<T>(Stream stream) => await Task.Run(() => Read<T>(stream));
}
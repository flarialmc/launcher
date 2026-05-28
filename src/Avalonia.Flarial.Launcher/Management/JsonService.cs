using System.Text.Json.Serialization;

namespace Flarial.Launcher.Management;

[JsonSerializable(typeof(AppSettings))]
sealed partial class JsonService : JsonSerializerContext;
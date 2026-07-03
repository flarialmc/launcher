using System.Text.Json.Serialization;

namespace Flarial.Launcher.Management;

[JsonSerializable(typeof(AppSettings), TypeInfoPropertyName = "BE685D530ED443F58931421005109117")]
sealed partial class JsonService : JsonSerializerContext;
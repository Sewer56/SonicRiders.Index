using System.Text.Json.Serialization;

namespace LayoutToJson.Serialization;

/// <summary>System.Object in own context; collides with Managed.Object otherwise.</summary>
[JsonSerializable(typeof(object))]
internal partial class SystemObjectJsonContext : JsonSerializerContext
{
}

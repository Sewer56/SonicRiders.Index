using System.Text.Json.Serialization;
using Sewer56.SonicRiders.Parser.Menu.Metadata.Managed;
using ManagedFrames = Sewer56.SonicRiders.Parser.Menu.Metadata.Managed.Frames;
using MetadataObject = Sewer56.SonicRiders.Parser.Menu.Metadata.Managed.Object;
using StructFrames = Sewer56.SonicRiders.Parser.Menu.Metadata.Structs.Frames;

namespace LayoutToJson.Serialization;

/// <summary>Source-gen JSON metadata for <see cref="ManagedMenuMetadata"/>; NativeAOT.</summary>
[JsonSourceGenerationOptions(IncludeFields = true, WriteIndented = true, UseStringEnumConverter = true)]
[JsonSerializable(typeof(ManagedMenuMetadata))]
// Registered explicitly: `Object` (Managed.Object) name-collides with System.Object.
[JsonSerializable(typeof(MetadataObject))]
[JsonSerializable(typeof(StructFrames.Color))]
[JsonSerializable(typeof(ManagedFrames.Unknown))]
internal partial class MenuMetadataJsonContext : JsonSerializerContext
{
}

using System.Collections.Generic;
using System.Text.Json.Serialization;
using IndexTool.Structs;
using Sewer56.SonicRiders.Parser.File.Structures;

namespace IndexTool.Serialization;

/// <summary>Source-gen JSON metadata; reflection serializer fails under NativeAOT.</summary>
[JsonSourceGenerationOptions(IncludeFields = true, WriteIndented = true)]
[JsonSerializable(typeof(List<FileTypeEx>))]
[JsonSerializable(typeof(List<FileType>))]
[JsonSerializable(typeof(List<InternalFileType>))]
[JsonSerializable(typeof(List<DataFile>))]
[JsonSerializable(typeof(FileType))]
internal partial class IndexToolJsonContext : JsonSerializerContext
{
}

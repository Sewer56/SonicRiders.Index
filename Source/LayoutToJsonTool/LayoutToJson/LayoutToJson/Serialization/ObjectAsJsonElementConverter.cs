using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LayoutToJson.Serialization;

/// <summary>Dictionary object values as JsonElement; converted by Keyframe.Initialize. NativeAOT.</summary>
public class ObjectAsJsonElementConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonElement.ParseValue(ref reader);

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is JsonElement e)
            e.WriteTo(writer);
        else
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}

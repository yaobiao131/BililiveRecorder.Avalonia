using Newtonsoft.Json;

namespace BililiveRecorder.Common;

public class NestedStringConverter<T> : JsonConverter<T> where T : new()
{
    public override void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer)
    {
        writer.WriteValue(JsonConvert.SerializeObject(value));
    }

    public override T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        return reader.Value == null ? new T() : JsonConvert.DeserializeObject<T>(reader.Value.ToString()!);
    }
}

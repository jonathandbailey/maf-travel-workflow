using Microsoft.Agents.AI.Workflows;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Application.Workflows.Conversations;

/// <summary>
/// Custom JSON converter for serializing/deserializing Dictionary&lt;CheckpointInfo, JsonElement&gt;.
/// </summary>
public class CheckpointDictionaryConverter : JsonConverter<Dictionary<CheckpointInfo, JsonElement>>
{
    public override Dictionary<CheckpointInfo, JsonElement> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<CheckpointInfo, JsonElement>();

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token");
        }

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Expected StartObject token");
            }

            CheckpointInfo? key = null;
            JsonElement? value = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    break;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("Expected PropertyName token");
                }

                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName)
                {
                    case "Key":
                        key = JsonSerializer.Deserialize<CheckpointInfo>(ref reader, options);
                        break;
                    case "Value":
                        value = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                        break;
                }
            }

            if (key is not null && value.HasValue)
            {
                dictionary.Add(key, value.Value);
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, Dictionary<CheckpointInfo, JsonElement> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var kvp in value)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Key");
            JsonSerializer.Serialize(writer, kvp.Key, options);
            writer.WritePropertyName("Value");
            JsonSerializer.Serialize(writer, kvp.Value, options);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }
}

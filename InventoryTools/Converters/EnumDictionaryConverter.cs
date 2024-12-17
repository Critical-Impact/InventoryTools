using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InventoryTools.Converters;

public class EnumDictionaryConverter : JsonConverter<Dictionary<string, Enum>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<string, Enum> value, JsonSerializer serializer)
    {
        writer.WriteStartObject();
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);
            writer.WriteStartObject();
            writer.WritePropertyName("Type");
            writer.WriteValue(kvp.Value.GetType().FullName);
            writer.WritePropertyName("Assembly");
            writer.WriteValue(kvp.Value.GetType().Assembly.GetName().Name);
            writer.WritePropertyName("Value");
            writer.WriteValue(kvp.Value.ToString());
            writer.WriteEndObject();
        }
        writer.WriteEndObject();
    }

    public override Dictionary<string, Enum> ReadJson(JsonReader reader, Type objectType, Dictionary<string, Enum> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = new Dictionary<string, Enum>();
        var jsonObject = JObject.Load(reader);

        foreach (var property in jsonObject.Properties())
        {
            string key = property.Name;
            var typeString = property.Value["Type"]?.ToString();
            var assemblyString = property.Value["Assembly"]?.ToString();
            var valueString = property.Value["Value"]?.ToString();

            Type enumType = AppDomain.CurrentDomain.GetAssemblies().First(c => c.GetName().Name == assemblyString).GetType(typeString);
            if (enumType == null)
            {
                throw new JsonSerializationException($"Unknown type: {typeString}");
            }

            var enumValue = Enum.Parse(enumType, valueString);
            result[key] = (Enum)enumValue;
        }

        return result;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InventoryTools.Converters;

public class TypeDictionaryConverter : JsonConverter<Dictionary<string, Dictionary<Type, bool>>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<string, Dictionary<Type, bool>> value, JsonSerializer serializer)
    {
        var jObject = new JObject();
        foreach (var outerPair in value)
        {
            var innerObject = new JObject();
            foreach (var innerPair in outerPair.Value)
            {
                innerObject[$"{innerPair.Key.FullName}, {innerPair.Key.Assembly.GetName().Name}"] = innerPair.Value;
            }
            jObject[outerPair.Key] = innerObject;
        }
        jObject.WriteTo(writer);
    }

    public override Dictionary<string, Dictionary<Type, bool>> ReadJson(JsonReader reader, Type objectType, Dictionary<string, Dictionary<Type, bool>> existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        var result = new Dictionary<string, Dictionary<Type, bool>>();
        var jObject = JObject.Load(reader);

        foreach (var outerProperty in jObject.Properties())
        {
            var innerDict = new Dictionary<Type, bool>();
            foreach (var innerProperty in ((JObject)outerProperty.Value).Properties())
            {
                var type = Type.GetType(innerProperty.Name);
                if (type != null)
                {
                    innerDict[type] = innerProperty.Value.ToObject<bool>();
                }
            }
            result[outerProperty.Name] = innerDict;
        }

        return result;
    }
}
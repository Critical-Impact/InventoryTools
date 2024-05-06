using System;
using System.Collections.Generic;
using InventoryTools.Logic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace InventoryTools.Converters;

class ColumnConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(List<ColumnConfiguration>);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        
        if (token.Type == JTokenType.Array)
        {
            var list = new List<ColumnConfiguration>();
            foreach (var item in token)
            {
                if (item.Type == JTokenType.String)
                {
                    // Convert string to MyClass instance
                    var columnName = item.Value<string>();
                    if (columnName != null)
                    {
                        var filterConfigurationColumn = new ColumnConfiguration(columnName);
                        list.Add(filterConfigurationColumn);
                    }
                }
                else if (item.Type == JTokenType.Object)
                {
                    var filterConfigurationColumn = item.ToObject<ColumnConfiguration>();
                    if (filterConfigurationColumn != null)
                    {
                        list.Add(filterConfigurationColumn);
                    }
                }
            }
            return list;
        }
        return new List<ColumnConfiguration>();
    }

    public override bool CanWrite => true;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}
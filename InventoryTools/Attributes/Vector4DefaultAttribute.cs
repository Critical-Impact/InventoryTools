using System.ComponentModel;
using System.Globalization;
using System.Numerics;

namespace InventoryTools.Attributes;

public class Vector4DefaultAttribute : DefaultValueAttribute
{
    public Vector4DefaultAttribute(string json) : base (ConvertFromJson(json))
    {            
    }

    private static object ConvertFromJson(string json)
    {
        var parts = json.Split(",");
        var x = float.Parse(parts[0], CultureInfo.InvariantCulture);
        var y = float.Parse(parts[1], CultureInfo.InvariantCulture);
        var z = float.Parse(parts[2], CultureInfo.InvariantCulture);
        var w = float.Parse(parts[3], CultureInfo.InvariantCulture);
        return new Vector4(x, y, z, w);
    }
}
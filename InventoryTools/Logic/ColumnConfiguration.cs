using System;
using System.Collections.Generic;
using System.Reflection;
using InventoryTools.Logic.Columns;
using Newtonsoft.Json;

namespace InventoryTools.Logic;

public class ColumnConfiguration
{
    private string _columnName;
    private string _key;
    private string? _name;
    private string? _exportName;

    public string ColumnName
    {
        get => _columnName;
        set => _columnName = value;
    }

    public string Key
    {
        get => _key;
        set => _key = value;
    }

    public string? Name
    {
        get => _name;
        set => _name = value;
    }

    public string? ExportName
    {
        get => _exportName;
        set => _exportName = value;
    }

    private Dictionary<string, string>? _stringSettings;
    private Dictionary<string, uint>? _uintSettings;

    public void SetSetting(string key, string? value)
    {
        if (value == null)
        {
            StringSettings.Remove(key);
        }
        else
        {
            StringSettings[key] = value;
        }

    }

    public void SetSetting(string key, uint? value)
    {
        if (value == null)
        {
            UintSettings.Remove(key);
        }
        else
        {
            UintSettings[key] = value.Value;
        }
    }

    public void GetSetting(string key, out string? value)
    {
        value = StringSettings.ContainsKey(key) ? StringSettings[key] : null;
    }

    public void GetSetting(string key, out uint? value)
    {
        value = UintSettings.ContainsKey(key) ? UintSettings[key] : null;
    }

    public ColumnConfiguration(string columnName)
    {
        _columnName = columnName;
        _key = Guid.NewGuid().ToString("N");
    }

    public ColumnConfiguration()
    {
        
    }

    [JsonIgnore]
    public IColumn Column
    {
        get;
        set;
    }

    public Dictionary<string, string> StringSettings
    {
        get => _stringSettings ??= new Dictionary<string, string>();
        set => _stringSettings = value;
    }

    public Dictionary<string, uint> UintSettings
    {
        get => _uintSettings ??= new Dictionary<string, uint>();
        set => _uintSettings = value;
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using CharacterTools.Logic.Editors;
using Dalamud.Game.Text;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Editors;
using Newtonsoft.Json;

namespace InventoryTools.Logic;

public class ColumnConfiguration
{
    private string _columnName;
    private string _key;
    private string? _name;
    private string? _exportName;
    private bool _hiddenImGui;

    public bool IsDirty { get; set; }

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

    //Used to store the filtering configuration for a column, not persisted
    [JsonIgnore]
    public ColumnConfiguration FilterConfiguration
    {
        get
        {
            return _columnConfiguration ??= new ColumnConfiguration();
        }
        set => _columnConfiguration = value;
    }

    private Dictionary<string, string>? _stringSettings;
    private Dictionary<string, uint>? _uintSettings;
    private Dictionary<string, ulong>? _ulongSettings;
    private Dictionary<string, List<ItemInfoType>>? _itemInfoTypes;
    private Dictionary<string, List<InventorySearchScope>>? _inventorySearchScopes;
    private Dictionary<string, List<CharacterSearchScope>>? _characterSearchScopes;
    private Dictionary<string, List<int>>? _intListSettings;

    [JsonIgnore]
    private IColumn _column;

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

    public void SetSetting(string key, ulong? value)
    {
        if (value == null)
        {
            UlongSettings.Remove(key);
        }
        else
        {
            UlongSettings[key] = value.Value;
        }
    }

    public void SetSetting(string key, List<InventorySearchScope>? value)
    {
        if (value == null)
        {
            InventorySearchScopes.Remove(key);
        }
        else
        {
            InventorySearchScopes[key] = value;
        }
    }

    public void SetSetting(string key, List<CharacterSearchScope>? value)
    {
        if (value == null)
        {
            CharacterSearchScopes.Remove(key);
        }
        else
        {
            CharacterSearchScopes[key] = value;
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

    public void GetSetting(string key, out ulong? value)
    {
        value = UlongSettings.ContainsKey(key) ? UlongSettings[key] : null;
    }

    public void GetSetting<T>(string key, out List<T>? value) where T : Enum
    {
        value = IntListSettings.ContainsKey(key) ? IntListSettings[key].Select(c => (T)Enum.ToObject(typeof(T), c)).ToList() : null;
    }

    public void GetSetting(string key, out List<ItemInfoType>? value)
    {
        value = ItemInfoTypes.ContainsKey(key) ? ItemInfoTypes[key] : null;
    }

    public void GetSetting(string key, out List<InventorySearchScope>? value)
    {
        value = InventorySearchScopes.ContainsKey(key) ? InventorySearchScopes[key] : null;
    }

    public void GetSetting(string key, out List<CharacterSearchScope>? value)
    {
        value = CharacterSearchScopes.ContainsKey(key) ? CharacterSearchScopes[key] : null;
    }

    public void SetSetting(string key, List<ItemInfoType>? value)
    {
        if (value == null)
        {
            ItemInfoTypes.Remove(key);
        }
        else
        {
            ItemInfoTypes[key] = value;
        }
    }

    public void SetSetting<T>(string key, List<T>? value) where T : Enum
    {
        if (value == null)
        {
            IntListSettings.Remove(key);
        }
        else
        {
            IntListSettings[key] = value.Select(c => (int)(object)c).ToList();
        }
    }

    public ColumnConfiguration(string columnName)
    {
        _columnName = columnName;
        _key = Guid.NewGuid().ToString("N");
    }

    public ColumnConfiguration()
    {

    }

    private string _filterText = "";

    [JsonIgnore]
    public string FilterText
    {
        get => _filterText;
        set
        {
            _filterText = value.Replace((char)SeIconChar.Collectible,  ' ').Replace((char)SeIconChar.HighQuality, ' ');
            _filterComparisonText = new FilterComparisonExtensions.FilterComparisonText(_filterText);
        }
    }

    private FilterComparisonExtensions.FilterComparisonText? _filterComparisonText;
    private ColumnConfiguration? _columnConfiguration;

    [JsonIgnore]
    public FilterComparisonExtensions.FilterComparisonText FilterComparisonText
    {
        get
        {
            if (_filterComparisonText == null)
            {
                _filterComparisonText = new FilterComparisonExtensions.FilterComparisonText(FilterText);
            }

            return _filterComparisonText;
        }
    }

    [JsonIgnore]
    public IColumn Column
    {
        get => _column;
        set => _column = value;
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

    public Dictionary<string, ulong> UlongSettings
    {
        get => _ulongSettings ??= new Dictionary<string, ulong>();
        set => _ulongSettings = value;
    }

    public Dictionary<string, List<ItemInfoType>> ItemInfoTypes
    {
        get => _itemInfoTypes ??= new Dictionary<string, List<ItemInfoType>>();
        set => _itemInfoTypes = value;
    }

    public Dictionary<string, List<int>> IntListSettings
    {
        get => _intListSettings ??= new Dictionary<string, List<int>>();
        set => _intListSettings = value;
    }

    public Dictionary<string, List<InventorySearchScope>> InventorySearchScopes
    {
        get => _inventorySearchScopes ??= new Dictionary<string, List<InventorySearchScope>>();
        set => _inventorySearchScopes = value;
    }

    public Dictionary<string, List<CharacterSearchScope>> CharacterSearchScopes
    {
        get => _characterSearchScopes ??= new Dictionary<string, List<CharacterSearchScope>>();
        set => _characterSearchScopes = value;
    }

}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using AllaganLib.GameSheets.Caches;
using AllaganLib.Interface.Converters;
using AllaganLib.Interface.FormFields;

namespace InventoryTools.EquipmentSuggest;

public class EquipmentSuggestConfig : INotifyPropertyChanged, AllaganLib.Interface.FormFields.IConfigurable<string?>, AllaganLib.Interface.FormFields.IConfigurable<uint>, AllaganLib.Interface.FormFields.IConfigurable<List<ItemInfoType>?>, AllaganLib.Interface.FormFields.IConfigurable<int?>, IConfigurable<bool?>, IConfigurable<Enum?>
{
    private readonly Dictionary<string, string> _stringFilters = [];
    private readonly Dictionary<string, uint> _uintFilters = [];
    private readonly Dictionary<string, int> _intFilters = [];
    private readonly Dictionary<string, bool> _boolFilters = [];
    private readonly Dictionary<string, List<ItemInfoType>> _itemInfoTypesFilters = [];
    private Dictionary<string, Enum> _enumSettings = [];
    private bool _isDirty = false;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    [JsonConverter(typeof(EnumDictionaryConverter))]
    public Dictionary<string, Enum> EnumSettings
    {
        get => this._enumSettings;
        set => this._enumSettings = value;
    }

    public bool IsDirty
    {
        get => _isDirty;
        set
        {
            _isDirty = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsDirty"));
            _isDirty = false;
        }
    }

    public string? Get(string key)
    {
        return this._stringFilters.GetValueOrDefault(key);
    }

    public void Set(string key, bool? newValue)
    {
        if (newValue == null)
        {
            this._boolFilters.Remove(key);
        }
        else
        {
            this._boolFilters[key] = newValue.Value;
        }

        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    public void Set(string key, int? newValue)
    {
        if (newValue == null)
        {
            this._intFilters.Remove(key);
        }
        else
        {
            this._intFilters[key] = newValue.Value;
        }

        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    public void Set(string key, List<ItemInfoType>? newValue)
    {
        if (newValue == null)
        {
            this._itemInfoTypesFilters.Remove(key);
        }
        else
        {
            this._itemInfoTypesFilters[key] = newValue;
        }

        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    public void Set(string key, Enum? newValue)
    {
        if (newValue == null)
        {
            this.EnumSettings.Remove(key);
        }
        else
        {
            this.EnumSettings[key] = newValue;
        }

        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    public void Set(string key, uint newValue)
    {
        if (newValue == 0)
        {
            this._uintFilters.Remove(key);
        }
        else
        {
            this._uintFilters[key] = newValue;
        }

        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    public void Set(string key, string? newValue)
    {
        if (newValue == null)
        {
            this._stringFilters.Remove(key);
        }
        else
        {
            this._stringFilters[key] = newValue;
        }

        this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
    }

    Enum? IConfigurable<Enum?>.Get(string key)
    {
        return this.EnumSettings.GetValueOrDefault(key);
    }

    uint AllaganLib.Interface.FormFields.IConfigurable<uint>.Get(string key)
    {
        return this._uintFilters.GetValueOrDefault(key);
    }

    List<ItemInfoType>? AllaganLib.Interface.FormFields.IConfigurable<List<ItemInfoType>?>.Get(string key)
    {
        return this._itemInfoTypesFilters.GetValueOrDefault(key);
    }

    int? AllaganLib.Interface.FormFields.IConfigurable<int?>.Get(string key)
    {
        return this._intFilters.TryGetValue(key, out var value) ? value : null;
    }

    bool? IConfigurable<bool?>.Get(string key)
    {
        return this._boolFilters.TryGetValue(key, out var value) ? value : null;
    }
}
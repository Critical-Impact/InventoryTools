using System;
using System.Collections.Generic;
using AllaganLib.GameSheets.Caches;
using Dalamud.Game.Text;
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

    private Dictionary<string, string>? _stringSettings;
    private Dictionary<string, uint>? _uintSettings;
    private Dictionary<string, List<ItemInfoType>>? _itemInfoTypes;
    private Dictionary<string, List<InventorySearchScope>>? _inventorySearchScopes;

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

    public void GetSetting(string key, out string? value)
    {
        value = StringSettings.ContainsKey(key) ? StringSettings[key] : null;
    }

    public void GetSetting(string key, out uint? value)
    {
        value = UintSettings.ContainsKey(key) ? UintSettings[key] : null;
    }

    public void GetSetting(string key, out List<ItemInfoType>? value)
    {
        value = ItemInfoTypes.ContainsKey(key) ? ItemInfoTypes[key] : null;
    }

    public void GetSetting(string key, out List<InventorySearchScope>? value)
    {
        value = InventorySearchScopes.ContainsKey(key) ? InventorySearchScopes[key] : null;
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

    public virtual bool DrawFilter(string tableKey, int columnIndex)
    {
        if (Column.FilterType == ColumnFilterType.Text)
        {
            var filter = FilterText;
            var hasChanged = false;

            ImGui.TableSetColumnIndex(columnIndex);
            ImGui.PushItemWidth(-20.000000f);
            ImGui.PushID(Column.Name);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
            ImGui.InputText("##" + tableKey + "FilterI" + Column.Name, ref filter, Column.MaxFilterLength);
            ImGui.PopStyleVar();
            ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGui.TableHeader("");
            ImGui.PopID();
            ImGui.PopItemWidth();
            if (filter != FilterText)
            {
                FilterText = filter;
                hasChanged = true;
            }

            return hasChanged;
        }
        else if (Column.FilterType == ColumnFilterType.Choice)
        {
            var hasChanged = false;
            ImGui.TableSetColumnIndex(columnIndex);
            ImGui.PushItemWidth(-20.000000f);
            using (ImRaii.PushId(Column.Name))
            {
                using (ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(0, 0)))
                {

                    var currentItem = FilterText;

                    using (var combo = ImRaii.Combo("##Choice", currentItem))
                    {
                        if (combo.Success)
                        {
                            if (Column.FilterChoices != null)
                            {
                                if (ImGui.Selectable("", false))
                                {
                                    FilterText = "";
                                    hasChanged = true;
                                }

                                foreach (var column in Column.FilterChoices)
                                {
                                    if (ImGui.Selectable(column, currentItem == column))
                                    {
                                        FilterText = column;
                                        hasChanged = true;
                                    }
                                }
                            }
                        }
                    }
                }

                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.TableHeader("");
            }
            ImGui.PopItemWidth();
            return hasChanged;
        }
        else if (Column.FilterType == ColumnFilterType.Boolean)
        {
            var hasChanged = false;
            ImGui.TableSetColumnIndex(columnIndex);
            ImGui.PushItemWidth(-20.000000f);
            using (ImRaii.PushId(Column.Name))
            {
                using (ImRaii.PushStyle(ImGuiStyleVar.FramePadding, new Vector2(0, 0)))
                {

                    var currentItem = FilterText;

                    if (currentItem == "true")
                    {
                        currentItem = "Yes";
                    }
                    else if(currentItem == "false")
                    {
                        currentItem = "No";
                    }

                    using (var combo = ImRaii.Combo("##Choice", currentItem))
                    {
                        if (combo.Success)
                        {
                            if (ImGui.Selectable("", false))
                            {
                                FilterText = "";
                                hasChanged = true;
                            }



                            if (ImGui.Selectable("Yes", currentItem == "Yes"))
                            {
                                FilterText = "true";
                                hasChanged = true;
                            }

                            if (ImGui.Selectable("No", currentItem == "No"))
                            {
                                FilterText = "false";
                                hasChanged = true;
                            }
                        }
                    }
                }

                ImGui.SameLine(0.0f, ImGui.GetStyle().ItemInnerSpacing.X);
                ImGui.TableHeader("");
            }
            ImGui.PopItemWidth();
            return hasChanged;
        }

        return false;
    }

    private FilterComparisonExtensions.FilterComparisonText? _filterComparisonText;

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

    public Dictionary<string, List<ItemInfoType>> ItemInfoTypes
    {
        get => _itemInfoTypes ??= new Dictionary<string, List<ItemInfoType>>();
        set => _itemInfoTypes = value;
    }

    public Dictionary<string, List<InventorySearchScope>> InventorySearchScopes
    {
        get => _inventorySearchScopes ??= new Dictionary<string, List<InventorySearchScope>>();
        set => _inventorySearchScopes = value;
    }

}
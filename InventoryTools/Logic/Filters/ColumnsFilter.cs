using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Filters.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters
{
    public class ColumnsFilter : SortedListFilter<ColumnConfiguration, IColumn>
    {
        private readonly IEnumerable<IColumn> _columns;

        public ColumnsFilter(ILogger<ColumnsFilter> logger, ImGuiService imGuiService, IEnumerable<IColumn> columns) : base(logger, imGuiService)
        {
            _columns = columns;
        }
        public override Dictionary<ColumnConfiguration, (string, string?)> CurrentValue(FilterConfiguration configuration)
        {
            (string, string?) GetColumnDetails(ColumnConfiguration c)
            {
                return (c.Name ?? c.Column.Name, c.Column.HelpText);
            }

            return (configuration.Columns ?? new List<ColumnConfiguration>()).ToDictionary(c => c, GetColumnDetails);
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new Dictionary<ColumnConfiguration, (string, string?)>());
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<ColumnConfiguration, (string, string?)> newValue)
        {
            configuration.Columns = newValue.Select(c => c.Key).ToList();
        }

        public override string Key { get; set; } = "Columns";
        public override string Name { get; set; } = "Columns";
        public override string HelpText { get; set; } = "Add a new column. Leave the column name blank if you want to use the default.";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Columns;
        public override bool ShowReset { get; set; } = false;
        public override Dictionary<ColumnConfiguration, (string, string?)> DefaultValue { get; set; } = new();

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.Columns != null && configuration.Columns.Count != 0;
        }

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.CraftFilter | FilterType.HistoryFilter;
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public override bool CanRemove { get; set; } = true;

        public override bool CanRemoveItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            if (item.Column != null)
            {
                if (!item.Column.CanBeRemoved)
                {
                    return false;
                }
            }

            return true;
        }

        public override IColumn? GetItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            return item.Column;
        }

        public void AddItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            var value = CurrentValue(configuration);
            value.Add(item, ("", null));
            UpdateFilterConfiguration(configuration, value);
        }

        public Dictionary<string, IColumn> GetAvailableItems(FilterConfiguration configuration)
        {
            var value = _columns;
            return value.Where(c => c.CraftOnly != true && c.AvailableInType(configuration.FilterType)).ToDictionary(c => c.GetType().ToString(), c => c);
        }

        private List<IGrouping<ColumnCategory, KeyValuePair<string, IColumn>>>? _groupedItems;
        public List<IGrouping<ColumnCategory, KeyValuePair<string, IColumn>>> GetGroupedItems(FilterConfiguration configuration)
        {
            var availableItems = GetAvailableItems(configuration).OrderBy(c => c.Value.RenderName ?? c.Value.Name);
            if (_groupedItems == null)
            {
                _groupedItems = availableItems.GroupBy(c => c.Value.ColumnCategory).ToList();
            }

            return _groupedItems;
        }

        private string _selectedColumnKey = "";
        private string _selectedColumnName = "";
        private string _customName = "";
        private string _exportName = "";
        private bool _editMode = false;
        private ColumnConfiguration? _selectedColumn;

        public override void DrawItem(FilterConfiguration configuration, KeyValuePair<ColumnConfiguration, (string, string?)> item, int index)
        {
            base.DrawItem(configuration, item, index);
            if (item.Key.Name != null)
            {
                ImGui.SameLine();
                ImGuiService.HelpMarker("Original Column Name: " + item.Key.Column.Name);
            }
        }

        public override void DrawButtons(FilterConfiguration configuration, KeyValuePair<ColumnConfiguration, (string, string?)> item, int index)
        {
            base.DrawButtons(configuration, item, index);
            ImGui.SameLine();
            if (ImGui.Button("Edit##Column" + index))
            {
                EditItem(configuration, item.Key);
            }
        }

        private void EditItem(FilterConfiguration configuration, ColumnConfiguration item)
        {
            _editMode = true;
            _selectedColumn = item;
            _selectedColumnKey = item.Key;
            _selectedColumnName = item.Column.Name;
            _customName = item.Name ?? "";
            _exportName = item.ExportName ?? "";
        }

        public override void DrawTable(FilterConfiguration configuration)
        {
            var groupedItems = GetGroupedItems(configuration);
            base.DrawTable(configuration);
            
            var currentAddColumn = _selectedColumnName;
            ImGui.Separator();
            ImGui.SetNextItemWidth(LabelSize);
            ImGui.LabelText("##" + Key + "Label",  _editMode ? "Edit column: " : "Add new column: ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(InputSize);
            if (_editMode)
            {
                ImGui.Text(_selectedColumnName);
            }
            else
            {
                using (var combo = ImRaii.Combo("##Add" + Key, currentAddColumn, ImGuiComboFlags.HeightLarge))
                {
                    if (combo.Success)
                    {
                        var count = 0;
                        foreach (var group in groupedItems)
                        {
                            ImGui.TextUnformatted(group.Key.ToString());
                            ImGui.Separator();
                            foreach (var column in group)
                            {
                                if (ImGui.Selectable(column.Value.Name, currentAddColumn == column.Value.Name))
                                {
                                    _selectedColumnName = column.Value.Name;
                                    _selectedColumnKey = column.Key;
                                    _selectedColumn = new ColumnConfiguration(column.Key);
                                    _selectedColumn.Column = column.Value;
                                    _customName = "";
                                    _exportName = "";
                                }

                                ImGuiUtil.HoverTooltip(column.Value.HelpText);
                            }

                            count++;
                            if (count != groupedItems.Count)
                            {
                                ImGui.NewLine();
                            }

                        }
                    }
                }
            }

            if (_selectedColumnKey != "")
            {
                string customName = _customName;
                ImGui.SetNextItemWidth(LabelSize);
                ImGui.LabelText("##" + Key + "Custom", "Column Name: ");
                ImGui.SetNextItemWidth(InputSize);
                ImGui.SameLine();
                if (ImGui.InputTextWithHint("##CustomColumnName",_selectedColumnName, ref customName, 100, ImGuiInputTextFlags.None))
                {
                    _customName = customName;
                }
                string exportName = _exportName;
                ImGui.SetNextItemWidth(LabelSize);
                ImGui.LabelText("##" + Key + "Export", "Export Name: ");
                ImGui.SetNextItemWidth(InputSize);
                ImGui.SameLine();
                if (ImGui.InputTextWithHint("##CustomExportName",_selectedColumnName, ref exportName, 100, ImGuiInputTextFlags.None))
                {
                    _exportName = exportName;
                }
                ImGui.SameLine();
                var posX = ImGui.GetCursorPosX();

                if (_selectedColumn != null)
                {
                    _selectedColumn.Column.DrawEditor(_selectedColumn, configuration);
                }
                
                
                ImGui.NewLine();
                ImGui.SetCursorPosX(posX - ImGui.GetStyle().ItemSpacing.X - 40);
                if (ImGui.Button(_editMode ? "Save" : "Add", new Vector2(40, 20)))
                {
                    if (_editMode)
                    {
                        var columnConfiguration = configuration.GetColumn(_selectedColumnKey);
                        if (columnConfiguration != null)
                        {
                            columnConfiguration.Name = _customName == "" ? null : _customName;
                            columnConfiguration.ExportName = _exportName == "" ? null : _exportName;
                            UpdateFilterConfiguration(configuration, CurrentValue(configuration));
                        }

                        _selectedColumnName = "";
                        _selectedColumnKey = "";
                        _customName = "";
                        _editMode = false;
                    }
                    else
                    {
                        var columnConfiguration = _selectedColumn ?? new ColumnConfiguration(_selectedColumnKey);
                        columnConfiguration.Name = _customName == "" ? null : _customName;
                        columnConfiguration.ExportName = _exportName == "" ? null : _exportName;
                        AddItem(configuration, columnConfiguration);

                        _selectedColumnName = "";
                        _selectedColumnKey = "";
                        _customName = "";
                    }
                };
            }
        }

        public override void Draw(FilterConfiguration configuration)
        {
            base.Draw(configuration);
            ImGui.Separator();
        }
    }
}
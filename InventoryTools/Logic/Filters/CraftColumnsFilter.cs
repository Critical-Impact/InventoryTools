using System.Collections.Generic;
using System.Linq;
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
    public class CraftColumnsFilter : SortedListFilter<ColumnConfiguration, IColumn>
    {
        private readonly IEnumerable<IColumn> _columns;

        public CraftColumnsFilter(ILogger<CraftColumnsFilter> logger, ImGuiService imGuiService, IEnumerable<IColumn> columns) : base(logger, imGuiService)
        {
            _columns = columns;
        }
        public override Dictionary<ColumnConfiguration, (string, string?)> CurrentValue(FilterConfiguration configuration)
        {
            (string, string?) GetColumnDetails(ColumnConfiguration c)
            {
                return c.Column != null ? (c.Column.Name, c.Column.HelpText): (c.ColumnName, null);
            }

            return (configuration.CraftColumns ?? new List<ColumnConfiguration>()).ToDictionary(c => c, GetColumnDetails);
        }
        

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<ColumnConfiguration, (string, string?)> newValue)
        {
            configuration.CraftColumns = newValue.Select(c => c.Key).ToList();
            _availableItems = null;
            _allItems = null;
            _groupedItems = null;
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new Dictionary<ColumnConfiguration, (string, string?)>());
        }

        public override string Key { get; set; } = "Craft Columns";
        public override string Name { get; set; } = "Craft Columns";
        public override string HelpText { get; set; } = "";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.CraftColumns;
        public override bool ShowReset { get; set; } = false;
        public override Dictionary<ColumnConfiguration, (string, string?)> DefaultValue { get; set; } = new();

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.CraftColumns != null && configuration.CraftColumns.Count != 0;
        }

        public override FilterType AvailableIn { get; set; } = FilterType.CraftFilter;
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
            var column = GetItem(configuration, item);
            if (column != null)
            {
                if (!column.CanBeRemoved)
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

        public void AddItem(FilterConfiguration configuration, string item)
        {
            var value = CurrentValue(configuration);
            value.Add(new ColumnConfiguration(item), ("", null));
            UpdateFilterConfiguration(configuration, value);
        }

        private Dictionary<string, IColumn>? _availableItems;

        public Dictionary<string, IColumn> GetAvailableItems(FilterConfiguration configuration)
        {
            //TODO: Fix this so that it invalidates per filter
            if (_availableItems == null)
            {
                var value = _columns;
                var currentValue = CurrentValue(configuration);
                _availableItems = value.Where(c => c.CraftOnly != false && c.AvailableInType(configuration.FilterType)).ToDictionary(c => c.GetType().ToString(), c => c);
            }

            return _availableItems;
        }

        private Dictionary<string, IColumn>? _allItems;

        public Dictionary<string, IColumn> GetAllItems(FilterConfiguration configuration)
        {
            if (_allItems == null)
            {
                var value = _columns;
                _allItems = value.Where(c => c.CraftOnly != false && c.AvailableInType(configuration.FilterType)).ToDictionary(c => c.GetType().ToString(), c => c);
            }

            return _allItems;
        }

        private List<IGrouping<ColumnCategory, KeyValuePair<string, IColumn>>>? _groupedItems;
        public List<IGrouping<ColumnCategory, KeyValuePair<string, IColumn>>> GetGroupedItems(FilterConfiguration configuration)
        {
            var availableItems = GetAvailableItems(configuration).OrderBy(c => c.Value.RenderName ?? c.Value.Name);
            _groupedItems = availableItems.GroupBy(c => c.Value.ColumnCategory).ToList();

            return _groupedItems;
        }

        public override void DrawTable(FilterConfiguration configuration)
        {
            var groupedItems = GetGroupedItems(configuration);
            base.DrawTable(configuration);
            
            var currentAddColumn = "";
            ImGui.SetNextItemWidth(LabelSize);
            ImGui.LabelText("##" + Key + "Label", "Add new column: ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(InputSize);
            using (var combo = ImRaii.Combo("Add##" + Key, currentAddColumn, ImGuiComboFlags.HeightLarge))
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
                                AddItem(configuration, column.Key);
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
    }
}
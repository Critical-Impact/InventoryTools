using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Filters.Abstract;
using OtterGui;
using OtterGui.Raii;

namespace InventoryTools.Logic.Filters
{
    public class ColumnsFilter : SortedListFilter<string>
    {
        public override Dictionary<string, (string, string?)> CurrentValue(FilterConfiguration configuration)
        {
            (string, string?) GetColumnDetails(string c)
            {
                return PluginService.PluginLogic.GridColumns.ContainsKey(c) ? (PluginService.PluginLogic.GridColumns[c].Name, PluginService.PluginLogic.GridColumns[c].HelpText): (c, null);
            }

            return (configuration.Columns ?? new List<string>()).ToDictionary(c => c, GetColumnDetails);
        }
        
        public override void ResetFilter(FilterConfiguration configuration)
        {
            UpdateFilterConfiguration(configuration, new Dictionary<string, (string, string?)>());
        }

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<string, (string, string?)> newValue)
        {
            configuration.Columns = newValue.Select(c => c.Key).ToList();
        }

        public override string Key { get; set; } = "Columns";
        public override string Name { get; set; } = "Columns";
        public override string HelpText { get; set; } = "";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Columns;
        public override bool ShowReset { get; set; } = false;

        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.Columns != null && configuration.Columns.Count != 0;
        }

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter | FilterType.CraftFilter;
        public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return null;
        }

        public override bool? FilterItem(FilterConfiguration configuration, ItemEx item)
        {
            return null;
        }

        public override bool CanRemove { get; set; } = true;
        
        public void AddItem(FilterConfiguration configuration, string item)
        {
            var value = CurrentValue(configuration);
            if (!value.ContainsKey(item))
            {
                value.Add(item, ("", null));
            }
            UpdateFilterConfiguration(configuration, value);
        }

        public Dictionary<string, IColumn> GetAvailableItems(FilterConfiguration configuration)
        {
            var value = PluginService.PluginLogic.GridColumns;
            var currentValue = CurrentValue(configuration);
            return value.Where(c => c.Value.CraftOnly != true && c.Value.AvailableInType(configuration.FilterType) && !currentValue.ContainsKey(c.Key)).ToDictionary(c => c.Key, c => c.Value);
        }

        public override void DrawTable(FilterConfiguration configuration)
        {
            var value = GetAvailableItems(configuration);
            base.DrawTable(configuration);
            
            var currentAddColumn = "";
            ImGui.SetNextItemWidth(LabelSize);
            ImGui.LabelText("##" + Key + "Label", "Add new column: ");
            ImGui.SameLine();
            ImGui.SetNextItemWidth(InputSize);
            using (var combo = ImRaii.Combo("Add##" + Key, currentAddColumn))
            {
                if (combo.Success)
                {
                    foreach (var column in value.OrderBy(c => c.Value.Name))
                    {
                        if (ImGui.Selectable(column.Value.Name, currentAddColumn == column.Value.Name))
                        {
                            AddItem(configuration, column.Key);
                        }

                        ImGuiUtil.HoverTooltip(column.Value.HelpText);
                    }
                }
            }
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Filters.Abstract;
using OtterGui;

namespace InventoryTools.Logic.Filters
{
    public class CraftColumnsFilter : SortedListFilter<string>
    {
        public override Dictionary<string, (string, string?)> CurrentValue(FilterConfiguration configuration)
        {
            (string, string?) GetColumnDetails(string c)
            {
                return PluginService.PluginLogic.GridColumns.ContainsKey(c) ? (PluginService.PluginLogic.GridColumns[c].Name, PluginService.PluginLogic.GridColumns[c].HelpText): (c, null);
            }

            return (configuration.CraftColumns ?? new List<string>()).ToDictionary(c => c, GetColumnDetails);
        }
        

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<string, (string, string?)> newValue)
        {
            configuration.CraftColumns = newValue.Select(c => c.Key).ToList();
        }

        public override string Key { get; set; } = "Craft Columns";
        public override string Name { get; set; } = "Craft Columns";
        public override string HelpText { get; set; } = "";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.CraftColumns;
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
            return value.Where(c => c.Value.CraftOnly != false && c.Value.AvailableInType(configuration.FilterType) && !currentValue.ContainsKey(c.Key)).ToDictionary(c => c.Key, c => c.Value);
        }

        public override void DrawTable(FilterConfiguration configuration)
        {
            var value = GetAvailableItems(configuration);
            base.DrawTable(configuration);
            
            var currentAddColumn = "";
            ImGui.SetNextItemWidth(LabelSize);
            ImGui.LabelText("##" + Key + "Label", "Add new column: ");
            ImGui.SameLine();
            if (ImGui.BeginCombo("Add##" + Key, currentAddColumn))
            {
                foreach(var column in value.OrderBy(c => c.Value.Name))
                {
                    if (ImGui.Selectable(column.Value.Name, currentAddColumn == column.Value.Name))
                    {
                        AddItem(configuration, column.Key);
                    }
                    ImGuiUtil.HoverTooltip(column.Value.HelpText);
                }

                ImGui.EndCombo();
            }
        }
    }
}
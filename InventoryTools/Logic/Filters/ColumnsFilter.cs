using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Filters.Abstract;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Logic.Filters
{
    public class ColumnsFilter : SortedListFilter<string>
    {
        public override Dictionary<string, string> CurrentValue(FilterConfiguration configuration)
        {
            string GetColumnName(string c)
            {
                return PluginService.PluginLogic.GridColumns.ContainsKey(c)
                    ? PluginService.PluginLogic.GridColumns[c]
                    : c;
            }

            return (configuration.Columns ?? new List<string>()).ToDictionary(c => c, GetColumnName);
        }
        

        public override void UpdateFilterConfiguration(FilterConfiguration configuration, Dictionary<string, string> newValue)
        {
            configuration.Columns = newValue.Select(c => c.Key).ToList();
        }

        public override string Key { get; set; } = "Columns";
        public override string Name { get; set; } = "Columns";
        public override string HelpText { get; set; } = "";
        public override FilterCategory FilterCategory { get; set; } = FilterCategory.Columns;
        public override bool HasValueSet(FilterConfiguration configuration)
        {
            return configuration.Columns != null && configuration.Columns.Count != 0;
        }

        public override FilterType AvailableIn { get; set; } =
            FilterType.SearchFilter | FilterType.SortingFilter | FilterType.GameItemFilter;
        public override bool FilterItem(FilterConfiguration configuration, InventoryItem item)
        {
            return true;
        }

        public override bool FilterItem(FilterConfiguration configuration, Item item)
        {
            return true;
        }

        public override bool CanRemove { get; set; } = true;
        
        public void AddItem(FilterConfiguration configuration, string item)
        {
            var value = CurrentValue(configuration);
            if (!value.ContainsKey(item))
            {
                value.Add(item, "");
            }
            UpdateFilterConfiguration(configuration, value);
        }

        public Dictionary<string, string> GetAvailableItems(FilterConfiguration configuration)
        {
            var value = PluginService.PluginLogic.GridColumns;
            var currentValue = CurrentValue(configuration);
            return value.Where(c => !currentValue.ContainsKey(c.Key)).ToDictionary(c => c.Key, c => c.Value);
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
                foreach(var column in value.OrderBy(c => c.Value))
                {
                    if (ImGui.Selectable(column.Value, currentAddColumn == column.Value))
                    {
                        AddItem(configuration, column.Key);
                    }
                }

                ImGui.EndCombo();
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using NaturalSort.Extension;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class TextColumn : Column<string?>
    {
        public TextColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item) ?? "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return CurrentValue(columnConfiguration, (ItemEx)item) ?? "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item) ?? "";
        }
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.Item);
        }
        
        public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.InventoryItem);
        }
        
        public override IEnumerable<CraftItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<CraftItem> items)
        {
            return items;
        }

        public override IEnumerable<CraftItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<CraftItem> items)
        {
            return items;
        }
        
        public virtual string EmptyText
        {
            get
            {
                return "N/A";
            }
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex, int columnIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex, int columnIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex, int columnIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, (ItemEx)item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex, int columnIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryChange item, int rowIndex, int columnIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }

        public override IEnumerable<ItemEx> Filter(ColumnConfiguration columnConfiguration, IEnumerable<ItemEx> items)
        {
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == columnConfiguration.FilterText;
                }
                return currentValue.ToLower().PassesFilter(columnConfiguration.FilterText.ToLower());
            });
        }

        public override IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryItem> items)
        {
            var isChecked = columnConfiguration.FilterText != "";
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == columnConfiguration.FilterText;
                }
                return currentValue.ToLower().PassesFilter(columnConfiguration.FilterText.ToLower());
            });
        }

        public override IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SortingResult> items)
        {
            var isChecked = columnConfiguration.FilterText != "";
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == columnConfiguration.FilterText;
                }
                return currentValue.ToLower().PassesFilter(columnConfiguration.FilterText.ToLower());
            });
        }
        
        public override IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items)
        {
            var isChecked = columnConfiguration.FilterText != "";
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                if (FilterType == ColumnFilterType.Choice)
                {
                    return currentValue == columnConfiguration.FilterText;
                }
                return currentValue.ToLower().PassesFilter(columnConfiguration.FilterText.ToLower());
            });
        }

        public override IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue(columnConfiguration, item), StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(item => CurrentValue(columnConfiguration, item), StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue(columnConfiguration, (ItemEx)item), StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(item => CurrentValue(columnConfiguration, (ItemEx)item), StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue(columnConfiguration, item), StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(item => CurrentValue(columnConfiguration, item), StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }
        
        public override IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => CurrentValue(columnConfiguration, c), StringComparison.OrdinalIgnoreCase.WithNaturalSort()) : items.OrderByDescending(c => CurrentValue(columnConfiguration, c), StringComparison.OrdinalIgnoreCase.WithNaturalSort());
        }

        public override List<MessageBase>? DoDraw(IItem item, string? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                if (currentValue != null)
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextUnformatted(currentValue);
                }
                else
                {
                    ImGui.TextUnformatted(EmptyText);
                }
            }

            return null;
        }


    }
}
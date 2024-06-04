using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace InventoryTools.Logic.Columns.Abstract
{
    using CriticalCommonLib.Time;

    public abstract class TimeIntervalColumn : Column<TimeInterval?>
    {
        public TimeIntervalColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CsvExport(columnConfiguration, item.Item);
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            var currentValue = CurrentValue(columnConfiguration, item);
            if (currentValue != null)
            {
                return TimeInterval.DurationString(currentValue.Value.End, TimeStamp.UtcNow, true);
            }
            return "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CsvExport(columnConfiguration, item.InventoryItem);
        }
        public override TimeInterval? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.Item);
        }
        
        public override TimeInterval? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange currentValue)
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
                return currentValue.Value.PassesFilter(columnConfiguration.FilterText.ToLower());
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
                return currentValue.Value.PassesFilter(columnConfiguration.FilterText.ToLower());
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
                return currentValue.Value.PassesFilter(columnConfiguration.FilterText.ToLower());
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
                return currentValue.Value.PassesFilter(columnConfiguration.FilterText.ToLower());
            });
        }

        public override IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue(columnConfiguration, item)) : items.OrderByDescending(item => CurrentValue(columnConfiguration, item));
        }

        public override IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue(columnConfiguration, item)) : items.OrderByDescending(item => CurrentValue(columnConfiguration, item));
        }

        public override IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item => CurrentValue(columnConfiguration, item)) : items.OrderByDescending(item => CurrentValue(columnConfiguration, item));
        }
        
        public override IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(currentValue => CurrentValue(columnConfiguration, currentValue)) : items.OrderByDescending(currentValue => CurrentValue(columnConfiguration, currentValue));
        }

        public override void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            ImGui.TableSetupColumn(columnConfiguration.Name ?? (RenderName ?? Name) ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DateTimeColumn : Column<DateTime?>
    {
        public DateTimeColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item)?.ToString(CultureInfo.CurrentCulture) ?? "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return CurrentValue(columnConfiguration, item)?.ToString(CultureInfo.CurrentCulture) ?? "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item)?.ToString(CultureInfo.CurrentCulture) ?? "";
        }
        public override DateTime? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.Item);
        }
        
        public override DateTime? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange currentValue)
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
        public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
            InventoryItem item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, (ItemEx)item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryChange item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }

        public override IEnumerable<ItemEx> Filter(ColumnConfiguration columnConfiguration, IEnumerable<ItemEx> items)
        {
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                return currentValue.Value.PassesFilter(FilterText.ToLower());
            });
        }

        public override IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryItem> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                return currentValue.Value.PassesFilter(FilterText.ToLower());
            });
        }

        public override IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SortingResult> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                return currentValue.Value.PassesFilter(FilterText.ToLower());
            });
        }
        
        public override IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }
                return currentValue.Value.PassesFilter(FilterText.ToLower());
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

        public override List<MessageBase>? DoDraw(IItem item, DateTime? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                var formattedValue = currentValue.Value.ToString(CultureInfo.CurrentCulture);
                var columnWidth = ImGui.GetColumnWidth();
                var frameHeight = filterConfiguration.TableHeight / 2.0f;
                var calcText = ImGui.CalcTextSize(formattedValue);
                var textHeight = calcText.X >= columnWidth ? 0 : calcText.Y / 2.0f;
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + frameHeight - textHeight);
                ImGui.TextUnformatted(formattedValue);
            }
            else
            {
                ImGui.TextUnformatted(EmptyText);
            }

            return null;
        }

        public override void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            ImGui.TableSetupColumn(columnConfiguration.Name ?? (RenderName ?? Name) ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }
    }
}
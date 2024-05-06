using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class ColoredTextColumn : Column<(string, Vector4)?>
    {
        public ColoredTextColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return CurrentValue(columnConfiguration, (ItemEx)item)?.Item1 ?? "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item)
        {
            return CurrentValue(columnConfiguration, item)?.Item1 ?? "";
        }

        public override (string, Vector4)? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.Item);
        }
        
        public override (string, Vector4)? CurrentValue(ColumnConfiguration columnConfiguration,
            InventoryChange currentValue)
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

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
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

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
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

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
            });
        }
        
        public override IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items)
        {
            var isChecked = FilterText != "";
            return FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c.InventoryItem);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.ToLower().PassesFilter(FilterComparisonText);
            });
        }

        public override IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, (ItemEx)item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, (ItemEx)item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }
        
        public override IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item.InventoryItem);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item.InventoryItem);
                return !currentValue.HasValue ? "" : currentValue.Value.Item1;
            });
        }

        public override List<MessageBase>? DoDraw(IItem item, (string, Vector4)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue.HasValue)
            {
                if (filterConfiguration.FilterType == Logic.FilterType.CraftFilter)
                {
                    ImGuiUtil.VerticalAlignTextColored(currentValue.Value.Item1, currentValue.Value.Item2, filterConfiguration.TableHeight, true);
                }
                else
                {
                    ImGuiUtil.VerticalAlignTextColored(currentValue.Value.Item1, currentValue.Value.Item2, filterConfiguration.TableHeight, false);
                }
            }
            else
            {
                ImGui.Text(EmptyText);
            }
            return null;
        }


    }
}
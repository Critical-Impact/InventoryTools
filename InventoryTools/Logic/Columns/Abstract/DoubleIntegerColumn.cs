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
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class DoubleIntegerColumn : Column<(int,int)?>
    {
        public DoubleIntegerColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return (CurrentValue(columnConfiguration, item)?.Item1.ToString()  ?? "") + "/" + (CurrentValue(columnConfiguration, item)?.Item2.ToString() ?? "");
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return (CurrentValue(columnConfiguration, (ItemEx)item)?.Item1.ToString()  ?? "") + "/" + (CurrentValue(columnConfiguration, (ItemEx)item)?.Item2.ToString() ?? "");
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return (CurrentValue(columnConfiguration, item)?.Item1.ToString()  ?? "") + "/" + (CurrentValue(columnConfiguration, item)?.Item2.ToString() ?? "");
        }
        
        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.Item);
        }
        
        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange currentValue)
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
        
        public virtual string Divider => "/";

        public virtual string EmptyText => "";

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

                return currentValue.Value.Item1.PassesFilter(columnConfiguration.FilterText) || currentValue.Value.Item2.PassesFilter(columnConfiguration.FilterText);
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

                return currentValue.Value.Item1.PassesFilter(columnConfiguration.FilterText) || currentValue.Value.Item2.PassesFilter(columnConfiguration.FilterText);
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

                return currentValue.Value.Item1.PassesFilter(columnConfiguration.FilterText) || currentValue.Value.Item2.PassesFilter(columnConfiguration.FilterText);
            });
        }
        
        public override IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items)
        {
            var isChecked = columnConfiguration.FilterText != "";
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c.InventoryItem);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Value.Item1.PassesFilter(columnConfiguration.FilterText) || currentValue.Value.Item2.PassesFilter(columnConfiguration.FilterText);
            });
        }

        public override IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, (ItemEx)item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, (ItemEx)item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item.InventoryItem);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item.InventoryItem);
                if (currentValue == null)
                {
                    return 0;
                }

                return currentValue.Value.Item1;
            });
        }

        public override List<MessageBase>? DoDraw(IItem item, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (ImGui.TableNextColumn())
            {
                if (currentValue != null)
                {
                    var text = $"{currentValue.Value.Item1:n0}" + Divider + $"{currentValue.Value.Item2:n0}";
                    ImGuiUtil.VerticalAlignText(text, filterConfiguration.TableHeight, false);
                }
                else
                {
                    ImGuiUtil.VerticalAlignText(EmptyText, filterConfiguration.TableHeight, false);
                }
            }

            return null;
        }


    }
}
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Abstract
{
    public abstract class CheckboxColumn : Column<bool?>
    {
        public CheckboxColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return CurrentValue(columnConfiguration, item) ?? false ? "true" : "false";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return CurrentValue(columnConfiguration, (ItemEx)item) ?? false ? "true" : "false";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item) ?? false ? "true" : "false";
        }
        
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.Item);
        }
        
        public override bool? CurrentValue(ColumnConfiguration columnConfiguration, InventoryChange currentValue)
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
            bool isChecked;
            if (columnConfiguration.FilterText == "")
            {
                return items;
            }

            if (columnConfiguration.FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, (ItemEx)c);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                if (isChecked)
                {
                    return currentValue.Value;
                }
                return !currentValue.Value;
            });
        }

        public override IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryItem> items)
        {
            bool isChecked;
            if (columnConfiguration.FilterText == "")
            {
                return items;
            }

            if (columnConfiguration.FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                return isChecked && currentValue.Value || !isChecked && !currentValue.Value;
            });
        }

        public override IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SortingResult> items)
        {
            bool isChecked;
            if (columnConfiguration.FilterText == "")
            {
                return items;
            }

            if (columnConfiguration.FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                return isChecked && currentValue.Value || !isChecked && !currentValue.Value;
            });
        }

        public override IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryChange> items)
        {
            bool isChecked;
            if (columnConfiguration.FilterText == "")
            {
                return items;
            }

            if (columnConfiguration.FilterText == "true")
            {
                isChecked = true;
            }
            else
            {
                isChecked = false;
            }
            return columnConfiguration.FilterText == "" ? items : items.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c.InventoryItem);
                if (!currentValue.HasValue)
                {
                    return false;
                }

                return isChecked && currentValue.Value || !isChecked && !currentValue.Value;
            });
        }

        private int GetSortOrder(ColumnConfiguration columnConfiguration, ItemEx c)
        {
            var currentValue = CurrentValue(columnConfiguration, c);
            return currentValue switch
            {
                null => 0,
                false => 1,
                _ => 2
            };
        }

        public override IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => GetSortOrder(columnConfiguration, c.Item)) : items.OrderByDescending(c => GetSortOrder(columnConfiguration, c.Item));
        }

        public override IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => GetSortOrder(columnConfiguration, c)) : items.OrderByDescending(c => GetSortOrder(columnConfiguration, c));
        }

        public override IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => GetSortOrder(columnConfiguration, c.InventoryItem.Item)) : items.OrderByDescending(c => GetSortOrder(columnConfiguration, c.InventoryItem.Item));
        }

        public override IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(c => GetSortOrder(columnConfiguration, c.InventoryItem.Item)) : items.OrderByDescending(c => GetSortOrder(columnConfiguration, c.InventoryItem.Item));
        }
        

        public override List<MessageBase>? DoDraw(IItem item, bool? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue.HasValue)
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (ImGui.GetContentRegionAvail().X / 2) - filterConfiguration.TableHeight / 2.0f);
                ImGuiService.DrawUldIcon(currentValue.Value ? ImGuiService.TickIcon : ImGuiService.CrossIcon, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight));
            }
            return null;
        }


    }
}
using System.Collections.Generic;
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
    public abstract class GameIconsColumn : Column<List<ushort>?>
    {
        public GameIconsColumn(ILogger logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
        }
        public override string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            return "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            return "";
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return "";
        }
        public override List<ushort>? CurrentValue(ColumnConfiguration columnConfiguration, CraftItem currentValue)
        {
            return CurrentValue(columnConfiguration, currentValue.Item);
        }
        
        public override List<ushort>? CurrentValue(ColumnConfiguration columnConfiguration,
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
            return items;
        }

        public override IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public override IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SortingResult> items)
        {
            return items;
        }

        public override IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
        {
            return items;
        }

        public override IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction,
            IEnumerable<ItemEx> items)
        {
            return items;
        }

        public override IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SortingResult> items)
        {
            return items;
        }

        public override List<MessageBase>? DoDraw(IItem item1, List<ushort>? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (currentValue != null)
            {
                for (var index = 0; index < currentValue.Count; index++)
                {
                    var item = currentValue[index];
                    ImGuiService.DrawIcon(item, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight) * ImGui.GetIO().FontGlobalScale);
                    if (index != currentValue.Count)
                    {
                        ImGui.SameLine();
                    }
                }
            }
            return null;
        }


    }
}
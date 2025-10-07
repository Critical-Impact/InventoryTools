using System.Collections.Generic;
using System.Numerics;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Services;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Style;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Filters;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class NameColumn : ColoredTextColumn
    {
        private readonly ItemSheet _itemSheet;
        private readonly ExcelSheet<World> _worldSheet;
        private readonly ICharacterMonitor _characterMonitor;
        private readonly GroupedGroupByFilter _groupByFilter;
        private readonly Dictionary<GroupedItemKey, string> formattedNames = [];

        public NameColumn(ILogger<NameColumn> logger, ImGuiService imGuiService, ItemSheet itemSheet, ExcelSheet<World> worldSheet, ICharacterMonitor characterMonitor, GroupedGroupByFilter groupByFilter) : base(logger, imGuiService)
        {
            _itemSheet = itemSheet;
            _worldSheet = worldSheet;
            _characterMonitor = characterMonitor;
            _groupByFilter = groupByFilter;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override (string, Vector4)? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.GroupedItem != null)
            {
                var groupedItem = searchResult.GroupedItem;
                if (!groupedItem.Grouping.IsGrouped)
                {
                    return (_itemSheet.GetRowOrDefault(groupedItem.ItemId)?.NameString ?? "Unknown Item", ImGuiColors.DalamudWhite);
                }

                if (!this.formattedNames.ContainsKey(groupedItem.Grouping))
                {
                    List<string> pieces = [];
                    if (groupedItem.Grouping.WorldId != null)
                    {
                        var world = _worldSheet.GetRowOrDefault(groupedItem.Grouping.WorldId.Value);
                        if (world != null)
                        {
                            pieces.Add(world.Value.Name.ExtractText());
                        }
                    }

                    if (groupedItem.Grouping.OwnerId != null)
                    {
                        var character = _characterMonitor.GetCharacterById(groupedItem.Grouping.OwnerId.Value);
                        if (character != null)
                        {
                            pieces.Add(character.FormattedName);
                        }
                    }

                    if (groupedItem.Grouping.CharacterId != null)
                    {
                        var character =
                            _characterMonitor.GetCharacterById(groupedItem.Grouping.CharacterId.Value);
                        if (character != null)
                        {
                            pieces.Add(character.FormattedName);
                        }
                    }

                    var item = _itemSheet.GetRowOrDefault(groupedItem.Grouping.ItemId);
                    if (item != null)
                    {
                        pieces.Add(item.NameString);
                    }

                    if (groupedItem.Grouping.IsHq != null)
                    {
                        pieces.Add(groupedItem.Grouping.IsHq.Value ? "HQ" : "NQ");
                    }

                    this.formattedNames[groupedItem.Grouping] = string.Join(" - ", pieces);
                }

                return (this.formattedNames[groupedItem.Grouping], ImGuiColors.DalamudWhite);
            }
            return (
                searchResult.CraftItem?.FormattedName ??
                searchResult.InventoryItem?.FormattedName ?? searchResult.Item.NameString,
                searchResult.InventoryItem?.ItemColour ?? ImGuiColors.DalamudWhite);
        }
        public override string Name { get; set; } = "Name";
        public override float Width { get; set; } = 250.0f;
        public override string HelpText { get; set; } = "The name of the item.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter | Logic.FilterType.GroupedList;
    }
}
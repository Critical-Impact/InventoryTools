using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using Vector2 = FFXIVClientStructs.FFXIV.Common.Math.Vector2;

namespace InventoryTools.Logic.Columns
{
    public class UseIconsColumn : Column<List<ItemSource>?>
    {
        private readonly ItemInfoRenderService _itemInfoRenderService;

        public UseIconsColumn(ILogger<UseIconsColumn> logger, ImGuiService imGuiService, ItemInfoRenderService itemInfoRenderService) : base(logger, imGuiService)
        {
            _itemInfoRenderService = itemInfoRenderService;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override List<ItemSource>? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.Uses;
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, List<ItemSource>? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;

            using var foo = ImRaii.PushId(columnConfiguration.Key + rowIndex);
            var messages = new List<MessageBase>();
            if (currentValue != null)
            {
                var itemUses = columnConfiguration.FilterText != "" ? currentValue.Where(c => (_itemInfoRenderService.GetUseName(c).ToLower() + " " + _itemInfoRenderService.GetUseTypeName(c.GetType()).Singular.ToLower()).PassesFilterComparisonText(columnConfiguration.FilterComparisonText)) : currentValue;
                messages.AddRange(_itemInfoRenderService.DrawItemUseIconsContainer("ItemUses" + rowIndex, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight), itemUses.ToList()));
            }
            return messages;
        }

        public override void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration,
            int columnIndex)
        {
            ImGui.TableSetupColumn(RenderName ?? Name, ImGuiTableColumnFlags.WidthFixed, Width, (uint)columnIndex);
        }

        public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Uses.Select(c => _itemInfoRenderService.GetUseName(c)));
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Uses.Select(c => _itemInfoRenderService.GetUseName(c)));
        }

        public override float Width { get; set; } = 250;
        public override string Name { get; set; } = "Uses";

        public override string HelpText { get; set; } =
            "Shows icons indicating what the items drop/can be used for";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration,
            IEnumerable<SearchResult> searchResults)
        {
            return columnConfiguration.FilterText == "" ? searchResults : searchResults.Where(c =>
            {
                var currentValue = CurrentValue(columnConfiguration, c);
                if (currentValue == null)
                {
                    return false;
                }

                return currentValue.Any(e => (_itemInfoRenderService.GetUseName(e).ToLower() + " " + _itemInfoRenderService.GetUseTypeName(e.GetType()).Singular.ToLower()).PassesFilterComparisonText(columnConfiguration.FilterComparisonText));
            });
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return currentValue?.Count ?? 0;
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                return currentValue?.Count ?? 0;
            });
        }

        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            return DoDraw(searchResult, CurrentValue(columnConfiguration, searchResult), rowIndex, configuration, columnConfiguration);
        }

        public override FilterType DefaultIn => Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter;

    }
}
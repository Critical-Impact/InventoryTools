using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class AcquisitionSourceIconsColumn : Column<List<ItemSource>?>
    {
        public SourceTypeSelectorSetting SourceTypeSelectorSetting { get; }

        private readonly ItemInfoRenderService _itemInfoRenderService;

        public AcquisitionSourceIconsColumn(ILogger<AcquisitionSourceIconsColumn> logger, ImGuiService imGuiService, ItemInfoRenderService itemInfoRenderService, SourceTypeSelectorSetting sourceTypeSelectorSetting) : base(logger, imGuiService)
        {
            SourceTypeSelectorSetting = sourceTypeSelectorSetting;
            _itemInfoRenderService = itemInfoRenderService;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

        public override List<ItemSource>? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return searchResult.Item.Sources;
        }

        public override void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Separator();
            SourceTypeSelectorSetting.Draw(columnConfiguration, null);
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
                var search = currentValue.AsEnumerable();
                var sourceTypeSelectorSetting = SourceTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (sourceTypeSelectorSetting != null)
                {
                    search = currentValue.Where(c => sourceTypeSelectorSetting.Contains(c.Type));
                }

                var itemSources = columnConfiguration.FilterText != "" ? search.Where(c => (_itemInfoRenderService.GetSourceName(c).ToLower() + " " + _itemInfoRenderService.GetSourceTypeName(c.GetType()).Singular.ToLower()).PassesFilterComparisonText(columnConfiguration.FilterComparisonText)) : search;
                messages.AddRange(_itemInfoRenderService.DrawItemSourceIconsContainer("ItemUses" + rowIndex, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight), itemSources.ToList()));
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
            return String.Join(", ", searchResult.Item.Sources.Select(c => _itemInfoRenderService.GetSourceName(c)));
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            return String.Join(", ", searchResult.Item.Sources.Select(c => _itemInfoRenderService.GetSourceName(c)));
        }

        public override string Name { get; set; } = "Acquisition";
        public override float Width { get; set; } = 250;

        public override string HelpText { get; set; } =
            "Shows icons indicating what items can be obtained with(gathering, crafting, currency, etc)";
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

                var search = currentValue.AsEnumerable();
                var sourceTypeSelectorSetting = SourceTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (sourceTypeSelectorSetting != null)
                {
                    search = currentValue.Where(c => sourceTypeSelectorSetting.Contains(c.Type));
                }


                return search.Any(e => (_itemInfoRenderService.GetSourceName(e).ToLower() + " " + _itemInfoRenderService.GetSourceTypeName(e.GetType()).Singular.ToLower()).PassesFilterComparisonText(columnConfiguration.FilterComparisonText));
            });
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration,
            ImGuiSortDirection direction, IEnumerable<SearchResult> items)
        {
            return direction == ImGuiSortDirection.Ascending ? items.OrderBy(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }
                var search = currentValue.AsEnumerable();
                var sourceTypeSelectorSetting = SourceTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (sourceTypeSelectorSetting != null)
                {
                    search = currentValue.Where(c => sourceTypeSelectorSetting.Contains(c.Type));
                }

                return search.Count();
            }) : items.OrderByDescending(item =>
            {
                var currentValue = CurrentValue(columnConfiguration, item);
                if (currentValue == null)
                {
                    return 0;
                }
                var search = currentValue.AsEnumerable();
                var sourceTypeSelectorSetting = SourceTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (sourceTypeSelectorSetting != null)
                {
                    search = currentValue.Where(c => sourceTypeSelectorSetting.Contains(c.Type));
                }

                return search.Count();
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
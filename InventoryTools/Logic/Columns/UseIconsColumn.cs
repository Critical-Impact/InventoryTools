using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Columns.ColumnSettings;
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
        private readonly StringColumnSetting.Factory _stringColumnFactory;
        private readonly MapSheet _mapSheet;
        private readonly StringColumnSetting requirementSetting;
        private readonly StringColumnSetting rewardsSetting;
        private readonly StringColumnSetting mapSetting;
        private readonly UseTypeSelectorSetting _useTypeSelectorSetting;
        private readonly SourceCategorySelectorSetting _useCategorySelectorSetting;

        public UseIconsColumn(ILogger<UseIconsColumn> logger, ImGuiService imGuiService, ItemInfoRenderService itemInfoRenderService,
            UseTypeSelectorSetting useTypeSelectorSetting, StringColumnSetting.Factory stringColumnFactory,SourceCategorySelectorSetting useCategorySelectorSetting,
            MapSheet mapSheet) : base(logger, imGuiService)
        {
            _useTypeSelectorSetting = useTypeSelectorSetting;
            _itemInfoRenderService = itemInfoRenderService;
            _stringColumnFactory = stringColumnFactory;
            _useCategorySelectorSetting = useCategorySelectorSetting;
            this.requirementSetting = _stringColumnFactory.Invoke("requirement", "Requires", "Search for a source that requires a specific item.",
                null);
            this.rewardsSetting = _stringColumnFactory.Invoke("rewards", "Rewards", "Search for a source that rewards a specific item. While this might seem redundant, certain sources provide more than one item.",
                null);
            this.mapSetting = _stringColumnFactory.Invoke("maps", "Maps", "Search for a source in a specific map.",
                null);
            _mapSheet = mapSheet;

            this.Settings.Add(useTypeSelectorSetting);
            this.FilterSettings.Add(useTypeSelectorSetting);
            this.FilterSettings.Add(useCategorySelectorSetting);
            this.FilterSettings.Add(rewardsSetting);
            this.FilterSettings.Add(requirementSetting);
            this.FilterSettings.Add(mapSetting);
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
                var search = currentValue.AsEnumerable();
                var useTypeSelectorSetting = _useTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (useTypeSelectorSetting != null)
                {
                    search = search.Where(c => useTypeSelectorSetting.Contains(c.Type));
                }

                var useTypeSelectorFilterSetting = _useTypeSelectorSetting.CurrentValue(columnConfiguration.FilterConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (useTypeSelectorFilterSetting != null)
                {
                    search = search.Where(c => useTypeSelectorFilterSetting.Contains(c.Type));
                }

                var useCategorySelectorSetting = _useCategorySelectorSetting.CurrentValue(columnConfiguration.FilterConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (useCategorySelectorSetting != null)
                {
                    search = search.Where(itemUse => useCategorySelectorSetting.Any(category => _itemInfoRenderService.InUseCategory(itemUse.Type, category)));
                }

                var rewardsValue = this.rewardsSetting.CurrentValue(columnConfiguration.FilterConfiguration);
                if (rewardsValue != null)
                {
                    var filterText = new FilterComparisonText(rewardsValue);
                    search = search.Where(c => c.Item.NameString.ToLower().PassesFilter(filterText) || c.Items.Any(d => d.NameString.ToLower().PassesFilter(filterText)));
                }
                var requirementsValue = this.requirementSetting.CurrentValue(columnConfiguration.FilterConfiguration);
                if (requirementsValue != null)
                {
                    var filterText = new FilterComparisonText(requirementsValue);
                    search = search.Where(c => (c.CostItem != null && c.CostItem.NameString.ToLower().PassesFilter(filterText)) || c.CostItems.Any(d => d.NameString.ToLower().PassesFilter(filterText)));
                }
                var mapsValue = this.mapSetting.CurrentValue(columnConfiguration.FilterConfiguration);
                if (mapsValue != null)
                {
                    var filterText = new FilterComparisonText(mapsValue);
                    search = search.Where(itemSource => itemSource.MapIds != null && itemSource.MapIds.Any(mapId => _mapSheet.GetRow(mapId).FormattedName.ToLower().PassesFilter(filterText)));
                }

                var itemSources = columnConfiguration.FilterText != "" ? search.Where(c => (_itemInfoRenderService.GetUseName(c).ToLower() + " " + _itemInfoRenderService.GetUseTypeName(c.GetType()).Singular.ToLower()).PassesFilterComparisonText(columnConfiguration.FilterComparisonText)) : search;
                messages.AddRange(_itemInfoRenderService.DrawItemUseIconsContainer("ItemUses" + rowIndex, filterConfiguration.TableHeight * ImGui.GetIO().FontGlobalScale - ImGui.GetStyle().FramePadding.X, new Vector2(filterConfiguration.TableHeight, filterConfiguration.TableHeight), itemSources.ToList()));
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
            var itemUses = columnConfiguration.FilterText != "" ? searchResult.Item.Uses.Where(c => (_itemInfoRenderService.GetUseName(c).ToLower() + " " + _itemInfoRenderService.GetUseTypeName(c.GetType()).Singular.ToLower()).PassesFilterComparisonText(columnConfiguration.FilterComparisonText)) : searchResult.Item.Uses;
            return String.Join(", ", itemUses.Select(c => _itemInfoRenderService.GetUseDescription(c)));
        }

        public override dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            var itemUses = columnConfiguration.FilterText != "" ? searchResult.Item.Uses.Where(c => (_itemInfoRenderService.GetUseName(c).ToLower() + " " + _itemInfoRenderService.GetUseTypeName(c.GetType()).Singular.ToLower()).PassesFilterComparisonText(columnConfiguration.FilterComparisonText)) : searchResult.Item.Uses;
            return String.Join(", ", itemUses.Select(c => _itemInfoRenderService.GetUseDescription(c)));
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
            var filterUseTypeSetting = _useTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
            var filterUseTypeFilterSetting = _useTypeSelectorSetting.CurrentValue(columnConfiguration.FilterConfiguration)?.Select(d => d.Item1).ToList() ?? null;
            var useCategorySelectorSetting = _useCategorySelectorSetting.CurrentValue(columnConfiguration.FilterConfiguration)?.Select(d => d.Item1).ToList() ?? null;
            var rewardsValue = this.rewardsSetting.CurrentValue(columnConfiguration.FilterConfiguration);
            var requirementsValue = this.requirementSetting.CurrentValue(columnConfiguration.FilterConfiguration);
            var mapsValue = this.mapSetting.CurrentValue(columnConfiguration.FilterConfiguration);

            searchResults = searchResults.Where(searchResult =>
            {
                if (useCategorySelectorSetting == null && filterUseTypeFilterSetting == null && filterUseTypeSetting == null && rewardsValue == null && requirementsValue == null && mapsValue == null && columnConfiguration.FilterText == string.Empty)
                {
                    return true;
                }
                return searchResult.Item.Uses.Any(c =>
                {
                    if (columnConfiguration.FilterText != string.Empty)
                    {
                        if (!(_itemInfoRenderService.GetUseName(c).ToLower() + " " + _itemInfoRenderService
                                .GetUseTypeName(c.GetType()).Singular.ToLower())
                            .PassesFilterComparisonText(columnConfiguration.FilterComparisonText))
                        {
                            return false;
                        }
                    }
                    if (filterUseTypeSetting != null)
                    {
                        if(!filterUseTypeSetting.Contains(c.Type))
                        {
                            return false;
                        }
                    }
                    if (filterUseTypeFilterSetting != null)
                    {
                        if(!filterUseTypeFilterSetting.Contains(c.Type))
                        {
                            return false;
                        }
                    }
                    if (useCategorySelectorSetting != null)
                    {
                        if (!useCategorySelectorSetting.Any(category =>
                                _itemInfoRenderService.InUseCategory(c.Type, category)))
                        {
                            return false;
                        }
                    }
                    if (rewardsValue != null)
                    {
                        if (!c.Items.Any(d => d.NameString.ToLower().PassesFilter(rewardsValue)))
                        {
                            return false;
                        }
                    }
                    if (requirementsValue != null)
                    {
                         if (!c.CostItems.Any(d => d.NameString.ToLower().PassesFilter(requirementsValue)))
                         {
                             return false;
                         }
                    }

                    if (mapsValue != null)
                    {
                        if (c.MapIds == null)
                        {
                            return false;
                        }
                        var filterText = new FilterComparisonText(mapsValue);
                        if(!c.MapIds.Any(mapId => _mapSheet.GetRow(mapId).FormattedName.ToLower().PassesFilter(filterText)))
                        {
                            return false;
                        }
                    }

                    return true;
                });
            });
            return searchResults;
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
                var useTypeSelectorSetting = _useTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (useTypeSelectorSetting != null)
                {
                    search = currentValue.Where(c => useTypeSelectorSetting.Contains(c.Type));
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
                var useTypeSelectorSetting = _useTypeSelectorSetting.CurrentValue(columnConfiguration)?.Select(d => d.Item1).ToList() ?? null;
                if (useTypeSelectorSetting != null)
                {
                    search = currentValue.Where(c => useTypeSelectorSetting.Contains(c.Type));
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
using System.Collections.Generic;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardMinPriceNQColumn : GilColumn
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IMarketCache _marketCache;

        public MarketBoardMinPriceNQColumn(ILogger<MarketBoardMinPriceNQColumn> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _marketCache = marketCache;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;

        protected readonly string LoadingString = "loading...";
        protected readonly string UntradableString = "untradable";
        protected readonly int Loading = -1;
        protected readonly int Untradable = -2;

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

        public override List<MessageBase>? DoDraw(IItem item, int? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (currentValue.HasValue && currentValue.Value == Loading)
            {
                ImGui.TableNextColumn();
                ImGuiUtil.VerticalAlignTextColored(LoadingString, ImGuiColors.DalamudYellow, filterConfiguration.TableHeight, false);
            }
            else if (currentValue.HasValue && currentValue.Value == Untradable)
            {
                ImGui.TableNextColumn();
                ImGuiUtil.VerticalAlignTextColored(UntradableString, ImGuiColors.DalamudRed, filterConfiguration.TableHeight, false);
            }
            else if(currentValue.HasValue)
            {
                base.DoDraw(item, currentValue, rowIndex, filterConfiguration, columnConfiguration);
                ImGui.SameLine();
                if (ImGui.SmallButton("R##" + rowIndex))
                {
                    var activeCharacter = _characterMonitor.ActiveCharacter;
                    if (activeCharacter != null)
                    {
                        return new List<MessageBase> {new MarketRequestItemUpdate(item.ItemId)};
                    }
                }
            }
            else
            {
                base.DoDraw(item, currentValue, rowIndex, filterConfiguration, columnConfiguration);
            }
            return null;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return Untradable;
            }
            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null)
            {
                var marketBoardData = _marketCache.GetPricing(item.ItemId, activeCharacter.WorldId, false);
                if (marketBoardData != null)
                {
                    var hq = marketBoardData.MinPriceNq;
                    return (int)hq;
                }
            }

            return Loading;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            if (!item.CanBeTraded)
            {
                return Untradable;
            }
            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null)
            {
                var marketBoardData = _marketCache.GetPricing(item.RowId, activeCharacter.WorldId, false);
                if (marketBoardData != null)
                {
                    var hq = marketBoardData.MinPriceNq;
                    return (int)hq;
                }
            }

            return Loading;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Market Board Minimum Price NQ";
        public override string RenderName => "MB Min. Price NQ";
        public override string HelpText { get; set; } =
            "Shows the minimum price of the NQ form of the item. This data is sourced from universalis.";
        public override float Width { get; set; } = 250.0f;
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
using System.Collections.Generic;
using CriticalCommonLib.Crafting;
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
using InventoryTools.Logic.Columns.Settings;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardPriceColumn : DoubleGilColumn
    {
        private readonly IMarketCache _marketCache;
        private readonly ICharacterMonitor _characterMonitor;

        public MarketBoardPriceColumn(ILogger<MarketBoardPriceColumn> logger, ImGuiService imGuiService, MarketboardWorldSetting marketboardWorldSetting, ICharacterMonitor characterMonitor, IMarketCache marketCache) : base(logger, imGuiService)
        {
            _marketCache = marketCache;
            _characterMonitor = characterMonitor;
            MarketboardWorldSetting = marketboardWorldSetting;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;
        protected readonly string LoadingString = "loading...";
        protected readonly string UntradableString = "untradable";
        protected readonly int Loading = -1;
        protected readonly int Untradable = -2;
        public MarketboardWorldSetting MarketboardWorldSetting { get; }

        public override void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Separator();
            MarketboardWorldSetting.Draw(columnConfiguration);
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
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
        {
            return DoDraw(item, CurrentValue(columnConfiguration, item), rowIndex, configuration, columnConfiguration);
        }

        public override List<MessageBase>? DoDraw(IItem item, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (currentValue.HasValue && currentValue.Value.Item1 == Loading)
            {
                ImGui.TableNextColumn();
                ImGuiUtil.VerticalAlignTextColored(LoadingString, ImGuiColors.DalamudYellow, filterConfiguration.TableHeight, false);
            }
            else if (currentValue.HasValue && currentValue.Value.Item1 == Untradable)
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
                    var selectedWorld = MarketboardWorldSetting.CurrentValue(columnConfiguration);
                    if (selectedWorld != null)
                    {
                        return new List<MessageBase> {new MarketRequestItemWorldUpdate(item.ItemId, selectedWorld.RowId)};
                    }
                    else
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

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }
            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null)
            {
                var marketBoardData = _marketCache.GetPricing(item.ItemId, activeCharacter.WorldId, false);
                if (marketBoardData != null)
                {
                    var nq = marketBoardData.AveragePriceNq;
                    var hq = marketBoardData.AveragePriceHq;
                    return ((int)nq, (int)hq);
                }
            }

            return (Loading, Loading);
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
        {
            if (!item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            uint selectedWorldId = 0; 
            var selectedWorld = MarketboardWorldSetting.CurrentValue(columnConfiguration);
            if (selectedWorld != null)
            {
                selectedWorldId = selectedWorld.RowId;
            }
            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null && selectedWorldId == 0)
            {
                selectedWorldId = activeCharacter.WorldId;
            }

            if (selectedWorldId != 0)
            {
                var marketBoardData = _marketCache.GetPricing(item.RowId, selectedWorldId, false);
                if (marketBoardData != null)
                {
                    var nq = marketBoardData.AveragePriceNq;
                    var hq = marketBoardData.AveragePriceHq;
                    return ((int)nq, (int)hq);
                }
            }

            return (Loading, Loading);
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Market Board Average Price NQ/HQ";
        public override string RenderName => "MB Avg. Price NQ/HQ";
        public override string HelpText { get; set; } =
            "Shows the average price of both the NQ and HQ form of the item. This data is sourced from universalis.";
        public override float Width { get; set; } = 200.0f;
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
using System.Collections.Generic;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardSevenDayCountColumn : IntegerColumn
    {
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IMarketCache _marketCache;
        private readonly InventoryToolsConfiguration _configuration;
        private MarketboardWorldSetting MarketboardWorldSetting { get; }

        public MarketBoardSevenDayCountColumn(ILogger<MarketBoardSevenDayCountColumn> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IMarketCache marketCache, InventoryToolsConfiguration configuration, MarketboardWorldSetting marketboardWorldSetting) : base(logger, imGuiService)
        {
            _characterMonitor = characterMonitor;
            _marketCache = marketCache;
            _configuration = configuration;
            MarketboardWorldSetting = marketboardWorldSetting;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Market;
        protected readonly string LoadingString = "loading...";
        protected readonly string UntradableString = "untradable";
        protected readonly int Loading = -1;
        protected readonly int Untradable = -2;
        public override bool IsConfigurable => true;

        public override void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Separator();
            MarketboardWorldSetting.Draw(columnConfiguration, null);
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, int? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (currentValue.HasValue && currentValue.Value == Loading)
            {
                ImGui.TableNextColumn();
                if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
                {
                    ImGui.TextColored(ImGuiColors.DalamudYellow, LoadingString);
                }
            }
            else if (currentValue.HasValue && currentValue.Value == Untradable)
            {
                ImGui.TableNextColumn();
                if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
                {
                    ImGui.TextColored(ImGuiColors.DalamudRed, UntradableString);
                }
            }
            else if(currentValue.HasValue)
            {
                base.DoDraw(searchResult, currentValue, rowIndex, filterConfiguration, columnConfiguration);

            }
            else
            {
                base.DoDraw(searchResult, currentValue, rowIndex, filterConfiguration, columnConfiguration);
            }

            return null;
        }
        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem is {CanBeTraded: false})
            {
                return Untradable;
            }

            if (!searchResult.Item.CanBeTraded)
            {
                return Untradable;
            }
            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null)
            {
                var selectedWorldId = MarketboardWorldSetting.SelectedWorldId(columnConfiguration, activeCharacter);
                var marketBoardData = _marketCache.GetPricing(searchResult.Item.RowId, selectedWorldId, false);
                if (marketBoardData != null)
                {
                    var sevenDaySellCount = marketBoardData.SevenDaySellCount;
                    return sevenDaySellCount;
                }
            }

            return Loading;
        }
        public override string Name
        {
            get => "Market Board " + _configuration.MarketSaleHistoryLimit + " Day Sale Count";
            set { }
        }

        public override string RenderName => "MB " + _configuration.MarketSaleHistoryLimit + " Day Sales";
        public override string HelpText
        {
            get =>
                "Shows the number of sales over a " + +_configuration.MarketSaleHistoryLimit +
                " day period for the item. If no world is selected, your home world is used. This data is sourced from universalis.";
            set { }
        }

        public override float Width { get; set; } = 250.0f;
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
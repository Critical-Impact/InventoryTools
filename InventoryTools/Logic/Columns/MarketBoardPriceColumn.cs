using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

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

        public override bool IsConfigurable => true;

        public override void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
        {
            ImGui.NewLine();
            ImGui.Separator();
            MarketboardWorldSetting.Draw(columnConfiguration, null);
        }
        public override List<MessageBase>? Draw(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex, int columnIndex)
        {
            return DoDraw(searchResult, CurrentValue(columnConfiguration, searchResult), rowIndex, configuration, columnConfiguration);
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, (int, int)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (currentValue.HasValue && currentValue.Value.Item1 == Loading)
            {
                ImGui.TableNextColumn();
                if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
                {
                    ImGuiUtil.VerticalAlignTextColored(LoadingString, ImGuiColors.DalamudYellow,
                        filterConfiguration.TableHeight, false);
                }
            }
            else if (currentValue.HasValue && currentValue.Value.Item1 == Untradable)
            {
                ImGui.TableNextColumn();
                if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
                {
                    ImGuiUtil.VerticalAlignTextColored(UntradableString, ImGuiColors.DalamudRed,
                        filterConfiguration.TableHeight, false);
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

            if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
            {
                var activeCharacter = _characterMonitor.ActiveCharacter;
                if (activeCharacter != null)
                {
                    ImGui.SameLine();
                    ImGui.Image(ImGuiService.GetIconTexture(Icons.MarketboardIcon).ImGuiHandle, new Vector2(16, 16));
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.None))
                    {
                        using (var tooltip = ImRaii.Tooltip())
                        {
                            if (tooltip.Success)
                            {
                                var selectedWorldId =
                                    MarketboardWorldSetting.SelectedWorldId(columnConfiguration, activeCharacter);
                                var pricing = _marketCache.GetPricing(searchResult.Item.RowId, selectedWorldId, false);
                                if (pricing is { recentHistory: null, listings: null })
                                {
                                    ImGui.Text("No data available");
                                }

                                if (pricing is { listings: not null })
                                {
                                    ImGui.Text("Listings: ");
                                    ImGui.Separator();

                                    foreach (var price in pricing.listings)
                                    {
                                        ImGui.Text(price.quantity + " available at " + price.pricePerUnit +
                                                   (price.hq ? " (HQ)" : ""));
                                    }
                                }

                                if (pricing is { recentHistory: not null })
                                {
                                    ImGui.Text("History: ");
                                    ImGui.Separator();

                                    foreach (var price in pricing.recentHistory)
                                    {
                                        ImGui.Text(price.quantity + " available at " + price.pricePerUnit +
                                                   (price.hq ? " (HQ)" : ""));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public override (int, int)? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem is {CanBeTraded: false})
            {
                return (Untradable, Untradable);
            }
            if (!searchResult.Item.CanBeTraded)
            {
                return (Untradable, Untradable);
            }

            var activeCharacter = _characterMonitor.ActiveCharacter;
            if (activeCharacter != null)
            {
                var selectedWorldId = MarketboardWorldSetting.SelectedWorldId(columnConfiguration, activeCharacter);
                var marketBoardData = _marketCache.GetPricing(searchResult.Item.RowId, selectedWorldId, false);
                if (marketBoardData != null)
                {
                    var nq = marketBoardData.AveragePriceNq;
                    var hq = marketBoardData.AveragePriceHq;
                    return ((int)nq, (int)hq);
                }
            }

            return (Loading, Loading);
        }

        public override string Name { get; set; } = "Market Board Average Price NQ/HQ";
        public override string RenderName => "MB Avg. Price NQ/HQ";
        public override string HelpText { get; set; } =
            "Shows the average price of both the NQ and HQ form of the item. If no world is selected, your home world is used. This data is sourced from universalis.";
        public override float Width { get; set; } = 200.0f;
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
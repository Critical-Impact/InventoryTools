using System.Collections.Generic;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class MarketBoardMinPriceHQColumn : GilColumn
    {
        public MarketboardWorldSetting MarketboardWorldSetting { get; }
        private readonly ICharacterMonitor _characterMonitor;
        private readonly IMarketCache _marketCache;

        public MarketBoardMinPriceHQColumn(ILogger<MarketBoardMinPriceHQColumn> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, IMarketCache marketCache, MarketboardWorldSetting marketboardWorldSetting) : base(logger, imGuiService)
        {
            MarketboardWorldSetting = marketboardWorldSetting;
            _characterMonitor = characterMonitor;
            _marketCache = marketCache;
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
            MarketboardWorldSetting.Draw(columnConfiguration);
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

        public override List<MessageBase>? DoDraw(IItem item, int? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            if (currentValue.HasValue && currentValue.Value == Loading)
            {
                if (ImGui.TableNextColumn())
                {
                    ImGuiUtil.VerticalAlignTextColored(LoadingString, ImGuiColors.DalamudYellow,
                        filterConfiguration.TableHeight, false);
                }
            }
            else if (currentValue.HasValue && currentValue.Value == Untradable)
            {
                if (ImGui.TableNextColumn())
                {
                    ImGuiUtil.VerticalAlignTextColored(UntradableString, ImGuiColors.DalamudRed,
                        filterConfiguration.TableHeight, false);
                }
            }
            else if(currentValue.HasValue)
            {
                if (ImGui.TableNextColumn())
                {
                    if (currentValue != null)
                    {
                        OtterGui.ImGuiUtil.RightAlign($"{currentValue.Value:n0}" + SeIconChar.Gil.ToIconString());
                    }
                    else
                    {
                        ImGui.Text(EmptyText);
                    }
                    ImGui.SameLine();
                    if (ImGui.SmallButton("R##" + rowIndex))
                    {
                        var activeCharacter = _characterMonitor.ActiveCharacter;
                        if (activeCharacter != null)
                        {
                            return new List<MessageBase> { new MarketRequestItemUpdateMessage(item.ItemId) };
                        }
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

            return CurrentValue(columnConfiguration, item.Item);
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
                var selectedWorldId = MarketboardWorldSetting.SelectedWorldId(columnConfiguration, activeCharacter);
                var marketBoardData = _marketCache.GetPricing(item.ItemId, selectedWorldId, false);
                if (marketBoardData != null)
                {
                    var hq = marketBoardData.MinPriceHq;
                    return (int)hq;
                }
            }

            return Loading;
        }

        public override int? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
        {
            return CurrentValue(columnConfiguration, item.InventoryItem);
        }

        public override string Name { get; set; } = "Market Board Minimum Price HQ";
        public override string RenderName => "MB Min. Price HQ";
        public override float Width { get; set; } = 250.0f;

        public override string HelpText { get; set; } =
            "Shows the minimum price of the HQ form of the item. If no world is selected, your home world is used. This data is sourced from universalis.";
        public override bool HasFilter { get; set; } = true;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
    }
}
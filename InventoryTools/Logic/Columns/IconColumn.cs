using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Settings;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;

using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IconColumn : GameIconColumn
    {
        private readonly ImGuiTooltipService _tooltipService;
        private readonly ImGuiTooltipModeSetting _tooltipModeSetting;
        private readonly InventoryToolsConfiguration _configuration;
        private readonly IKeyState _keyState;

        public IconColumn(ILogger<IconColumn> logger, ImGuiTooltipService tooltipService, ImGuiService imGuiService, ImGuiTooltipModeSetting tooltipModeSetting, InventoryToolsConfiguration configuration, IKeyState keyState) : base(logger, imGuiService)
        {
            _tooltipService = tooltipService;
            _tooltipModeSetting = tooltipModeSetting;
            _configuration = configuration;
            _keyState = keyState;
        }
        public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
        public override (ushort, bool)? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
        {
            if (searchResult.InventoryItem != null)
            {
                return (searchResult.InventoryItem.Icon, searchResult.InventoryItem.IsHQ);
            }
            return (searchResult.Item.Icon, false);
        }

        public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> searchResults)
        {
            return searchResults;
        }

        public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<SearchResult> searchResults)
        {
            return searchResults;
        }

        public override List<MessageBase>? DoDraw(SearchResult searchResult, (ushort, bool)? currentValue, int rowIndex,
            FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
        {
            ImGui.TableNextColumn();
            if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;
            var messages = new List<MessageBase>();
            if (currentValue != null)
            {
                using (ImRaii.PushId("icon" + rowIndex))
                {
                    if (ImGui.ImageButton(
                            ImGuiService.GetIconTexture(currentValue.Value.Item1, currentValue.Value.Item2).ImGuiHandle,
                            new Vector2(filterConfiguration.TableHeight - 1, filterConfiguration.TableHeight - 1) *
                            ImGui.GetIO().FontGlobalScale, new Vector2(0, 0), new Vector2(1, 1), 2))
                    {
                        if (!this._keyState[VirtualKey.CONTROL] && !this._keyState[VirtualKey.SHIFT] &&
                            !this._keyState[VirtualKey.MENU])
                        {
                            messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), searchResult.Item.RowId));
                        }
                    }

                    if (_tooltipModeSetting.CurrentValue(_configuration) == ImGuiTooltipMode.Icons)
                    {
                        if (ImGui.IsItemHovered())
                        {
                            _tooltipService.DrawItemTooltip(searchResult);
                        }
                    }
                }
            }
            return messages;

        }


        public override string Name { get; set; } = "Icon";
        public override string RenderName => "";
        public override float Width { get; set; } = 60.0f;
        public override string HelpText { get; set; } = "Shows the icon of the item, pressing it will open the more information window for the item.";
        public override bool HasFilter { get; set; } = false;
        public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

        public override FilterType DefaultIn => Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter | Logic.FilterType.GameItemFilter | Logic.FilterType.CraftFilter | Logic.FilterType.HistoryFilter;
    }
}
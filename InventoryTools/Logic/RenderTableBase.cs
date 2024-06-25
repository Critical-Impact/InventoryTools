using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Services;

namespace InventoryTools.Logic
{
    public abstract class RenderTableBase : IRenderTableBase
    {
        private readonly RightClickService _rightClickService;
        private readonly InventoryToolsConfiguration _configuration;

        protected ImGuiTableFlags _tableFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersV |
                                                ImGuiTableFlags.BordersOuterV | ImGuiTableFlags.BordersInnerV |
                                                ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersOuterH |
                                                ImGuiTableFlags.BordersInnerH |
                                                ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable |
                                                ImGuiTableFlags.Hideable | ImGuiTableFlags.ScrollX |
                                                ImGuiTableFlags.ScrollY;

        public List<SearchResult> SearchResults { get; set; } = new List<SearchResult>();
        public List<SearchResult> RenderSearchResults { get; set; } = new List<SearchResult>();
        public bool InitialColumnSetupDone { get; set; }

        public RenderTableBase(RightClickService rightClickService, InventoryToolsConfiguration configuration)
        {
            _rightClickService = rightClickService;
            _configuration = configuration;
        }

        public virtual void Initialize(FilterConfiguration filterConfiguration)
        {
            FilterConfiguration = filterConfiguration;
        }

        public string Name
        {
            get
            {
                return FilterConfiguration.Name;
            }
        }

        public virtual string Key
        {
            get
            {
                return FilterConfiguration.TableId;
            }
        }

        public virtual List<MessageBase>? DrawMenu(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SearchResult searchResult, int rowIndex)
        {
            var messages = new List<MessageBase>();
            var hoveredRow = -1;
            ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, configuration.TableHeight) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow)) {
                hoveredRow = rowIndex;
            }
            if (hoveredRow == rowIndex && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick" + rowIndex);
            }
            if (searchResult.CraftItem != null)
            {
                using (var popup = ImRaii.Popup("RightClick" + rowIndex))
                {
                    if (popup.Success)
                    {
                        _rightClickService.DrawRightClickPopup(searchResult, messages, configuration);
                    }
                }
            }
            else
            {
                using (var popup = ImRaii.Popup("RightClick" + rowIndex))
                {
                    using var _ = ImRaii.PushId("RightClick" + rowIndex);
                    if (popup.Success)
                    {
                        _rightClickService.DrawRightClickPopup(searchResult, messages, configuration);
                    }
                }
            }

            return messages;
        }

        public virtual List<ColumnConfiguration> Columns { get; set; } = new ();
        public int? SortColumn { get; set; }
        public ImGuiSortDirection? SortDirection { get; set; }
        public int? FreezeCols { get; set; }
        public int? FreezeRows { get; set; }
        public bool ShowFilterRow { get; set; } = true;
        public bool NeedsRefresh { get; set; }
        public bool NeedsColumnRefresh { get; set; }
        public bool Refreshing { get; set; } 
        public bool IsSearching { get; set; }
        public FilterConfiguration FilterConfiguration { get; set; }
        public bool HighlightItems => _configuration.ActiveUiFilter == FilterConfiguration.Key;

        public abstract void RefreshColumns();

        public abstract List<MessageBase> Draw(Vector2 size, bool shouldDraw = true);
        public abstract void DrawFooterItems();
    }
}
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

        public List<SortingResult> SortedItems { get; set; } = new List<SortingResult>();
        public List<SortingResult> RenderSortedItems { get; set; } = new List<SortingResult>();
        public List<ItemEx> Items { get; set; } = new List<ItemEx>();
        public List<ItemEx> RenderItems { get; set; } = new List<ItemEx>();
        public List<InventoryChange> InventoryChanges { get; set; } = new List<InventoryChange>();
        public List<InventoryChange> RenderInventoryChanges { get; set; } = new List<InventoryChange>();
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
            InventoryItem item, int rowIndex)
        {
            return DrawMenu(configuration, columnConfiguration, item.Item, rowIndex);
        }

        public virtual List<MessageBase>? DrawMenu(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            SortingResult item, int rowIndex)
        {
            return DrawMenu(configuration, columnConfiguration, item.InventoryItem.Item, rowIndex);
        }

        public virtual List<MessageBase>? DrawMenu(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            InventoryChange item, int rowIndex)
        {
            return DrawMenu(configuration, columnConfiguration, item.InventoryItem, rowIndex);
        }

        public virtual List<MessageBase>? DrawMenu(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            CraftItem item, int rowIndex)
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

            using (var popup = ImRaii.Popup("RightClick" + rowIndex))
            {
                if (popup.Success)
                {
                    _rightClickService.DrawRightClickPopup(item, configuration, messages);
                }
            }

            return messages;
        }

        

        public virtual List<MessageBase>? DrawMenu(FilterConfiguration configuration,
            ColumnConfiguration columnConfiguration,
            ItemEx item, int rowIndex)
        {
            var messages = new List<MessageBase>();
            var hoveredRow = -1;
            ImGui.Selectable("", false, ImGuiSelectableFlags.SpanAllColumns & ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, configuration.TableHeight) * ImGui.GetIO().FontGlobalScale);
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled & ImGuiHoveredFlags.AllowWhenOverlapped & ImGuiHoveredFlags.AllowWhenBlockedByPopup & ImGuiHoveredFlags.AllowWhenBlockedByActiveItem & ImGuiHoveredFlags.AnyWindow)) {
                hoveredRow = rowIndex;
            }
            if (hoveredRow == rowIndex && ImGui.IsMouseReleased(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("RightClick" + rowIndex);
            }

            using (var popup = ImRaii.Popup("RightClick" + rowIndex))
            {
                using var _ = ImRaii.PushId("RightClick" + rowIndex);
                if (popup.Success)
                {
                    _rightClickService.DrawRightClickPopup(item, messages);
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
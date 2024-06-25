using System.Collections.Generic;
using System.Numerics;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using Dalamud.Plugin.Services;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Lumina.Excel.GeneratedSheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns
{
    public class IconColumn : GameIconColumn
    {
        public IconColumn(ILogger<IconColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
        {
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
                ImGui.PushID("icon" + rowIndex);
                if (ImGui.ImageButton(ImGuiService.GetIconTexture(currentValue.Value.Item1, currentValue.Value.Item2).ImGuiHandle, new Vector2(filterConfiguration.TableHeight - 1, filterConfiguration.TableHeight - 1) * ImGui.GetIO().FontGlobalScale,new Vector2(0,0), new Vector2(1,1), 2))
                {
                    ImGui.PopID();
                    messages.Add(new OpenUintWindowMessage(typeof(ItemWindow), searchResult.Item.ItemId));
                }
                ImGui.PopID();
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
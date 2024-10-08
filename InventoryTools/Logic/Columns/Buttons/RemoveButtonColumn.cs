using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using OtterGui;

namespace InventoryTools.Logic.Columns.Buttons;

public class RemoveButtonColumn : ButtonColumn
{
    private readonly ImGuiService _imGuiService;

    public RemoveButtonColumn(ImGuiService imGuiService)
    {
        _imGuiService = imGuiService;
    }
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Buttons;
    public override bool HasFilter { get; set; } = false;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.None;

    public override FilterType AvailableIn => Logic.FilterType.CuratedList | Logic.FilterType.CraftFilter;

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, SearchResult searchResult,
        int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;

        if (searchResult.CraftItem != null && !searchResult.CraftItem.IsOutputItem)
        {
            return null;
        }
        if (ImGui.Button("X##RM" + rowIndex + "_" + columnIndex))
        {
            if (searchResult.CraftItem != null && searchResult.CraftItem.IsOutputItem)
            {
                configuration.CraftList.RemoveCraftItem(searchResult.CraftItem.ItemId);
            }
            else if(searchResult.CuratedItem != null)
            {
                configuration.RemoveCuratedItem(searchResult.CuratedItem);
            }
        }
        ImGuiUtil.HoverTooltip("Remove this item");

        return null;
    }

    public override string? RenderName { get; } = "";
    public override string Name { get; set; } = "Remove";
    public override float Width { get; set; } = 60;
    public override string HelpText { get; set; } = "Adds a button for quickly removing items from your list";
}
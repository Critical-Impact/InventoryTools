using System.Collections.Generic;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns.Buttons;

public class CraftButtonColumn : ButtonColumn
{
    private readonly IGameInterface _gameInterface;
    private readonly IChatUtilities _chatUtilities;

    public CraftButtonColumn(IGameInterface gameInterface, IChatUtilities chatUtilities)
    {
        _gameInterface = gameInterface;
        _chatUtilities = chatUtilities;
    }
    public override string Name { get; set; } = "Craft Button";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "A button that opens the crafting log for the item";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SearchResult searchResult, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            if (searchResult.Item.CanBeCrafted && ImGui.Button("Craft##" + rowIndex + "_" + columnIndex))
            {
                var result = _gameInterface.OpenCraftingLog(searchResult.Item.RowId);
                if (!result)
                {
                    _chatUtilities.PrintError("Could not open the crafting log, you are currently crafting.");
                }
            }
        }

        return null;
    }
}
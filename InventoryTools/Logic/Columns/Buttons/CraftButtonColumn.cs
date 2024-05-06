using System.Collections.Generic;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns.Buttons;

public class CraftButtonColumn : ButtonColumn
{
    private readonly IGameInterface _gameInterface;

    public CraftButtonColumn(IGameInterface gameInterface)
    {
        _gameInterface = gameInterface;
    }
    public override string Name { get; set; } = "Craft Button";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "A button that opens the crafting log for the item";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, ItemEx item, int rowIndex)
    {
        ImGui.TableNextColumn();
        if (item.CanBeCrafted && ImGui.Button("Craft"))
        {
            _gameInterface.OpenCraftingLog(item.RowId);
        }

        return null;
    }
}
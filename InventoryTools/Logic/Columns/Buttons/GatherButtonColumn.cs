using System.Collections.Generic;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns.Buttons;

public class GatherButtonColumn : ButtonColumn
{
    private readonly IGameInterface _gameInterface;

    public GatherButtonColumn(IGameInterface gameInterface)
    {
        _gameInterface = gameInterface;
    }
    public override string Name { get; set; } = "Gather Button";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Shows a button that calls gather buddy's /gather command";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration, ItemEx item, int rowIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.Button("Gather"))
        {
            if (item.ObtainedFishing)
            {
                _gameInterface.OpenFishingLog(item.RowId, item.IsSpearfishingItem());
            }
            else
            {
                _gameInterface.OpenGatheringLog(item.RowId);
            }
        }

        return null;
    }
}
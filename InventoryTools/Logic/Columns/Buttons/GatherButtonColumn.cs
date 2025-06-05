using System.Collections.Generic;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
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
    public override string Name { get; set; } = "Gathering Log Button";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "Shows a button that opens the item's gathering/fishing log entry if applicable.";

    public override List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SearchResult searchResult, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (!ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled)) return null;

        if (ImGui.Button("Log##" + rowIndex + "_" + columnIndex))
        {
            if (searchResult.Item.ObtainedFishing)
            {
                _gameInterface.OpenFishingLog(searchResult.Item.RowId, searchResult.Item.ObtainedSpearFishing);
            }
            else
            {
                _gameInterface.OpenGatheringLog(searchResult.Item.RowId);
            }
        }

        return null;
    }
}
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class HasBeenGatheredColumn : CheckboxColumn
{
    private readonly IGameInterface _gameInterface;

    public HasBeenGatheredColumn(ILogger<HasBeenGatheredColumn> logger, ImGuiService imGuiService, IGameInterface gameInterface) : base(logger, imGuiService)
    {
        _gameInterface = gameInterface;
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;

    public override bool? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        return _gameInterface.IsItemGathered(searchResult.Item.RowId);
    }

    public override string Name { get; set; } = "Logged in Gathering Log?";
    public override string RenderName => "Logged?";
    public override float Width { get; set; } = 80;

    public override string HelpText { get; set; } =
        "Has this gathering item been gathered at least once by the currently logged in character? This only supports mining and botany at present.";

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Boolean;
}
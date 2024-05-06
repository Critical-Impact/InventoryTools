using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class SourceWorldColumn : TextColumn
{
    private readonly ICharacterMonitor _characterMonitor;

    public SourceWorldColumn(ILogger<SourceWorldColumn> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor) : base(logger, imGuiService)
    {
        _characterMonitor = characterMonitor;
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Inventory;
    public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        var character = _characterMonitor.GetCharacterById(item.RetainerId);
        return character != null ? character.World?.FormattedName ?? "" : "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return CurrentValue(columnConfiguration, item.InventoryItem);
    }

    public override string Name { get; set; } = "Source World";
    public override float Width { get; set; } = 120;

    public override string HelpText { get; set; } =
        "The world where the item is stored(be it in a character, retainer, free company)";

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}
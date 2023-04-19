using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class SourceWorldColumn : TextColumn
{
    public override string? CurrentValue(InventoryItem item)
    {
        var character = PluginService.CharacterMonitor.GetCharacterById(item.RetainerId);
        return character != null ? character.World?.FormattedName ?? "" : "";
    }

    public override string? CurrentValue(ItemEx item)
    {
        return "";
    }

    public override string? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Source World";
    public override float Width { get; set; } = 120;

    public override string HelpText { get; set; } =
        "The world where the item is stored(be it in a character, retainer, free company)";

    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}
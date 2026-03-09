using Lumina.Excel;

namespace InventoryTools.Compendium.Sections;

public sealed class SingleRowRefSectionOptions
{
    public string? SectionName { get; init; }
    public RowRef RelatedRef { get; init; }
}
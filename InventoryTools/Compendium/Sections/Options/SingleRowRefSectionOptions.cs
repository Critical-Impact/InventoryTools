using Lumina.Excel;

namespace InventoryTools.Compendium.Sections.Options;

public record SingleRowRefSectionOptions : SectionOptions
{
    public RowRef RelatedRef { get; init; }
}
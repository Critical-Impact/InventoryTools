using InventoryTools.Compendium.Interfaces;
using Lumina.Excel;

namespace InventoryTools.Compendium.Sections.Options;

public record SingleRowRefSectionOptions : SectionOptions
{
    public RowRef RelatedRef { get; init; }

    public ICompendiumType? OverrideCompendiumType { get; init; }
}
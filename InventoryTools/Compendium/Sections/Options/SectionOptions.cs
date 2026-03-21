namespace InventoryTools.Compendium.Sections.Options;

public record SectionOptions
{
    public required string SectionName { get; init; }
    public bool HideHeader { get; init; }
}
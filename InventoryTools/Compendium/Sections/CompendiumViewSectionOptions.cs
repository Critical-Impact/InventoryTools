using System;
using System.Collections.Generic;
using DalaMock.Host.Mediator;

namespace InventoryTools.Compendium.Sections;

public record CompendiumViewSectionOptions
{
    public required string SectionName { get; init; }
    public string SectionId => SectionName.ToLower().Replace(" ", "");
    public bool HideHeader { get; init; }
    public Func<List<MessageBase>>? OptionsMenu { get; init; }
    public Func<List<MessageBase>>? ActionMenu { get; init; }
}
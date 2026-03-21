using System;
using System.Collections.Generic;
using DalaMock.Host.Mediator;

namespace InventoryTools.Compendium.Sections.Options;

public record ViewSectionOptions : SectionOptions
{
    public string SectionId => SectionName.ToLower().Replace(" ", "");
    public Func<List<MessageBase>>? OptionsMenu { get; init; }
    public Func<List<MessageBase>>? ActionMenu { get; init; }
}
using System.Collections.Generic;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class CharacterOwnerColumn : TextColumn
{
    private readonly ICharacterMonitor _characterMonitor;
    private Dictionary<ulong, string> _characterOwners;

    public CharacterOwnerColumn(ICharacterMonitor characterMonitor, ILogger<CharacterOwnerColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _characterMonitor = characterMonitor;
        _characterMonitor.OnCharacterUpdated += CharacterMonitorOnOnCharacterUpdated;
        _characterOwners = new();
    }

    private void CharacterMonitorOnOnCharacterUpdated(Character? character)
    {
        _characterOwners = new();
    }

    public override string Name { get; set; } = "Character Owner";
    public override float Width { get; set; } = 100;

    public override string HelpText { get; set; } =
        "Display's the name of the owner of the character this item is on if applicable(retainers, free companies, etc)";

    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Inventory;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        var item = searchResult.InventoryItem;
        if (item == null)
        {
            return null;
        }
        var characterOwners = _characterOwners;

        if (characterOwners.TryGetValue(item.RetainerId, out var value))
        {
            return value;
        }
        Character? character = _characterMonitor.GetCharacterById(item.RetainerId);

        if (character == null)
        {
            characterOwners[item.RetainerId] = "Unknown";
            return null;
        }

        if (character is { OwnerId: 0, Owners.Count: 0, CharacterType: not CharacterType.FreeCompanyChest })
        {
            characterOwners[item.RetainerId] = "";
            return characterOwners[item.RetainerId];
        }
        var mainOwner = _characterMonitor.GetCharacterById(character.OwnerId);
        if (mainOwner != null)
        {
            characterOwners[item.RetainerId] = mainOwner.FormattedName;
        }

        if (character.CharacterType == CharacterType.FreeCompanyChest)
        {
            var freeCompanyCharacters = _characterMonitor.GetFreeCompanyCharacters(character.CharacterId);
            foreach (var freeCompanyCharacter in freeCompanyCharacters)
            {
                if (!characterOwners.ContainsKey(item.RetainerId))
                {
                    characterOwners[item.RetainerId] = freeCompanyCharacter.Value.FormattedName;
                }
                else
                {
                    characterOwners[item.RetainerId] += ", " + freeCompanyCharacter.Value.FormattedName;
                }
            }
        }

        foreach (var subOwnerId in character.Owners)
        {
            var subOwner = _characterMonitor.GetCharacterById(subOwnerId);
            if (subOwner != null)
            {
                if (!characterOwners.ContainsKey(item.RetainerId))
                {
                    characterOwners[item.RetainerId] = subOwner.FormattedName;
                }
                else
                {
                    characterOwners[item.RetainerId] += ", " + subOwner.FormattedName;
                }
            }
        }

        return characterOwners[item.RetainerId];
    }

    public override void Dispose()
    {
        _characterMonitor.OnCharacterUpdated -= CharacterMonitorOnOnCharacterUpdated;
        base.Dispose();
    }
}
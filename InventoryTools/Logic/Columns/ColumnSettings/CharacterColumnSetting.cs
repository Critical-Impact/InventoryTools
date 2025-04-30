using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services;
using CsvHelper;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public sealed class CharacterColumnSetting : ChoiceColumnSetting<ulong?>
{
    private readonly ICharacterMonitor _characterMonitor;
    private readonly HashSet<CharacterType> _characterTypes;
    private readonly bool _activeOnly;

    public delegate CharacterColumnSetting Factory(string name, string helpText, HashSet<CharacterType> characterTypes, bool activeOnly);

    public CharacterColumnSetting(ILogger<CharacterColumnSetting> logger, ImGuiService imGuiService, ICharacterMonitor characterMonitor, string name, string helpText, HashSet<CharacterType> characterTypes, bool activeOnly) : base(logger, imGuiService)
    {
        _characterMonitor = characterMonitor;
        _characterTypes = characterTypes;
        _activeOnly = activeOnly;
        this.Name = name;
        this.HelpText = helpText;
    }

    public override ulong? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out ulong? value);
        return value;
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting(Key, (ulong?)null);
    }

    public override bool HasValueSet(ColumnConfiguration configuration)
    {
        return CurrentValue(configuration) != DefaultValue;
    }

    public override List<ulong?> GetChoices(ColumnConfiguration configuration)
    {
        var currentCharacterId = _characterMonitor.ActiveCharacterId;
        return _characterMonitor.Characters.Where(c => !_activeOnly || c.Value.OwnerId == currentCharacterId || c.Key == currentCharacterId).Where(c => _characterTypes.Contains(c.Value.CharacterType)).Select(c => (ulong?)c.Key).ToList();
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, ulong? choice)
    {
        if (choice == null)
        {
            return "None";
        }

        return _characterMonitor.GetCharacterNameById(choice.Value);
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, ulong? newValue)
    {
        configuration.SetSetting(Key, newValue);
    }

    public override string Key { get; set; } = "character";
    public override string Name { get; set; } = "Character";
    public override string HelpText { get; set; } = "The character to use";
    public override ulong? DefaultValue { get; set; } = null;

}
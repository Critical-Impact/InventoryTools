using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using InventoryTools.Extensions;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class QualitySelectorSetting : MultiChoiceColumnSetting<(FFXIVClientStructs.FFXIV.Client.Game.InventoryItem.ItemFlags,string)>
{
    public override string EmptyText => "All";

    public QualitySelectorSetting(ILogger<QualitySelectorSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
    public override List<(InventoryItem.ItemFlags,string)>? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out List<InventoryItem.ItemFlags>? value);
        if (value == null)
        {
            return null;
        }

        return value.Select(c => (c, c.FormattedName())).ToList();
    }


    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting<InventoryItem.ItemFlags>(Key, null);
    }


    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, List<(InventoryItem.ItemFlags, string)>? newValue)
    {
        configuration.SetSetting(Key, newValue?.Count == 0 ? null : newValue?.Select(c => c.Item1).ToList());
    }


    public override string Key { get; set; } = "QualitySelector";
    public override string Name { get; set; } = "Qualities";
    public override string HelpText { get; set; } = "Which quality of items should be counted?";
    public override List<(InventoryItem.ItemFlags,string)>? DefaultValue { get; set; } = null;
    public override List<(InventoryItem.ItemFlags,string)> GetChoices(ColumnConfiguration configuration)
    {
        return
        [
            (InventoryItem.ItemFlags.None, InventoryItem.ItemFlags.None.FormattedName()), (InventoryItem.ItemFlags.HighQuality, InventoryItem.ItemFlags.HighQuality.FormattedName()),
            (InventoryItem.ItemFlags.Collectable, InventoryItem.ItemFlags.Collectable.FormattedName())
        ];
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, (InventoryItem.ItemFlags,string) choice)
    {
        return choice.Item2;
    }
}
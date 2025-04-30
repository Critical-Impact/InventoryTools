using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class UseTypeSelectorSetting : MultiChoiceColumnSetting<(ItemInfoType,string)>
{
    private readonly ItemInfoRenderService _itemInfoRenderService;
    public override string EmptyText => "All";

    public UseTypeSelectorSetting(ILogger<MarketboardWorldSetting> logger, ImGuiService imGuiService, ItemInfoRenderService itemInfoRenderService) : base(logger, imGuiService)
    {
        _itemInfoRenderService = itemInfoRenderService;
    }
    public override List<(ItemInfoType,string)>? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out List<ItemInfoType>? value);
        if (value == null)
        {
            return null;
        }

        return value.Select(c => (c, _itemInfoRenderService.GetUseTypeName(c).Plural ?? _itemInfoRenderService.GetUseTypeName(c).Singular)).ToList();
    }


    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting(Key, (List<ItemInfoType>?)null);
    }


    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, List<(ItemInfoType, string)>? newValue)
    {
        configuration.SetSetting(Key, newValue?.Count == 0 ? null : newValue?.Select(c => c.Item1).ToList());
    }


    public override string Key { get; set; } = "UseTypes";
    public override string Name { get; set; } = "Types";
    public override string HelpText { get; set; } = "Which use types should this display?";
    public override List<(ItemInfoType,string)>? DefaultValue { get; set; } = null;
    public override List<(ItemInfoType,string)> GetChoices(ColumnConfiguration configuration)
    {
        List<(ItemInfoType itemInfoType, string FormattedName)> itemInfoTypes = Enum.GetValues<ItemInfoType>().Where(c => _itemInfoRenderService.HasUseRenderer(c)).Select(c => (c, _itemInfoRenderService.GetUseTypeName(c).Plural ?? _itemInfoRenderService.GetUseTypeName(c).Singular)).OrderBy(c => c.Item2).ToList();
        return itemInfoTypes;
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, (ItemInfoType,string) choice)
    {
        return choice.Item2;
    }
}
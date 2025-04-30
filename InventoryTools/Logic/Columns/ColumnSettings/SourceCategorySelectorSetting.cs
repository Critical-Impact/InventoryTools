using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using CriticalCommonLib.Models;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class SourceCategorySelectorSetting : MultiChoiceColumnSetting<(ItemInfoRenderCategory,string)>
{
    private readonly ItemInfoRenderService _itemInfoRenderService;
    public override string EmptyText => "All";

    public SourceCategorySelectorSetting(ILogger<MarketboardWorldSetting> logger, ImGuiService imGuiService, ItemInfoRenderService itemInfoRenderService) : base(logger, imGuiService)
    {
        _itemInfoRenderService = itemInfoRenderService;
    }
    public override List<(ItemInfoRenderCategory,string)>? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out List<ItemInfoRenderCategory>? value);
        if (value == null)
        {
            return null;
        }

        return value.Select(c => (c, _itemInfoRenderService.GetCategoryName(c))).ToList();
    }


    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting(Key, (List<ItemInfoRenderCategory>?)null);
    }


    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, List<(ItemInfoRenderCategory, string)>? newValue)
    {
        configuration.SetSetting(Key, newValue?.Count == 0 ? null : newValue?.Select(c => c.Item1).ToList());
    }


    public override string Key { get; set; } = "SourceCategories";
    public override string Name { get; set; } = "Categories";
    public override string HelpText { get; set; } = "Which source categories should this display?";
    public override List<(ItemInfoRenderCategory,string)>? DefaultValue { get; set; } = null;
    public override List<(ItemInfoRenderCategory,string)> GetChoices(ColumnConfiguration configuration)
    {
        List<(ItemInfoRenderCategory itemInfoRenderCategory, string FormattedName)> itemInfoRenderCategorys = Enum.GetValues<ItemInfoRenderCategory>().Select(c => (c, _itemInfoRenderService.GetCategoryName(c))).OrderBy(c => c.Item2).ToList();
        return itemInfoRenderCategorys;
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, (ItemInfoRenderCategory,string) choice)
    {
        return choice.Item2;
    }
}
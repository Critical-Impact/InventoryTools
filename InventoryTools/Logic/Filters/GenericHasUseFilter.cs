using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class GenericHasUseFilter : BooleanFilter, IGenericFilter, IItemTypeFilter
{
    private readonly ItemInfoRenderService _infoRenderService;
    public ItemInfoType ItemType { get; }

    public override int LabelSize { get; set; } = 250;

    public delegate GenericHasUseFilter Factory(ItemInfoType itemType);
    public GenericHasUseFilter(ItemInfoType itemType, ItemInfoRenderService infoRenderService, ILogger<GenericHasUseFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _infoRenderService = infoRenderService;
        ItemType = itemType;
    }

    public override string Key {
        get
        {
            return "HasUse" + ItemType;
        }
        set
        {

        }
    }
    public override string Name {
        get => (_infoRenderService.GetUseTypeName(ItemType).Plural ?? _infoRenderService.GetUseTypeName(ItemType).Singular);
        set
        {

        }
    }
    public override string HelpText {
        get =>  _infoRenderService.GetUseHelpText(ItemType);
        set
        {

        }
    }

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Uses;
    public override bool? FilterItem(FilterConfiguration configuration, InventoryItem item)
    {
        return FilterItem(configuration, item.Item);
    }

    public override bool? FilterItem(FilterConfiguration configuration, ItemRow item)
    {
        var currentValue = this.CurrentValue(configuration);
        if (currentValue == null)
        {
            return null;
        }

        return currentValue == true ? item.HasUsesByType(ItemType) : !item.HasUsesByType(ItemType);
    }
}
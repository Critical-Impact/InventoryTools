using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public interface IItemTypeFilter : IFilter
{
    ItemInfoType ItemType { get; }
}

public class GenericHasSourceFilter : BooleanFilter, IGenericFilter, IItemTypeFilter
{
    private readonly ItemInfoRenderService _infoRenderService;
    public ItemInfoType ItemType { get; }

    public override int LabelSize { get; set; } = 250;

    public delegate GenericHasSourceFilter Factory(ItemInfoType itemType);

    public GenericHasSourceFilter(ItemInfoType itemType, ItemInfoRenderService infoRenderService, ILogger<GenericHasSourceFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _infoRenderService = infoRenderService;
        ItemType = itemType;
    }

    public override string Key {
        get
        {
            return "HasSource" + ItemType;
        }
        set
        {

        }
    }
    public override string Name {
        get => (_infoRenderService.GetSourceTypeName(ItemType).Plural ?? _infoRenderService.GetSourceTypeName(ItemType).Singular);
        set
        {

        }
    }
    public override string HelpText {
        get =>  _infoRenderService.GetSourceHelpText(ItemType);
        set
        {

        }
    }

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.Sources;
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

        return currentValue == true ? item.HasSourcesByType(ItemType) : !item.HasSourcesByType(ItemType);
    }
}
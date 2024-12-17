using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets.Rows;
using CriticalCommonLib.Models;
using InventoryTools.Logic.Filters.Abstract;
using InventoryTools.Logic.ItemRenderers;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Filters;

public class GenericHasUseCategoryFilter : BooleanFilter, IGenericFilter
{
    private readonly ItemInfoRenderCategory _renderCategory;
    private readonly ItemInfoRenderService _infoRenderService;
    private ItemInfoType[] _sourceTypes;

    public override int LabelSize { get; set; } = 250;

    public delegate GenericHasUseCategoryFilter Factory(ItemInfoRenderCategory renderCategory);
    public GenericHasUseCategoryFilter(ItemInfoRenderCategory renderCategory, ItemInfoRenderService infoRenderService, ILogger<GenericHasUseCategoryFilter> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _renderCategory = renderCategory;
        _infoRenderService = infoRenderService;
    }

    public override string Key {
        get
        {
            return "HasUseCat" + (uint)_renderCategory;
        }
        set
        {

        }
    }
    public override string Name {
        get => _infoRenderService.GetCategoryName(_renderCategory);
        set
        {

        }
    }
    public override string HelpText {
        get => "Can the item be used for " +  _infoRenderService.GetCategoryName(_renderCategory).ToLower() + "?\n\nIt includes these uses: " + string.Join(",", _infoRenderService.GetUsesByCategory(_renderCategory).Select(c => c.SingularName));
        set
        {

        }
    }

    public override FilterCategory FilterCategory { get; set; } = FilterCategory.UseCategories;
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

        _sourceTypes = _infoRenderService.GetUsesByCategory(_renderCategory).Select(c => c.Type).ToArray();

        return currentValue == true ? item.HasUsesByType(_sourceTypes) : !item.HasUsesByType(_sourceTypes);
    }
}
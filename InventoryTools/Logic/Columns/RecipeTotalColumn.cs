using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class RecipeTotalColumn : IntegerColumn
{
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Crafting;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override int? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override int? CurrentValue(ItemEx item)
    {
        item.RecipesAsRequirement.Count();
        return Service.ExcelCache.ItemRecipeCount(item.RowId);
    }

    public override int? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Recipe Total Count";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The number of recipes the item is a component of.";
}
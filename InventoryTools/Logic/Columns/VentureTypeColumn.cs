using System;
using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract;

namespace InventoryTools.Logic.Columns;

public class VentureTypeColumn : TextColumn
{
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override string? CurrentValue(InventoryItem item)
    {
        return CurrentValue(item.Item);
    }

    public override string? CurrentValue(ItemEx item)
    {
        return item.RetainerTaskNames;
    }

    public override string? CurrentValue(SortingResult item)
    {
        return CurrentValue(item.InventoryItem);
    }

    public override string Name { get; set; } = "Venture Type";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The type of ventures that the item can be acquired from";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Stats;

public class DelayColumn : DecimalColumn
{
    public DelayColumn(ILogger<DelayColumn> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override ColumnCategory ColumnCategory => ColumnCategory.Stats;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override decimal? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (searchResult.Item.Base.Delayms == 0)
        {
            return null;
        }
        return (decimal)searchResult.Item.Base.Delayms / 1000;
    }

    public override string Name { get; set; } = "Delay";
    public override float Width { get; set; } = 80;
    public override string HelpText { get; set; } = "The time it takes between each automatic attack while engaged with and in range of an enemy in seconds.";
}
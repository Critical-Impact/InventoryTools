using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using CriticalCommonLib.Sheets;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.Settings;

public class MarketboardWorldSetting : ChoiceColumnSetting<WorldEx>
{
    private readonly ExcelCache _excelCache;

    public MarketboardWorldSetting(ILogger<MarketboardWorldSetting> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }
    public override WorldEx? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(Key, out uint? value);
        if (value == null)
        {
            return null;
        }

        var world = _excelCache.GetWorldSheet().GetRow(value.Value);
        return world;
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting(Key, (uint?)null);
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, WorldEx? newValue)
    {
        configuration.SetSetting(Key, newValue?.RowId ?? null);
    }

    public override string Key { get; set; } = "MBWorld";
    public override string Name { get; set; } = "World";
    public override string HelpText { get; set; } = "The world for this column to display?";
    public override WorldEx? DefaultValue { get; set; } = null;
    public override List<WorldEx> GetChoices(ColumnConfiguration configuration)
    {
        return _excelCache.GetWorldSheet().Where(c => c.IsPublic == true).ToList();
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, WorldEx choice)
    {
        return choice.FormattedName;
    }
}
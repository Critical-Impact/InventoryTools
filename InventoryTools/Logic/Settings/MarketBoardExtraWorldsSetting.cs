using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib;
using CriticalCommonLib.Services;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class MarketBoardExtraWorldsSetting : MultipleChoiceSetting<uint>
{
    private readonly ExcelCache _excelCache;
    public override List<uint> DefaultValue { get; set; } = new List<uint>();
    public override List<uint> CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.MarketBoardWorldIds;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, List<uint> newValue)
    {
        configuration.MarketBoardWorldIds = newValue;
    }

    public override string Key { get; set; } = "MarketBoardExtraWorlds";
    public override string Name { get; set; } = "Price Worlds";
    public override string HelpText { get; set; } = "A list of extra worlds we should automatically price";
    public override SettingCategory SettingCategory { get; set; } = SettingCategory.MarketBoard;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Market;
    public override string Version { get; } = "1.6.2.5";
    private Dictionary<uint, string>? _worldNames;
    public override Dictionary<uint, string> GetChoices(InventoryToolsConfiguration configuration)
    {
        if (_worldNames == null)
        {
            _worldNames = _excelCache.GetWorldSheet().Where(c => c.IsPublic)
                .ToDictionary(c => c.RowId, c => c.FormattedName);
        }

        return _worldNames;
    }

    public override bool HideAlreadyPicked { get; set; } = true;

    public MarketBoardExtraWorldsSetting(ILogger<MarketBoardExtraWorldsSetting> logger, ImGuiService imGuiService, ExcelCache excelCache) : base(logger, imGuiService)
    {
        _excelCache = excelCache;
    }
}
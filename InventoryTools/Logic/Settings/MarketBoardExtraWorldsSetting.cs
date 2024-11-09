using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class MarketBoardExtraWorldsSetting : MultipleChoiceSetting<uint>
{
    private readonly ExcelSheet<World> _worldSheet;

    public MarketBoardExtraWorldsSetting(ILogger<MarketBoardExtraWorldsSetting> logger, ImGuiService imGuiService, ExcelSheet<World> worldSheet) : base(logger, imGuiService)
    {
        _worldSheet = worldSheet;
    }

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
    public override string Version { get; } = "1.7.0.0";
    private Dictionary<uint, string>? _worldNames;
    public override Dictionary<uint, string> GetChoices(InventoryToolsConfiguration configuration)
    {
        if (_worldNames == null)
        {
            _worldNames = _worldSheet.Where(c => c.IsPublic).OrderBy(c => c.Name.ExtractText())
                .ToDictionary(c => c.RowId, c => c.Name.ExtractText());
        }

        return _worldNames;
    }

    public override bool HideAlreadyPicked { get; set; } = true;
}
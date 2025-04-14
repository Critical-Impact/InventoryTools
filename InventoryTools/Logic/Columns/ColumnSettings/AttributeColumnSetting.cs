using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns.ColumnSettings;

public class AttributeColumnSetting : ChoiceColumnSetting<uint?>
{
    private readonly ExcelSheet<BaseParam> _paramSheet;
    private List<uint?>? baseParams;
    private Dictionary<uint, string>? baseParamNames;

    public AttributeColumnSetting(ILogger<AttributeColumnSetting> logger, ExcelSheet<BaseParam> paramSheet, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _paramSheet = paramSheet;
    }

    public override uint? CurrentValue(ColumnConfiguration configuration)
    {
        configuration.GetSetting(this.Key, out uint? value);
        return value ?? DefaultValue;
    }

    public override void ResetFilter(ColumnConfiguration configuration)
    {
        configuration.SetSetting(this.Key, DefaultValue);
    }

    public override void UpdateColumnConfiguration(ColumnConfiguration configuration, uint? newValue)
    {
        configuration.SetSetting(this.Key, newValue);
    }

    public override string Key { get; set; } = "Attribute";
    public override string Name { get; set; } = "Attribute";
    public override string HelpText { get; set; } = "The attribute to show";
    public override uint? DefaultValue { get; set; } = 1;
    public override List<uint?> GetChoices(ColumnConfiguration configuration)
    {
        return this.baseParams ??= _paramSheet.OrderBy(c => c.Name.ExtractText()).Select(c => (uint?)c.RowId).ToList();
    }

    public override string GetFormattedChoice(ColumnConfiguration filterConfiguration, uint? choice)
    {
        baseParamNames ??= _paramSheet.ToDictionary(c => c.RowId, c => c.Name.ExtractText());
        baseParamNames.TryGetValue(choice.Value, out string? value);
        return value ?? "Unknown";
    }
}
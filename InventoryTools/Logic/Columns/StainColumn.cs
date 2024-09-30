using System;
using CriticalCommonLib.Models;
using CriticalCommonLib.Sheets;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Logic.Columns.ColumnSettings;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public class StainColumn : TextColumn
{
    private readonly StainColumnSetting _stainColumnSetting;

    public StainColumn(ILogger<StainColumn> logger, StainColumnSetting stainColumnSetting, ImGuiService imGuiService) : base(logger, imGuiService)
    {
        _stainColumnSetting = stainColumnSetting;
    }
    public override ColumnCategory ColumnCategory => ColumnCategory.Basic;
    public override string? CurrentValue(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        switch (_stainColumnSetting.CurrentValue(columnConfiguration))
        {
            case StainColumnSettingEnum.FirstStain:
                return item.StainEntry?.Name ?? "";
            case StainColumnSettingEnum.SecondStain:
                return item.Stain2Entry?.Name ?? "";
            case StainColumnSettingEnum.Both:
                var firstStain = item.StainEntry?.Name ?? "No Dye";
                var secondStain = item.Stain2Entry?.Name ?? "No Dye";
                return firstStain + " / " + secondStain;
        }

        return "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return "";
    }

    public override string? CurrentValue(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return CurrentValue(columnConfiguration, item.InventoryItem);
    }

    public override void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
    {
        ImGui.NewLine();
        ImGui.Separator();
        _stainColumnSetting.Draw(columnConfiguration);
        base.DrawEditor(columnConfiguration, configuration);
    }

    public override string Name { get; set; } = "Dye";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "The current dye of the item";
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Logic.Columns.Abstract;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui.Widgets;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Columns;

public enum DesynthResult
{
    Optimal,
    TooLow,
    Max
}

public class DesynthesisSkillDeltaColumn : Column<(decimal, DesynthResult)?>
{
    private readonly IPlayerState _playerState;
    private readonly ExcelSheet<ClassJob> _classJobSheet;
    private readonly uint _maxDesynthLevel = 590;
    private Dictionary<uint, decimal> _desynthLevels = new Dictionary<uint, decimal>();
    private DateTime? _lastUpdate;

    public DesynthesisSkillDeltaColumn(ILogger<DesynthesisSkillDeltaColumn> logger, ImGuiService imGuiService, IPlayerState playerState, ExcelSheet<ClassJob> classJobSheet) : base(logger, imGuiService)
    {
        _playerState = playerState;
        _classJobSheet = classJobSheet;
    }

    private void UpdateDesynthLevels()
    {
        if (_lastUpdate == null || _lastUpdate.Value.AddSeconds(5) <= DateTime.Now)
        {
            _lastUpdate = DateTime.Now;
            UpdateDesynthLevels();
            if (!_playerState.IsLoaded)
            {
                return;
            }

            foreach (var classJob in _classJobSheet.Where(c => c.DohDolJobIndex != -1))
            {
                var desynthesisLevel = (decimal)_playerState.GetDesynthesisLevel(classJob);
                if (!_desynthLevels.TryGetValue(classJob.RowId, out var value) || desynthesisLevel != value)
                {
                    value = desynthesisLevel;
                    _desynthLevels[classJob.RowId] = value;
                }
            }
        }
    }

    public override ColumnCategory ColumnCategory => ColumnCategory.Desynthesis;
    public override bool HasFilter { get; set; } = true;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> searchResults)
    {
        return columnConfiguration.FilterText == "" ? searchResults : searchResults.Where(c =>
        {
            var currentValue = CurrentValue(columnConfiguration, c);
            if (currentValue == null)
            {
                return false;
            }

            var text = "";
            switch (currentValue.Value.Item2)
            {
                case DesynthResult.Optimal:
                    text = "optimal";
                    break;
                case DesynthResult.TooLow:
                    text =  "too low";
                    break;
                case DesynthResult.Max:
                    text =  "max";
                    break;
            }

            return currentValue.Value.Item1.PassesFilter(columnConfiguration.FilterText) || text.PassesFilter(columnConfiguration.FilterText);
        });
    }

    public override IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<SearchResult> searchResults)
    {
        return direction == ImGuiSortDirection.Ascending ? searchResults.OrderBy(c => CurrentValue(columnConfiguration, c)?.Item1 ?? Int32.MaxValue) : searchResults.OrderByDescending(c => CurrentValue(columnConfiguration, c)?.Item1 ?? Int32.MinValue);

    }

    public override List<MessageBase>? Draw(FilterConfiguration configuration,
        ColumnConfiguration columnConfiguration,
        SearchResult searchResult, int rowIndex, int columnIndex)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            var currentValue = CurrentValue(columnConfiguration, searchResult);
            if (currentValue == null)
            {
                return null;
            }

            Vector4 color = ImGuiColors.DalamudGrey;
            var text = "";
            switch (currentValue.Value.Item2)
            {
                case DesynthResult.Optimal:
                    text = "Optimal";
                    color = ImGuiColors.HealerGreen;
                    break;
                case DesynthResult.TooLow:
                    text =  "Too Low";
                    color = ImGuiColors.DalamudYellow;
                    break;
                case DesynthResult.Max:
                    text =  "Max";
                    color = ImGuiColors.DalamudRed;
                    break;
            }

            using (ImRaii.PushColor(ImGuiCol.Text, color))
            {
                var value = currentValue.Value.Item1.ToString("N0", CultureInfo.InvariantCulture);
                ImGuiUtil.VerticalAlignText($"{text} ({value})", configuration.TableHeight, false);
            }

        }

        return null;
    }

    public override List<MessageBase>? DoDraw(SearchResult searchResult, (decimal, DesynthResult)? currentValue, int rowIndex,
        FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration)
    {
        return null;
    }

    public override string CsvExport(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        var currentValue = CurrentValue(columnConfiguration, searchResult);
        if (currentValue == null)
        {
            return "";
        }

        var text = currentValue.Value.Item2 switch
        {
            DesynthResult.Optimal => "Optimal",
            DesynthResult.TooLow => "Too Low",
            DesynthResult.Max => "Max",
            _ => ""
        };

        var value = (currentValue.Value.Item1).ToString("N0", CultureInfo.InvariantCulture);
        return $"{text} ({value})";
    }

    public override (decimal, DesynthResult)? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        if (!searchResult.Item.CanBeDesynthed)
        {
            return null;
        }

        if (!_playerState.IsLoaded)
        {
            return null;
        }

        UpdateDesynthLevels();

        var desynthesisLevel = _desynthLevels.GetValueOrDefault(searchResult.Item.Base.ClassJobRepair.RowId, 0);

        var result = DesynthResult.Optimal;

        if (desynthesisLevel >= _maxDesynthLevel)
        {
            result = DesynthResult.Max;
        }

        var delta = searchResult.Item.Base.LevelItem.RowId - (decimal)desynthesisLevel;

        if (delta <= -50) {
            result = DesynthResult.TooLow;
        }

        return (delta, result);
    }


    public override string Name { get; set; } = "Desynthesis Skill Delta";
    public override float Width { get; set; } = 100;

    public override string HelpText { get; set; } =
        "Shows the difference between the iLvl of the item and your desynthesis skill and if desynthesis is optimal";

}
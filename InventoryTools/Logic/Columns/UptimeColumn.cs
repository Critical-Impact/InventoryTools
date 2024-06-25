namespace InventoryTools.Logic.Columns;

using System.Collections.Generic;
using Abstract;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using CriticalCommonLib.Time;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Services;

public class UptimeColumn : TimeIntervalColumn
{
    private readonly ISeTime seTime;

    public UptimeColumn(ILogger<UptimeColumn> logger, ImGuiService imGuiService, ISeTime seTime) : base(logger, imGuiService)
    {
        this.seTime = seTime;
    }

    public override string Name { get; set; } = "Next Gather Uptime";
    public override float Width { get; set; } = 100;
    public override string HelpText { get; set; } = "Shows how long an item will be available to gather if it's already spawned, and when the next time an item will be available to gather";
    public override ColumnCategory ColumnCategory { get; } = ColumnCategory.Basic;
    public override bool HasFilter { get; set; } = false;
    public override ColumnFilterType FilterType { get; set; } = ColumnFilterType.Text;

    public override List<MessageBase>? DoDraw(SearchResult searchResult, TimeInterval? currentValue, int rowIndex, FilterConfiguration filterConfiguration,
        ColumnConfiguration columnConfiguration)
    {
        ImGui.TableNextColumn();
        if (ImGui.TableGetColumnFlags().HasFlag(ImGuiTableColumnFlags.IsEnabled))
        {
            if (currentValue.HasValue)
            {
                if (currentValue.Value.Start > TimeStamp.UtcNow)
                {
                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
                    {
                        ImGui.Text("Up in " +
                                   TimeInterval.DurationString(currentValue.Value.Start, TimeStamp.UtcNow,
                                       true));
                    }
                }
                else
                {
                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen))
                    {
                        ImGui.Text("Up for " +
                                   TimeInterval.DurationString(currentValue.Value.End, TimeStamp.UtcNow,
                                       true));
                    }
                }
            }
        }


        return null;
    }

    public override TimeInterval? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        var gatheringUptime = searchResult.Item.GetGatheringUptime();
        if (gatheringUptime != null)
        {
            var nextUptime = gatheringUptime.Value.NextUptime(seTime.ServerTime);
            if (nextUptime.Equals(TimeInterval.Always)
                || nextUptime.Equals(TimeInterval.Invalid)
                || nextUptime.Equals(TimeInterval.Never)) return null;
            return nextUptime;
        }

        return null;
    }
}
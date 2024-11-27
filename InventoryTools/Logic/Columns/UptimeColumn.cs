using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Shared.Time;
using Dalamud.Interface.Textures;
using Lumina.Extensions;


namespace InventoryTools.Logic.Columns;

using System.Collections.Generic;
using Abstract;
using CriticalCommonLib.Services.Mediator;


using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Services;

public class UptimeColumn : TimeIntervalColumn
{
    private readonly ISeTime _seTime;
    private readonly MapSheet _mapSheet;

    public UptimeColumn(ILogger<UptimeColumn> logger, ImGuiService imGuiService, ISeTime seTime, MapSheet mapSheet) : base(logger, imGuiService)
    {
        this._seTime = seTime;
        _mapSheet = mapSheet;
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


                ImGui.SameLine();
                var wrap = ImGuiService.TextureProvider.GetFromGameIcon(new GameIconLookup(66317)).GetWrapOrEmpty();
                ImGui.Image(wrap.ImGuiHandle, new(16, 16));

                if (ImGui.IsItemHovered())
                {
                    using (var tooltip = ImRaii.Tooltip())
                    {
                        if (tooltip.Success)
                        {
                            var pointsWithUpTimes = searchResult.Item.GatheringPoints.Where(c => c.GatheringPointTransient.GetGatheringUptime() != null).DistinctBy(c => c.GatheringPointTransient.GetGatheringUptime());
                            foreach (var nextUptime in pointsWithUpTimes.Select(row => (row, row.GatheringPointTransient.GetGatheringUptime()!.Value.NextUptime(_seTime.ServerTime))).Where(c => !c.Item2.Equals(TimeInterval.Always) && !c.Item2.Equals(TimeInterval.Invalid) && !c.Item2.Equals(TimeInterval.Never)).OrderBy(c => c.Item2))
                            {
                                var map = _mapSheet.GetRow(nextUptime.row.Base.TerritoryType.Value.Map.RowId);
                                ImGui.Text(map.FormattedName + ": ");
                                ImGui.SameLine();
                                if (nextUptime.Item2.Start > TimeStamp.UtcNow)
                                {
                                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudRed))
                                    {
                                        ImGui.Text( " (Up in " +
                                                   TimeInterval.DurationString(nextUptime.Item2.Start, TimeStamp.UtcNow,
                                                       true) + ")");
                                    }
                                }
                                else
                                {
                                    using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen))
                                    {
                                        ImGui.Text(" (Up for " +
                                                   TimeInterval.DurationString(nextUptime.Item2.End, TimeStamp.UtcNow,
                                                       true) + ")");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        return null;
    }

    public override TimeInterval? CurrentValue(ColumnConfiguration columnConfiguration, SearchResult searchResult)
    {
        var gatheringUptime = searchResult.Item.GatheringUpTimes.Select(c => c.NextUptime(_seTime.ServerTime)).Where(c => !c.Equals(TimeInterval.Always) && !c.Equals(TimeInterval.Invalid) && !c.Equals(TimeInterval.Never)).OrderBy(c => c).FirstOrNull();
        return gatheringUptime;
    }
}
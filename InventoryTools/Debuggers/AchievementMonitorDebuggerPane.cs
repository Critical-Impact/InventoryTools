using System.Collections.Generic;
using System.Linq;
using AllaganLib.Monitors.Interfaces;
using AllaganLib.Shared.Extensions;
using CriticalCommonLib;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Debuggers;

public class AchievementDebuggerPane : DebugLogPane
{
    private readonly IAchievementMonitorService _achievementMonitorService;

    public AchievementDebuggerPane(IAchievementMonitorService achievementMonitorService)
    {
        _achievementMonitorService = achievementMonitorService;
    }

    public override string Name => "Achievement Monitor";

    public override void SubscribeToEvents()
    {
    }

    public override void DrawInfo()
    {
        if (ImGui.CollapsingHeader("Status"))
        {
            ImGui.TextUnformatted($"Loaded: {_achievementMonitorService.IsLoaded}");
            ImGui.TextUnformatted($"Completed Achievement Count: {_achievementMonitorService.GetCompletedAchievementIds().Count}");
        }

        if (ImGui.CollapsingHeader("Completed Achievements"))
        {
            var completed = _achievementMonitorService.GetCompletedAchievements();

            if (completed.Count == 0)
            {
                ImGui.TextUnformatted("<none>");
            }
            else
            {
                foreach (var rowRef in completed.OrderBy(r => r.RowId))
                {
                    var name = rowRef.ValueNullable?.Name.ToImGuiString() ?? $"<unknown name>";
                    ImGui.TextUnformatted($"ID={rowRef.RowId}, Name={name}");
                }
            }
        }

        if (ImGui.CollapsingHeader("Configuration"))
        {
            var config = _achievementMonitorService.Configuration;
            if (config == null)
            {
                ImGui.TextUnformatted("<no configuration>");
            }
            else
            {
                // Print configuration recursively
                Utils.PrintOutObject(config, 0, new List<string>());
            }
        }
    }
}
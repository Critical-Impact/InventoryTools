using CriticalCommonLib.Services;
using DalaMock.Shared.Interfaces;
using ImGuiNET;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui;

namespace InventoryTools.Logic.Settings;

public class TrackMobSpawnSetting : BooleanSetting
{
    private readonly IFileDialogManager _fileDialogManager;
    private readonly IMobTracker _mobTracker;

    public TrackMobSpawnSetting(ILogger<TrackMobSpawnSetting> logger, ImGuiService imGuiService, IFileDialogManager fileDialogManager, IMobTracker mobTracker) : base(logger, imGuiService)
    {
        _fileDialogManager = fileDialogManager;
        _mobTracker = mobTracker;
    }
    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TrackMobSpawns;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TrackMobSpawns = newValue;
    }

    public override string Key { get; set; } = "TrackMobSpawns";
    public override string Name { get; set; } = "Track Mob Spawns";

    public override string HelpText { get; set; } =
        "Should the plugin track where mobs spawn as you move around. This data is not used by the plugin yet but once you have collected enough you can hit the button next to the checkbox to export a file containing those positions. If you upload those CSVs and send a url to via feedback I can use that spawn data to provide accurate mob spawns for everyone.";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.MobSpawnTracker;

    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.General;

    public override void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
        bool? disableColouring)
    {
        base.Draw(configuration, null, null, null);
        if (configuration.TrackMobSpawns)
        {
            ImGui.SameLine();
            if (ImGui.Button("Export CSV"))
            {
                _fileDialogManager.SaveFileDialog("Save to csv", "*.csv", "mob_spawns.csv", ".csv",
                    (b, s) => { SaveMobSpawns(b, s); }, null, true);
            }

            ImGuiUtil.HoverTooltip("Export a CSV containing the mob spawn IDs and their positions.");
        }
    }

    private void SaveMobSpawns(bool b, string s)
    {
        if (b)
        {
            var entries = _mobTracker.GetEntries();
            _mobTracker.SaveCsv(s, entries);
        }
    }
    public override string Version => "1.7.0.0";
}
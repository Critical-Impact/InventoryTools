using ImGuiNET;
using InventoryTools.Logic.Settings.Abstract;
using OtterGui;

namespace InventoryTools.Logic.Settings;

public class TrackMobSpawnSetting : BooleanSetting
{
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

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Experimental;

    public override void Draw(InventoryToolsConfiguration configuration)
    {
        base.Draw(configuration);
        if (configuration.TrackMobSpawns)
        {
            ImGui.SameLine();
            if (ImGui.Button("Export CSV"))
            {
                PluginService.FileDialogManager.SaveFileDialog("Save to csv", "*.csv", "mob_spawns.csv", ".csv",
                    (b, s) => { SaveMobSpawns(b, s); }, null, true);
            }

            ImGuiUtil.HoverTooltip("Export a CSV containing the mob spawn IDs and their positions.");
        }
    }

    private void SaveMobSpawns(bool b, string s)
    {
        if (b)
        {
            var entries = PluginService.MobTracker.GetEntries();
            PluginService.MobTracker.SaveCsv(s, entries);
        }
    }
}
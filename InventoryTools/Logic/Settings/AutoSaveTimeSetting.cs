using ImGuiNET;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class AutoSaveTimeSetting : IntegerSetting
    {
        private readonly PluginLogic _pluginLogic;

        public AutoSaveTimeSetting(ILogger<AutoSaveTimeSetting> logger, ImGuiService imGuiService, PluginLogic pluginLogic) : base(logger, imGuiService)
        {
            _pluginLogic = pluginLogic;
        }
        public override int DefaultValue { get; set; } = 10;
        public override int CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.AutoSaveMinutes;
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, int newValue)
        {
            configuration.AutoSaveMinutes = newValue;
            _pluginLogic.ClearAutoSave();
        }

        public override void Draw(InventoryToolsConfiguration configuration)
        {
            base.Draw(configuration);
            ImGui.SetNextItemWidth(LabelSize);
            ImGui.LabelText("##NextAutoSave","Next Autosave: " + (_pluginLogic.NextSaveTime?.ToString() ?? "N/A"));
        }

        public override string Key { get; set; } = "AutoSaveMinutes";
        public override string Name { get; set; } = "Auto save every";
        public override string HelpText { get; set; } = "How many minutes should there be between each auto save?";
        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.AutoSave;
        public override string Version => "1.6.2.5";
    }
}
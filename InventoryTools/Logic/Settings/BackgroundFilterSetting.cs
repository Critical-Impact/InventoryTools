using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class BackgroundFilterSetting : ChoiceSetting<string>
    {
        public override string DefaultValue { get; set; } = "";
        public override string CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.ActiveBackgroundFilter ?? "";
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, string newValue)
        {
            if (newValue == "")
            {
                PluginService.PluginLogic.DisableActiveBackgroundFilter();
            }
            else
            {
                PluginService.PluginLogic.EnableActiveBackgroundFilterByKey(newValue);
            }
        }

        public override string Key { get; set; } = "BackgroundFilter";
        public override string Name { get; set; } = "Active Background Filter";

        public override string HelpText { get; set; } =
            "This is the filter that is active when the Inventory Tools window is not visible. This filter can be toggled with the associated slash commands.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.FilterSettings;

        public override Dictionary<string, string> Choices
        {
            get
            {
                var filterItems = new Dictionary<string, string> {{"", "None"}};
                foreach (var config in PluginService.PluginLogic.FilterConfigurations)
                {
                    filterItems.Add(config.Key, config.Name);
                }
                return filterItems;
            }
        }
    }
}
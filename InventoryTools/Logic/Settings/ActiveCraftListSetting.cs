﻿using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings
{
    public class ActiveCraftListSetting : ChoiceSetting<string>
    {
        public override string DefaultValue { get; set; } = "";
        public override string CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.ActiveCraftList ?? "";
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, string newValue)
        {
            if (newValue == "")
            {
                PluginService.FilterService.ClearActiveCraftList();
            }
            else
            {
                PluginService.FilterService.SetActiveCraftListByKey(newValue);
            }
        }

        public override string Key { get; set; } = "ActiveCraftList";
        public override string Name { get; set; } = "Active Craft List";

        public override string HelpText { get; set; } =
            "This is the craft list that crafts will count towards.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.FilterSettings;

        public override Dictionary<string, string> Choices
        {
            get
            {
                var filterItems = new Dictionary<string, string> {{"", "None"}};
                foreach (var config in PluginService.FilterService.FiltersList.Where(c => c.FilterType == FilterType.CraftFilter && !c.CraftListDefault))
                {
                    filterItems.Add(config.Key, config.Name);
                }
                return filterItems;
            }
        }
    }
}
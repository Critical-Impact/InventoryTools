using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings
{
    public class BackgroundFilterSetting : ChoiceSetting<string>
    {
        private readonly IListService _listService;

        public BackgroundFilterSetting(ILogger<BackgroundFilterSetting> logger, ImGuiService imGuiService, IListService listService) : base(logger, imGuiService)
        {
            _listService = listService;
        }
        public override string DefaultValue { get; set; } = "";
        public override string CurrentValue(InventoryToolsConfiguration configuration)
        {
            return configuration.ActiveBackgroundFilter ?? "";
        }

        public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, string newValue)
        {
            if (newValue == "")
            {
                _listService.ClearActiveBackgroundList();
            }
            else
            {
                _listService.SetActiveBackgroundListByKey(newValue);
            }
        }

        public override string Key { get; set; } = "BackgroundFilter";
        public override string Name { get; set; } = "Active Background Filter";

        public override string HelpText { get; set; } =
            "This is the filter that is active when the allagan tools window is not visible. This filter can be toggled with the associated slash commands.";

        public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
        public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.FilterSettings;

        public override Dictionary<string, string> Choices
        {
            get
            {
                var filterItems = new Dictionary<string, string> {{"", "None"}};
                foreach (var config in _listService.Lists.Where(c => !c.CraftListDefault))
                {
                    filterItems.Add(config.Key, config.Name);
                }
                return filterItems;
            }
        }
        public override string Version => "1.6.2.5";
    }
}
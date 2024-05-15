using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class AddTitleMenuButtonSetting : BooleanSetting
{
    public AddTitleMenuButtonSetting(ILogger<AddTitleMenuButtonSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }

    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.AddTitleMenuButton;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.AddTitleMenuButton = newValue;
    }

    public override string Key { get; set; } = "AddTitleMenuButton";
    public override string Name { get; set; } = "Add Title Menu Button?";

    public override string HelpText { get; set; } =
        "Adds a button to the title menu along side the dalamud menu items allowing you to open Allagan Tools while not logged in.";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.TitleMenuButtons;
    public override SettingSubCategory SettingSubCategory => SettingSubCategory.General;
    public override string Version => "1.7.0.0"; 
}
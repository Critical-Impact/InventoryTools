﻿using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Logic.Settings;

public class EnableTetrisSetting : BooleanSetting
{
    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TetrisEnabled;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TetrisEnabled = newValue;
    }

    public override string Key { get; set; } = "TetrisEnabled";
    public override string Name { get; set; } = "Enable Tetris?";

    public override string HelpText { get; set; } =
        "Should tetris be enabled? If enabled a new 'Tetris' option will show up in the hamburger menus in the plugin.";

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.General;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.Fun;
}
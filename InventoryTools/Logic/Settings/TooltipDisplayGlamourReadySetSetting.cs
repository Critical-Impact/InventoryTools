using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipDisplayGlamourReadySetSetting : BooleanSetting
{
    public override bool DefaultValue { get; set; } = false;

    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipDisplayGlamourReadySet;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TooltipDisplayGlamourReadySet = newValue;
    }

    public override string Key { get; set; } = "TooltipDisplayGlamourReadySet";
    public override string Name { get; set; } = "Outfit Glamour Info";

    public override string WizardName { get; } = "Outfit Glamour Info";

    public override string HelpText { get; set; } =
        "When hovering an item, show whether it is a Outfit Glamour Set or part of one, along with which component items you already have in your Armory Chest or Armoire.";
    
    public override string Version => "1.12.0.0";

    public TooltipDisplayGlamourReadySetSetting(ILogger<TooltipDisplayGlamourReadySetSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}

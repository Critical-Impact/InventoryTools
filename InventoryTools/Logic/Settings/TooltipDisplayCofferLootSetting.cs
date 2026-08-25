using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipDisplayCofferLootSetting : BooleanSetting
{
    public override bool DefaultValue { get; set; } = false;

    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipDisplayCofferLoot;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.TooltipDisplayCofferLoot = newValue;
    }

    public override string Key { get; set; } = "TooltipDisplayCofferLoot";
    public override string Name { get; set; } = "Coffer Loot Info";

    public override string WizardName { get; } = "Coffer Loot Info";

    public override string HelpText { get; set; } =
        "When hovering a coffer, card pack, or loot container, show how many of its possible loot items you already own.";
    
    public override string Version => "1.12.0.0";

    public TooltipDisplayCofferLootSetting(ILogger<TooltipDisplayCofferLootSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}

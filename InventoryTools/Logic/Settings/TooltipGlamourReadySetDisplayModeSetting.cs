using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class TooltipGlamourReadySetDisplayModeSetting : ChoiceSetting<GlamourReadySetDisplayMode>
{
    public override GlamourReadySetDisplayMode DefaultValue { get; set; } = GlamourReadySetDisplayMode.Compact;

    public override GlamourReadySetDisplayMode CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.TooltipGlamourReadySetDisplayMode;
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, GlamourReadySetDisplayMode newValue)
    {
        configuration.TooltipGlamourReadySetDisplayMode = newValue;
    }

    public override string Key { get; set; } = "TooltipGlamourReadySetDisplayMode";
    public override string Name { get; set; } = "Display Mode";

    public override string WizardName { get; } = "Display Mode";

    public override string HelpText { get; set; } =
        "How the Glamour Ready Set information should be displayed in the tooltip. Detailed lists each piece with its ownership status; Compact shows a summary count.";
    
    public override Dictionary<GlamourReadySetDisplayMode, string> Choices => new()
    {
        { GlamourReadySetDisplayMode.Detailed, "Detailed (list all pieces)" },
        { GlamourReadySetDisplayMode.Compact, "Compact (count summary)" },
    };

    public override string Version => "1.12.0.0";

    public TooltipGlamourReadySetDisplayModeSetting(ILogger<TooltipGlamourReadySetDisplayModeSetting> logger, ImGuiService imGuiService) : base(logger, imGuiService)
    {
    }
}

using CriticalCommonLib.Services.Mediator;
using Dalamud.Plugin.Services;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public abstract class UintWindow : Window
{
    
    public UintWindow(ILogger logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "") : base(logger, mediator, imGuiService, configuration, name)
    {
    }

    public abstract void Initialize(uint windowId);
}

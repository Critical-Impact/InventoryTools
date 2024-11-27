using CriticalCommonLib.Services.Mediator;

using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public abstract class GenericWindow : Window
{
    public GenericWindow(ILogger logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "") : base(logger, mediator, imGuiService,configuration, name)
    {
    }
    public abstract void Initialize();
}
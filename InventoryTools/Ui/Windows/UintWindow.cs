using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public abstract class UintWindow : Window
{
    public uint WindowId { get; set; }
    public UintWindow(ILogger<UintWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "") : base(logger, mediator, imGuiService, configuration, name)
    {
    }

    public virtual void Initialize(uint windowId)
    {
        WindowId = windowId;
    }
}

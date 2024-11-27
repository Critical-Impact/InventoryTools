using CriticalCommonLib.Services.Mediator;

using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public abstract class StringWindow : Window
{
    public string WindowId { get; set; }
    public StringWindow(ILogger logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "") : base(logger, mediator, imGuiService, configuration, name)
    {
    }

    public virtual void Initialize(string windowId)
    {
        WindowId = windowId;
    }
}
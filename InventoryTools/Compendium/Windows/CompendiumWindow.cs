using DalaMock.Host.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Compendium;

public abstract class CompendiumWindow : Window
{
    public CompendiumWindow(ILogger<CompendiumWindow> logger, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, string name = "") : base(logger, mediator, imGuiService,configuration, name)
    {
    }
    public abstract void Initialize();
}
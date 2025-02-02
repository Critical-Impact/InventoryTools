using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;

using InventoryTools.Logic;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages;

public abstract class Page : IConfigPage
{
    private readonly ImGuiService _imGuiService;
    public abstract void Initialize();
    public abstract string Name { get; }
    public abstract List<MessageBase>? Draw();
    public abstract bool IsMenuItem { get; }
    public abstract bool DrawBorder { get; }

    public IEnumerable<Page>? ChildPages { get; set; }

    public ImGuiService ImGuiService => _imGuiService;

    public Page(ILogger logger, ImGuiService imGuiService)
    {
        _imGuiService = imGuiService;
    }
}
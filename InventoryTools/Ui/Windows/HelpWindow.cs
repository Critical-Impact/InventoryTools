using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Ui.Config;
using InventoryTools.Ui.Config.Layouts;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui;

public class HelpWindow : GenericWindow
{
    private readonly InventoryToolsConfiguration _configuration;
    private readonly ConfigNavigationState _navigationState;
    private readonly IReadOnlyDictionary<System.Type, ISetting> _settings;
    private readonly List<PageLayout> _pages;

    public HelpWindow(ILogger<HelpWindow> logger,
        MediatorService mediator,
        ImGuiService imGuiService,
        InventoryToolsConfiguration configuration,
        IEnumerable<IContentLayout> contentLayouts,
        IEnumerable<ISetting> settings,
        ConfigNavigationState navigationState,
        string name = "Help Window") : base(logger, mediator, imGuiService, configuration, name)
    {
        _configuration = configuration;
        _navigationState = navigationState;

        var byType = new Dictionary<System.Type, ISetting>();
        foreach (var setting in settings) byType[setting.GetType()] = setting;
        _settings = byType;

        _pages = contentLayouts.Select(c => c.Build())
            .Where(c => c.Key.StartsWith("help/"))
            .OrderBy(c => HelpPageOrder.IndexOf(c.Key))
            .ToList();
    }

    private static readonly List<string> HelpPageOrder =
    [
        "help/general", "help/list-basics", "help/filtering", "help/about",
    ];

    public override void Initialize()
    {
        WindowName = "Help";
        Key = "help";
    }

    public override bool SaveState => false;
    public override Vector2? DefaultSize { get; } = new Vector2(700, 700);
    public override Vector2? MaxSize { get; } = new Vector2(2000, 2000);
    public override Vector2? MinSize { get; } = new Vector2(200, 200);
    public override string GenericKey { get; } = "help";
    public override string GenericName { get; } = "Help";
    public override bool DestroyOnClose => true;

    private PageLayout? SelectedPage()
    {
        return _pages.FirstOrDefault(c => c.Key == _configuration.SelectedHelpPageKey) ?? _pages.FirstOrDefault();
    }

    public override void DrawWindow()
    {
        using (var sideBarChild =
               ImRaii.Child("SideBar", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
        {
            if (sideBarChild.Success)
            {
                var selected = SelectedPage();
                for (var index = 0; index < _pages.Count; index++)
                {
                    var page = _pages[index];
                    if (ImGui.Selectable($"{index + 1}. {page.Name}", page == selected))
                        _configuration.SelectedHelpPageKey = page.Key;
                }
            }
        }

        ImGui.SameLine();

        using (var mainChild = ImRaii.Child("###ivHelpView", new Vector2(-1, -1), true))
        {
            if (mainChild.Success)
            {
                var page = SelectedPage();
                page?.Draw(new ConfigDrawContext(_configuration, _settings, _navigationState, ImGuiService));
            }
        }
    }

    public override FilterConfiguration? SelectedConfiguration => null;

    public override void Invalidate()
    {
    }
}

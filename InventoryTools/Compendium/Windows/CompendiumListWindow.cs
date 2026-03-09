using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading;
using AllaganLib.Interface.Grid;
using AllaganLib.Shared.Extensions;
using Autofac;
using Autofac.Features.OwnedInstances;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace InventoryTools.Compendium.Windows;

public class CompendiumListWindow : CompendiumWindow
{
    private readonly WindowState _windowState;
    private readonly ICompendiumType _compendiumType;
    private readonly IPluginLog _pluginLog;
    private readonly IEnumerable<IMenuWindow> _menuWindows;
    private readonly IEnumerable<ICompendiumType> _compendiumTypes;
    private readonly Lazy<ICompendiumTable<WindowState, MessageBase>> _table;
    private ICompendiumGrouping? _compendiumGrouping = null;
    private List<KeyValuePair<object, string>>? _compendiumTabs = null;
    private object? _currentTab = null;
    private bool _defaultGroupSet = false;

    public delegate Owned<CompendiumListWindow> Factory(ICompendiumType compendiumType);

    public CompendiumListWindow(ILogger<CompendiumListWindow> logger, WindowState windowState, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ICompendiumType compendiumType, IComponentContext context, IPluginLog pluginLog, IEnumerable<IMenuWindow> menuWindows, IEnumerable<ICompendiumType> compendiumTypes) : base(logger, mediator, imGuiService, configuration, compendiumType.Plural + " Window")
    {
        _windowState = windowState;
        _compendiumType = compendiumType;
        _pluginLog = pluginLog;
        _menuWindows = menuWindows;
        _compendiumTypes = compendiumTypes;
        _table = new Lazy<ICompendiumTable<WindowState, MessageBase>>(
            compendiumType.BuildTable,
            LazyThreadSafetyMode.PublicationOnly);
        Flags = ImGuiWindowFlags.MenuBar;
    }

    private List<KeyValuePair<object, string>>? GetTabs(ICompendiumGrouping compendiumGrouping)
    {
        return _compendiumType.GetGroups(compendiumGrouping)?.OrderBy(k => k.Value).ToList();
    }

    public override void DrawWindow()
    {
        DrawMenuBar();

        if (!_defaultGroupSet)
        {
            _defaultGroupSet = true;
            var defaultGrouping = _compendiumType.GetDefaultGrouping();
            if (defaultGrouping != string.Empty)
            {
                var groupings = _compendiumType.GetGroupings();
                if (groupings != null)
                {
                    _compendiumGrouping = groupings.FirstOrDefault(c => c.Key == defaultGrouping);
                }
            }
        }

        if (_compendiumGrouping == null)
        {
            MediatorService.Publish(_table.Value.Draw(_windowState, new Vector2(0, 0)));
        }
        else
        {
            using (var tabBar = ImRaii.TabBar(_compendiumGrouping.Key, ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.ListPopupButton))
            {
                if (tabBar)
                {
                    _compendiumTabs ??= GetTabs(_compendiumGrouping);
                    if (_compendiumTabs == null)
                    {
                        MediatorService.Publish(_table.Value.Draw(_windowState, new Vector2(0, 0)));
                    }
                    else
                    {
                        foreach (var tab in _compendiumTabs)
                        {
                            using (var tabItem = ImRaii.TabItem(tab.Value, ImGuiTabItemFlags.NoReorder))
                            {
                                if (tabItem)
                                {
                                    if (_currentTab == null || _currentTab != tab.Key)
                                    {
                                        _currentTab = tab.Key;
                                        _table.Value.SetGrouping(_compendiumGrouping, tab.Key);
                                    }
                                    MediatorService.Publish(_table.Value.Draw(_windowState, new Vector2(0, 0)));

                                }
                            }
                        }
                    }

                }
            }
        }
    }

    private void DrawMenuBar()
        {
            using (var menuBar = ImRaii.MenuBar())
            {
                if (menuBar)
                {
                    using (var menu = ImRaii.Menu("File"))
                    {
                        if (menu)
                        {
                            if (ImGui.MenuItem("Configuration"))
                            {
                                this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWindow)));
                            }

                            if (ImGui.MenuItem("Changelog"))
                            {
                                this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(ChangelogWindow)));
                            }

                            if (ImGui.MenuItem("Help"))
                            {
                                this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
                            }

                            if (ImGui.MenuItem("Enable Verbose Logging", "",
                                    this._pluginLog.MinimumLogLevel == LogEventLevel.Verbose))
                            {
                                if (this._pluginLog.MinimumLogLevel == LogEventLevel.Verbose)
                                {
                                    this._pluginLog.MinimumLogLevel = LogEventLevel.Debug;
                                }
                                else
                                {
                                    this._pluginLog.MinimumLogLevel = LogEventLevel.Verbose;
                                }
                            }

                            if (ImGui.MenuItem("Report a Issue"))
                            {
                                "https://github.com/Critical-Impact/InventoryTools".OpenBrowser();
                            }

                            if (ImGui.MenuItem("Ko-Fi"))
                            {
                                "https://ko-fi.com/critical_impact".OpenBrowser();
                            }

                            if (ImGui.MenuItem("Close"))
                            {
                                this.IsOpen = false;
                            }
                        }
                    }


                    var groupings = _compendiumType.GetGroupings();
                    if (groupings != null)
                    {
                        using (var groupingMenu = ImRaii.Menu("Group By"))
                        {
                            if (groupingMenu)
                            {
                                if (ImGui.MenuItem("None", _compendiumGrouping == null))
                                {
                                    _compendiumGrouping = null;
                                    _table.Value.ClearGrouping();
                                }
                                foreach (var grouping in groupings)
                                {
                                    if (ImGui.MenuItem(grouping.Name, _compendiumGrouping?.Equals(grouping) ?? false))
                                    {
                                        _compendiumGrouping = grouping;
                                    }
                                }
                            }
                        }
                    }

                    using (var menu = ImRaii.Menu("Windows"))
                    {
                        if (menu)
                        {
                            foreach (var window in _menuWindows)
                            {
                                if (ImGui.MenuItem(window.GenericName))
                                {
                                    this.MediatorService.Publish(new OpenGenericWindowMessage(window.GetType()));
                                }
                            }
                        }
                    }

                    using (var menu = ImRaii.Menu("Compendium"))
                    {
                        if (menu)
                        {
                            foreach (var compendiumType in _compendiumTypes)
                            {
                                if (ImGui.MenuItem(compendiumType.Plural))
                                {
                                    this.MediatorService.Publish(new ToggleCompendiumListMessage(compendiumType));
                                }
                            }
                        }
                    }

                    if (ImGui.IsItemHovered())
                    {
                        using (ImRaii.Tooltip())
                        {
                            ImGui.Text("Compendium is a WIP feature, expect more here soon!");
                        }
                    }

                }
            }
        }

    public override void Invalidate()
    {

    }

    public override FilterConfiguration? SelectedConfiguration => null;
    public override string GenericKey => CompendiumType.Plural.ToLower();
    public override string GenericName => CompendiumType.Plural + " Window";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize => new Vector2(800, 500);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;

    public ICompendiumType CompendiumType => _compendiumType;

    public override void Initialize()
    {
    }
}
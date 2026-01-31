using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using AllaganLib.Interface.Grid;
using AllaganLib.Shared.Extensions;
using Autofac;
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
    private readonly Lazy<IRenderTable<WindowState, MessageBase>> _table;

    public delegate CompendiumListWindow Factory(ICompendiumType compendiumType);

    public CompendiumListWindow(ILogger<CompendiumListWindow> logger, WindowState windowState, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ICompendiumType compendiumType, IComponentContext context, IPluginLog pluginLog, IEnumerable<IMenuWindow> menuWindows, IEnumerable<ICompendiumType> compendiumTypes) : base(logger, mediator, imGuiService, configuration, compendiumType.Plural + " Window")
    {
        _windowState = windowState;
        _compendiumType = compendiumType;
        _pluginLog = pluginLog;
        _menuWindows = menuWindows;
        _compendiumTypes = compendiumTypes;
        _table = new Lazy<IRenderTable<WindowState, MessageBase>>(
            compendiumType.BuildTable,
            LazyThreadSafetyMode.PublicationOnly);
        Flags = ImGuiWindowFlags.MenuBar;
    }

    public override void Draw()
    {
        DrawMenuBar();

        MediatorService.Publish(_table.Value.Draw(_windowState, new Vector2(0,0)));
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

                    if (ImGui.MenuItem("Export"))
                    {
                        if (SelectedConfiguration != null)
                        {

                        }
                    }

                    using (var menu = ImRaii.Menu("Windows"))
                    {
                        if (menu)
                        {
                            if (_menuWindows != null)
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
                    }

                    using (var menu = ImRaii.Menu("Compendium"))
                    {
                        if (menu)
                        {
                            foreach (var compendiumType in _compendiumTypes)
                            {
                                if (ImGui.MenuItem(compendiumType.Plural))
                                {
                                    this.MediatorService.Publish(new OpenCompendiumListMessage(compendiumType));
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
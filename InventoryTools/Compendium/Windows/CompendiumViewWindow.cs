using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using AllaganLib.Interface.Grid;
using AllaganLib.Shared.Extensions;
using Autofac;
using Autofac.Features.OwnedInstances;
using CriticalCommonLib;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Services;
using InventoryTools.Logic;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace InventoryTools.Compendium.Windows;

public class CompendiumViewWindow : CompendiumWindow
{
    private readonly uint _entityId;
    private readonly CompendiumSectionStateService _sectionStateService;
    private readonly ICompendiumType _compendiumType;
    private readonly IPluginLog _pluginLog;
    private readonly IEnumerable<IMenuWindow> _menuWindows;
    private readonly IEnumerable<ICompendiumType> _compendiumTypes;
    private SectionState? _sectionState;
    private Task<SectionState>? _sectionStateTask;
    private CompendiumViewBuilder? _viewBuilder;
    private bool _viewBuilderRequested;

    public delegate Owned<CompendiumViewWindow> Factory(ICompendiumType compendiumType, uint entityId);

    public CompendiumViewWindow(uint entityId, ILogger<CompendiumViewWindow> logger, CompendiumSectionStateService sectionStateService, MediatorService mediator, ImGuiService imGuiService, InventoryToolsConfiguration configuration, ICompendiumType compendiumType, IPluginLog pluginLog, IEnumerable<IMenuWindow> menuWindows, IEnumerable<ICompendiumType> compendiumTypes) : base(logger, mediator, imGuiService, configuration, compendiumType.Singular + " - " + (compendiumType.GetName(entityId) ?? "Unknown Entity") + "##" + entityId)
    {
        _entityId = entityId;
        _sectionStateService = sectionStateService;
        _compendiumType = compendiumType;
        _pluginLog = pluginLog;
        _menuWindows = menuWindows;
        _compendiumTypes = compendiumTypes.Where(c => c.ShowInListing).OrderBy(c => c.Plural);
        Flags = ImGuiWindowFlags.MenuBar;
    }

    public override void DrawWindow()
    {
        if (_sectionState == null)
        {
            if (_sectionStateTask == null)
            {
                _sectionStateTask = _sectionStateService.GetState(_compendiumType, CompendiumSectionType.View);
            }

            if (_sectionStateTask.IsCompleted)
            {
                _sectionState = _sectionStateTask.Result;
                _sectionStateTask = null;
            }

            if (_sectionState == null)
            {
                return;
            }
        }
        DrawMenuBar(_sectionState);
        DrawView(_sectionState);
        DrawDebug();
    }

    public void DrawDebug()
    {
#if DEBUG
        if (ImGui.CollapsingHeader("Debug"))
        {
            ImGui.TextUnformatted("Entity ID: " + _entityId);
            var relatedObject = _compendiumType.GetObject(_entityId);
            if (relatedObject != null)
            {
                Utils.PrintOutObject(relatedObject, 0, new List<string>());
            }
        }
#endif
    }

    private void DrawView(SectionState sectionState)
    {
        if (!_viewBuilderRequested && _viewBuilder == null)
        {
            _viewBuilder = _compendiumType.BuildView(_entityId);
            _viewBuilderRequested = true;
        }

        if (_viewBuilder == null)
        {
            ImGui.Text("No item could be found with the ID " +  _entityId);
            return;
        }
        _viewBuilder.Draw(sectionState);
    }

    private void DrawMenuBar(SectionState sectionState)
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

    public uint EntityId => _entityId;

    public override FilterConfiguration? SelectedConfiguration => null;
    public override string GenericKey => CompendiumType.Plural.ToLower();
    public override string GenericName => CompendiumType.Plural + " Window";
    public override bool DestroyOnClose => true;
    public override bool SaveState => false;
    public override Vector2? DefaultSize => new Vector2(500, 800);
    public override Vector2? MaxSize => null;
    public override Vector2? MinSize => null;

    public ICompendiumType CompendiumType => _compendiumType;

    public override void Initialize()
    {
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _sectionStateTask?.Dispose();
        }
    }
}
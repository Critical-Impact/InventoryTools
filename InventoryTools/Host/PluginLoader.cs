using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using CriticalCommonLib;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Extensions;
using CriticalCommonLib.Interfaces;
using CriticalCommonLib.Ipc;
using CriticalCommonLib.MarketBoard;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Services.Ui;
using CriticalCommonLib.Time;
using DalaMock.Shared.Classes;
using DalaMock.Shared.Interfaces;
using Dalamud.Interface.ImGuiFileDialog;
using InventoryTools.Commands;
using InventoryTools.Hotkeys;
using InventoryTools.IPC;
using InventoryTools.Lists;
using InventoryTools.Logic;
using InventoryTools.Logic.Columns;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;
using InventoryTools.Logic.Features;
using InventoryTools.Logic.Filters;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Misc;
using InventoryTools.Overlays;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Tooltips;
using InventoryTools.Ui;
using InventoryTools.Ui.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OtterGui.Classes;
using OtterGui.Log;

namespace InventoryTools.Host;

using Dalamud.Plugin;

public class PluginLoader : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly Service _service;
    public IHost? Host { get; private set; }

    public PluginLoader(IDalamudPluginInterface pluginInterfaceService, Service service)
    {
        _pluginInterface = pluginInterfaceService;
        _service = service;
    }

    public IHost Build()
    {
        if (!_pluginInterface.ConfigDirectory.Exists)
        {
            _pluginInterface.ConfigDirectory.Create();
        }
        var hostBuilder = new HostBuilder();
        
            PreBuild(hostBuilder);
            var builtHost = hostBuilder
                .Build();
            builtHost.StartAsync();
            Host = builtHost;
            return builtHost;
    }

    /// <summary>
    /// Override this if you want to replace services before building
    /// </summary>
    /// <param name="hostBuilder"></param>
    public virtual void PreBuild(HostBuilder hostBuilder)
    {
        
    }

    public void Dispose()
    {
        Service.Log.Debug("Starting dispose of HostBuilder");
        Host?.StopAsync().GetAwaiter().GetResult();
        Host?.Dispose();
    }
}
using System;
using System.IO;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;

namespace InventoryTools.Boot;

public sealed class BootConfigurationService : IDisposable
{
    private readonly IFramework _framework;
    private readonly IPluginLog _pluginLog;
    private readonly string _configPath;

    public BootConfiguration Configuration { get; }

    public BootConfigurationService(
        IDalamudPluginInterface pluginInterface,
        IFramework framework,
        IPluginLog pluginLog)
    {
        this._framework = framework;
        _pluginLog = pluginLog;
        var pluginDir = pluginInterface.ConfigDirectory;
        _configPath = Path.Combine(pluginDir.FullName, "boot.json");
        Configuration = Load();
        framework.Update += OnFrameworkUpdate;
    }

    private BootConfiguration Load()
    {
        if (!File.Exists(_configPath))
        {
            return new BootConfiguration();
        }

        try
        {
            var json = File.ReadAllText(_configPath);
            return JsonConvert.DeserializeObject<BootConfiguration>(json)
                   ?? new BootConfiguration();
        }
        catch
        {
            return new BootConfiguration();
        }
    }

    private void OnFrameworkUpdate(IFramework _)
    {
        if (!Configuration.IsDirty)
        {
            return;
        }

        SaveInternal();
    }

    private void SaveInternal()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);

        var json = JsonConvert.SerializeObject(Configuration);
        File.WriteAllText(_configPath, json);
        _pluginLog.Verbose("Saving allagan tools boot configuration.");

        Configuration.ClearDirty();
    }

    public void Save()
    {
        SaveInternal();
    }

    public void Dispose()
    {
        _framework.Update -= OnFrameworkUpdate;

        if (Configuration.IsDirty)
            SaveInternal();
    }
}
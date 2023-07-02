using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace InventoryToolsMock;

public class AppSettings
{
    private AppSettings()
    {
    }

    private static string _jsonSource;
    private static AppSettings? _appSettings = null;
    private static bool _dirty = false;
    private string? _gamePath;
    private string? _pluginConfigPath;
    private bool _autoStart = true;

    public static AppSettings Default
    {
        get
        {
            if (_appSettings == null)
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                _jsonSource = $"{Directory.GetCurrentDirectory()}{Path.DirectorySeparatorChar}appsettings.json";
                _appSettings = new AppSettings();
                if (File.Exists(_jsonSource))
                {
                    var config = builder.Build();
                    config.Bind(_appSettings);
                }

                _dirty = false;
            }

            return _appSettings;
        }
    }

    public void Save()
    {
        _dirty = false;
        string json = JsonConvert.SerializeObject(_appSettings);
        File.WriteAllText(_jsonSource, json);
    }

    public string? GamePath
    {
        get => _gamePath;
        set
        {
            if (value != _gamePath)
            {
                _dirty = true;
                _gamePath = value;
            }
        }
    }

    public string? PluginConfigPath
    {
        get => _pluginConfigPath;
        set
        {
            if (value != _pluginConfigPath)
            {
                _dirty = true;
                _pluginConfigPath = value;
            }
        }
    }

    public bool AutoStart
    {
        get => _autoStart;
        set
        {
            if (value != _autoStart)
            {
                _dirty = true;
                _autoStart = value;
            }
        }
    }

    public static bool Dirty => _dirty;
}
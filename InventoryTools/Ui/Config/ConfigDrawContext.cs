using System;
using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using DalaMock.Host.Mediator;
using InventoryTools.Services;

namespace InventoryTools.Ui.Config;

public sealed class ConfigDrawContext
{
    private readonly IReadOnlyDictionary<Type, ISetting> _settings;

    public ConfigDrawContext(InventoryToolsConfiguration configuration,
        IReadOnlyDictionary<Type, ISetting> settings,
        ConfigNavigationState navigation,
        ImGuiService imGuiService,
        IReadOnlySet<Type>? newSettings = null)
    {
        NewSettings = newSettings ?? new HashSet<Type>();
        Configuration = configuration;
        _settings = settings;
        Navigation = navigation;
        ImGuiService = imGuiService;
    }

    public InventoryToolsConfiguration Configuration { get; }

    public ConfigNavigationState Navigation { get; }

    public ImGuiService ImGuiService { get; }

    public IReadOnlySet<Type> NewSettings { get; }

    public Queue<MessageBase> Messages { get; } = new();

    public ISetting? Find(Type settingType)
    {
        return _settings.GetValueOrDefault(settingType);
    }

    public TSetting? Find<TSetting>() where TSetting : class, ISetting
    {
        return Find(typeof(TSetting)) as TSetting;
    }
}
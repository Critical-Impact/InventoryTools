using System;
using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;

using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class WindowIgnoreEscapeSetting<T> : BooleanSetting where T : Window
{
    private readonly MediatorService _mediatorService;
    private string _key;
    private string _window;
    private string _helpText;
    public WindowIgnoreEscapeSetting(ILogger logger,MediatorService mediatorService, ImGuiService imGuiService, T window) : base(logger, imGuiService)
    {
        _mediatorService = mediatorService;
        _key = window.Key + "Escape";
        _window = window.GenericName + " Window";
        _helpText = "Should the escape key be ignored for the " + window.GenericName + " window?";
    }
    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.DoesWindowIgnoreEscape(typeof(T));
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.SetWindowIgnoreEscape(typeof(T),newValue);
        _mediatorService.Publish(new UpdateWindowRespectClose(typeof(T), !newValue));
    }

    private Dictionary<Type, Window>? _instances;

    public override string Key
    {
        get => _key;
        set {  }
    }

    public override string Name
    {
        get => _window;
        set {  }
    }
    public override string HelpText
    {
        get => _helpText;
        set {  }
    }

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.Windows;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.IgnoreEscape;
    public override string Version => "1.7.0.0";
}

 public class CraftWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<CraftsWindow>
 {
     public CraftWindowIgnoreEscapeSetting(ILogger<CraftWindowIgnoreEscapeSetting> logger,MediatorService mediatorService, ImGuiService imGuiService, CraftsWindow windowFactory) : base(logger,mediatorService, imGuiService, windowFactory)
     {
     }
 }
 public class FiltersWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<FiltersWindow>
 {
     public FiltersWindowIgnoreEscapeSetting(ILogger<FiltersWindowIgnoreEscapeSetting> logger,MediatorService mediatorService, ImGuiService imGuiService, FiltersWindow windowFactory) : base(logger,mediatorService, imGuiService, windowFactory)
     {
     }
 }
 public class ItemWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<ItemWindow>
 {
     public ItemWindowIgnoreEscapeSetting(ILogger<ItemWindowIgnoreEscapeSetting> logger,MediatorService mediatorService, ImGuiService imGuiService, ItemWindow windowFactory) : base(logger,mediatorService, imGuiService, windowFactory)
     {
     }
 }
 public class FilterWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<FilterWindow>
 {
     public FilterWindowIgnoreEscapeSetting(ILogger<FilterWindowIgnoreEscapeSetting> logger,MediatorService mediatorService, ImGuiService imGuiService, FilterWindow windowFactory) : base(logger,mediatorService, imGuiService, windowFactory)
     {
     }
 }

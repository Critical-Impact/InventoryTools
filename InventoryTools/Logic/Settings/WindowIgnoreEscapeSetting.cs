using System;
using System.Collections.Generic;
using System.Linq;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using InventoryTools.Ui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Logic.Settings;

public class WindowIgnoreEscapeSetting<T> : BooleanSetting where T : Window
{
    private string _key;
    private string _window;
    private string _helpText;
    public WindowIgnoreEscapeSetting(ILogger logger, ImGuiService imGuiService, T window) : base(logger, imGuiService)
    {
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
        //TODO: fix me, use configurator service maybe?
        //_windowService.UpdateRespectCloseHotkey(typeof(T), !newValue);
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
    public override string Version => "1.6.2.5";
}

//TODO: Fix me
// public class CraftWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<CraftsWindow>
// {
//     public CraftWindowIgnoreEscapeSetting(ILogger<CraftWindowIgnoreEscapeSetting> logger, ImGuiService imGuiService, CraftsWindow windowFactory) : base(logger, imGuiService, windowFactory)
//     {
//     }
// }
// public class FiltersWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<FiltersWindow>
// {
//     public FiltersWindowIgnoreEscapeSetting(ILogger<FiltersWindowIgnoreEscapeSetting> logger, ImGuiService imGuiService, FiltersWindow windowFactory) : base(logger, imGuiService, windowFactory)
//     {
//     }
// }
// public class ItemWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<ItemWindow>
// {
//     public ItemWindowIgnoreEscapeSetting(ILogger<ItemWindowIgnoreEscapeSetting> logger, ImGuiService imGuiService, ItemWindow windowFactory) : base(logger, imGuiService, windowFactory)
//     {
//     }
// }
// public class FilterWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<FilterWindow>
// {
//     public FilterWindowIgnoreEscapeSetting(ILogger<FilterWindowIgnoreEscapeSetting> logger, ImGuiService imGuiService, FilterWindow windowFactory) : base(logger, imGuiService, windowFactory)
//     {
//     }
// }

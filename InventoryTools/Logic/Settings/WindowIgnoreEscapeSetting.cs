using System;
using System.Collections.Generic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui;

namespace InventoryTools.Logic.Settings;

public class WindowIgnoreEscapeSetting<T> : BooleanSetting where T : Window
{
    public override bool DefaultValue { get; set; } = false;
    public override bool CurrentValue(InventoryToolsConfiguration configuration)
    {
        return configuration.DoesWindowIgnoreEscape(typeof(T));
    }

    public override void UpdateFilterConfiguration(InventoryToolsConfiguration configuration, bool newValue)
    {
        configuration.SetWindowIgnoreEscape(typeof(T),newValue);
        PluginService.WindowService.UpdateRespectCloseHotkey(typeof(T), !newValue);
    }

    private Dictionary<Type, Window>? _instances;

    private Window GetFakeInstance(Type t)
    {

        if (_instances == null)
        {
            _instances = new();
        }

        if (_instances.ContainsKey(t))
        {
            return _instances[t];
        }
        var instance = (Window)Activator.CreateInstance(t)!;
        _instances[t] = instance;
        return instance;
    }
    public override string Key
    {
        get => GetFakeInstance(typeof(T)).Key + "Escape";
        set {  }
    }

    public override string Name
    {
        get => $"{GetFakeInstance(typeof(T)).OriginalWindowName} Window";
        set {  }
    }
    public override string HelpText
    {
        get => "Should the escape key be ignored for the " + GetFakeInstance(typeof(T)).OriginalWindowName + " window?";
        set {  }
    }

    public override SettingCategory SettingCategory { get; set; } = SettingCategory.Windows;
    public override SettingSubCategory SettingSubCategory { get; } = SettingSubCategory.IgnoreEscape;
}

public class CraftWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<CraftsWindow> { }
public class FiltersWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<FiltersWindow> { }
public class ItemWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<ItemWindow> { }
public class FilterWindowIgnoreEscapeSetting : WindowIgnoreEscapeSetting<FilterWindow> { }

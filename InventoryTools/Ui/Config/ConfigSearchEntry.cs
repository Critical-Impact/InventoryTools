using System;

namespace InventoryTools.Ui.Config;

public sealed class ConfigSearchEntry
{
    public ConfigSearchEntry(Type settingType, string displayName, string helpText,
        string pageKey, string pageName, string? sectionTitle)
    {
        SettingType = settingType;
        DisplayName = displayName;
        HelpText = helpText;
        PageKey = pageKey;
        PageName = pageName;
        SectionTitle = sectionTitle;
    }

    public Type SettingType { get; }
    public string DisplayName { get; }
    public string HelpText { get; }
    public string PageKey { get; }
    public string PageName { get; }
    public string? SectionTitle { get; }

    public string Breadcrumb => SectionTitle == null ? PageName : PageName + "  >  " + SectionTitle;
}
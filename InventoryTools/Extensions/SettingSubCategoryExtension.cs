using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Extensions
{
    public static class SettingSubCategoryExtensions
    {
        public static string FormattedName(this SettingSubCategory settingSubCategory)
        {
            switch (settingSubCategory)
            {
                case SettingSubCategory.Experimental:
                    return "Experimental";
                case SettingSubCategory.Fun:
                    return "Fun";
                case SettingSubCategory.Highlighting:
                    return "Highlighting";
                case SettingSubCategory.DestinationHighlighting:
                    return "Destination Highlighting";
                case SettingSubCategory.Market:
                    return "Market";
                case SettingSubCategory.General:
                    return "General";
                case SettingSubCategory.Subsetting:
                    return "Settings";
                case SettingSubCategory.Visuals:
                    return "Visuals";
                case SettingSubCategory.WindowLayout:
                    return "Window Layout";
                case SettingSubCategory.AutoSave:
                    return "Auto Save";
                case SettingSubCategory.FilterSettings:
                    return "Filter Settings";
                case SettingSubCategory.ContextMenus:
                    return "Context/Right Click Menu";
                case SettingSubCategory.Hotkeys:
                    return "Hotkeys";
            }
            return settingSubCategory.ToString();
        }
    }
}
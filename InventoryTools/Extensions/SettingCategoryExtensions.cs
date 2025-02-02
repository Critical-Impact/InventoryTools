using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Extensions
{
    public static class SettingCategoryExtensions
    {
        public static string FormattedName(this SettingCategory settingCategory)
        {
            switch (settingCategory)
            {
                case SettingCategory.General:
                    return "General";
                case SettingCategory.Visuals:
                    return "Visuals";
                case SettingCategory.MarketBoard:
                    return "Marketboard";
                case SettingCategory.CraftOverlay:
                    return "Craft Overlay";
                case SettingCategory.ToolTips:
                    return "Tooltips";
                case SettingCategory.Hotkeys:
                    return "Hotkeys";
                case SettingCategory.History:
                    return "History";
                case SettingCategory.Windows:
                    return "Windows";
                case SettingCategory.Lists:
                    return "Lists";
                case SettingCategory.ContextMenu:
                    return "Context Menu";
                case SettingCategory.MobSpawnTracker:
                    return "Mob Spawn Tracker";
                case SettingCategory.TitleMenuButtons:
                    return "Title Menu Button";
                case SettingCategory.AutoSave:
                    return "Auto Save";
                case SettingCategory.Items:
                    return "Items";
                case SettingCategory.Highlighting:
                    return "Highlighting";
            }
            return settingCategory.ToString();
        }
    }
}
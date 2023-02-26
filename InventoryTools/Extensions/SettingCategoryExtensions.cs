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
                case SettingCategory.ToolTips:
                    return "Tooltips";
            }
            return settingCategory.ToString();
        }
    }
}
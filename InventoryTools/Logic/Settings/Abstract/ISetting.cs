using System.Collections.Generic;

namespace InventoryTools.Logic.Settings.Abstract
{
    public interface ISetting
    {
        public int LabelSize { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }
        
        public SettingCategory SettingCategory { get; set; }
        public SettingSubCategory SettingSubCategory { get; }

        public bool HasValueSet(InventoryToolsConfiguration configuration);
        
        public void Draw(InventoryToolsConfiguration configuration);

        public static readonly List<SettingCategory> SettingCategoryOrder = new() {SettingCategory.General, SettingCategory.Visuals, SettingCategory.ToolTips, SettingCategory.MarketBoard};
        
        public static readonly List<SettingSubCategory> SettingSubCategoryOrder = new() {SettingSubCategory.FilterSettings, SettingSubCategory.AutoSave, SettingSubCategory.Experimental, SettingSubCategory.Highlighting, SettingSubCategory.DestinationHighlighting, SettingSubCategory.General,SettingSubCategory.Subsetting,SettingSubCategory.Visuals, SettingSubCategory.WindowLayout, SettingSubCategory.Market, SettingSubCategory.ContextMenus, SettingSubCategory.Hotkeys};

    }
}
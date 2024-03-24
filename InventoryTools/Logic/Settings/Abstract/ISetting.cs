using System.Collections.Generic;
using System.Numerics;

namespace InventoryTools.Logic.Settings.Abstract
{
    public interface ISetting
    {
        public int LabelSize { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }
        
        public string WizardName { get; }
        
        public bool HideReset { get; set; }
        
        public bool ColourModified { get; set; }
        public string? Image { get; }
        public Vector2? ImageSize { get; }
        
        public SettingCategory SettingCategory { get; set; }
        public SettingSubCategory SettingSubCategory { get; }
        
        public string Version { get; }

        public bool HasValueSet(InventoryToolsConfiguration configuration);
        
        public void Draw(InventoryToolsConfiguration configuration);

        public static readonly List<SettingCategory> SettingCategoryOrder = new() {SettingCategory.General, SettingCategory.Visuals, SettingCategory.ToolTips, SettingCategory.MarketBoard, SettingCategory.History};
        
        public static readonly List<SettingSubCategory> SettingSubCategoryOrder = new() {SettingSubCategory.FilterSettings, SettingSubCategory.AutoSave, SettingSubCategory.Experimental, SettingSubCategory.Highlighting, SettingSubCategory.DestinationHighlighting, SettingSubCategory.General,SettingSubCategory.Subsetting,SettingSubCategory.Visuals, SettingSubCategory.WindowLayout, SettingSubCategory.Market, SettingSubCategory.ContextMenus, SettingSubCategory.Hotkeys, SettingSubCategory.Fun, SettingSubCategory.IgnoreEscape};

    }
}
using System.Collections.Generic;
using System.Numerics;

namespace InventoryTools.Logic.Settings.Abstract
{
    public interface ISetting
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }

        public string WizardName { get; }
        public string? Image { get; }
        public Vector2? ImageSize { get; }

        public SettingCategory SettingCategory { get; set; }
        public SettingSubCategory SettingSubCategory { get; }

        public string Version { get; }

        public uint? Order { get; }

        public bool HasValueSet(InventoryToolsConfiguration configuration);

        public void Draw(InventoryToolsConfiguration configuration, string? customName, bool? disableReset,
            bool? disableColouring);

        public static readonly List<SettingSubCategory> SettingSubCategoryOrder =
        [
            SettingSubCategory.General,SettingSubCategory.ActiveLists, SettingSubCategory.FilterSettings,
            SettingSubCategory.AutoSave, SettingSubCategory.Experimental, SettingSubCategory.Highlighting,
            SettingSubCategory.DestinationHighlighting, SettingSubCategory.Subsetting, SettingSubCategory.Visuals,
            SettingSubCategory.WindowLayout, SettingSubCategory.Market, SettingSubCategory.ContextMenus,
            SettingSubCategory.Hotkeys, SettingSubCategory.Fun, SettingSubCategory.IgnoreEscape,
        ];

    }
}
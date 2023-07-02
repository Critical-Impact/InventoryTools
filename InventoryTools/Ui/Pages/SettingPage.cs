using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Sections
{
    public class SettingPage : IConfigPage
    {
      
        public SettingPage(SettingCategory category)
        {
            Category = category;
        }

        public string Name
        {
            get
            {
                return Category.FormattedName();
            }
        }

        public SettingCategory Category { get; set; }
        private Dictionary<SettingSubCategory, List<ISetting>>? _settings;
        private bool _isSeparator = false;

        public Dictionary<SettingSubCategory, List<ISetting>> Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = GenerateSettings();
                }
                return _settings;
            }
        }

        public Dictionary<SettingSubCategory, List<ISetting>> GenerateSettings()
        {
            return PluginService.PluginLogic.AvailableSettings.Where(c => c.SettingCategory == Category).GroupBy(c => c.SettingSubCategory).OrderBy(c => ISetting.SettingSubCategoryOrder.IndexOf(c.Key)).ToDictionary(c => c.Key, c => c.OrderBy(s => s.Name).ToList());
        }


        public void Draw()
        {
            foreach (var groupedSettings in Settings)
            {
                if (ImGui.CollapsingHeader(groupedSettings.Key.FormattedName(), ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    foreach (var setting in groupedSettings.Value)
                    {
                        setting.Draw(ConfigurationManager.Config);
                    }
                }
            }
        }

        public bool IsMenuItem => _isSeparator;
    }
}
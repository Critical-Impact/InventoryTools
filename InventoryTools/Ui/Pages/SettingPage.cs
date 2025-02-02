using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;
using OtterGui.Raii;

namespace InventoryTools.Ui.Pages
{
    public class SettingPage : Page
    {
        public SettingSubCategory? SubCategory { get; }
        private readonly Factory _settingPageFactory;
        private readonly IEnumerable<ISetting> _settings;
        private readonly InventoryToolsConfiguration _configuration;

        public delegate SettingPage Factory(SettingCategory settingCategory, SettingSubCategory? subCategory = null);

        public SettingPage(SettingPage.Factory settingPageFactory, SettingCategory settingCategory, ILogger<SettingPage> logger, ImGuiService imGuiService, IEnumerable<ISetting> settings, InventoryToolsConfiguration configuration, SettingSubCategory? subCategory = null) : base(logger, imGuiService)
        {
            _settingPageFactory = settingPageFactory;
            SubCategory = subCategory;
            _settings = settings;
            _configuration = configuration;
            this.Initialize(settingCategory);
        }

        public SettingPage(SettingPage.Factory settingPageFactory, ILogger<SettingPage> logger, ImGuiService imGuiService, IEnumerable<ISetting> settings, InventoryToolsConfiguration configuration) : base(logger, imGuiService)
        {
            _settingPageFactory = settingPageFactory;
            _settings = settings;
            _configuration = configuration;
        }

        public void Initialize(SettingCategory settingCategory)
        {
            Category = settingCategory;
            Settings = _settings.Where(c => c.SettingCategory == Category && c.SettingCategory != SettingCategory.None)
                .Where(c => SubCategory == null || c.SettingSubCategory == SubCategory)
                .GroupBy(c => c.SettingSubCategory).OrderBy(c => ISetting.SettingSubCategoryOrder.IndexOf(c.Key))
                .ToDictionary(c => c.Key, c => c.OrderBy(s => s.Name).ToList());
            if (SubCategory == null)
            {
                if (this.Settings.Select(c => c.Key).Distinct().Count() > 1)
                {
                    this.ChildPages = Settings.Select(c => c.Key).Distinct().OrderBy(c =>
                        {
                            var indexOf = ISetting.SettingSubCategoryOrder.IndexOf(c);
                            return indexOf == -1 ? 9999 : indexOf;
                        })
                        .Select(c => _settingPageFactory.Invoke(settingCategory, c)).ToList();
                }
            }
        }

        public override void Initialize()
        {

        }

        public override string Name
        {
            get
            {
                return SubCategory?.FormattedName() ?? Category.FormattedName();
            }
        }

        public SettingCategory Category { get; set; }
        private bool _isSeparator = false;

        public Dictionary<SettingSubCategory, List<ISetting>> Settings;

        public override List<MessageBase>? Draw()
        {
            foreach (var groupedSettings in Settings)
            {
                foreach(var setting in groupedSettings.Value.OrderBy(c => c.Order ?? 999).ToList())
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                    setting.Draw(_configuration, null, null, null);
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                }
            }

            return null;
        }

        public override bool IsMenuItem => _isSeparator;
        public override bool DrawBorder => true;
    }
}
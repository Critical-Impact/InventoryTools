using System.Collections.Generic;
using System.Linq;
using CriticalCommonLib.Services.Mediator;

using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Services;
using Microsoft.Extensions.Logging;

namespace InventoryTools.Ui.Pages
{
    public class SettingPage : Page
    {
        private readonly IEnumerable<ISetting> _settings;
        private readonly InventoryToolsConfiguration _configuration;

        public SettingPage(ILogger<SettingPage> logger, ImGuiService imGuiService, IEnumerable<ISetting> settings, InventoryToolsConfiguration configuration) : base(logger, imGuiService)
        {
            _settings = settings;
            _configuration = configuration;
        }

        public void Initialize(SettingCategory settingCategory)
        {
            Category = settingCategory;
            Settings = _settings.Where(c => c.SettingCategory == Category && c.SettingCategory != SettingCategory.None)
                .GroupBy(c => c.SettingSubCategory).OrderBy(c => ISetting.SettingSubCategoryOrder.IndexOf(c.Key))
                .ToDictionary(c => c.Key, c => c.OrderBy(s => s.Name).ToList());
        }

        public override void Initialize()
        {

        }

        public override string Name
        {
            get
            {
                return Category.FormattedName();
            }
        }

        public SettingCategory Category { get; set; }
        private bool _isSeparator = false;

        public Dictionary<SettingSubCategory, List<ISetting>> Settings;

        public override List<MessageBase>? Draw()
        {
            foreach (var groupedSettings in Settings)
            {
                if (ImGui.CollapsingHeader(groupedSettings.Key.FormattedName(), ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
                {
                    for (var index = 0; index < groupedSettings.Value.Count; index++)
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                        var setting = groupedSettings.Value[index];
                        setting.Draw(_configuration);
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 5);
                    }
                }
            }

            return null;
        }

        public override bool IsMenuItem => _isSeparator;
        public override bool DrawBorder => true;
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Sections;

namespace InventoryTools.Ui
{
    public class ConfigurationWindow : Window
    {
        public ConfigurationWindow(string name = "Allagan Tools - Configuration") : base(name)
        {
            SetupWindow();
        }
        
        public ConfigurationWindow() : base("Allagan Tools - Configuration")
        {
            SetupWindow();
        }
        
        private void SetupWindow()
        {
            _configPages = new List<IConfigPage>();
            _configPages.Add(new SettingPage(SettingCategory.General));
            _configPages.Add(new SettingPage(SettingCategory.Visuals));
            _configPages.Add(new SettingPage(SettingCategory.MarketBoard));
            _configPages.Add(new FiltersPage());
            _configPages.Add(new CraftFiltersPage());
            _configPages.Add(new ImportExportPage());
            _configPages.Add(new CharacterRetainerPage());
            GenerateFilterPages();
        }

        
        private int ConfigSelectedConfigurationPage
        {
            get => ConfigurationManager.Config.SelectedConfigurationPage;
            set => ConfigurationManager.Config.SelectedConfigurationPage = value;
        }

        public void GenerateFilterPages()
        {
            
            var filterConfigurations = PluginService.FilterService.FiltersList.Where(c => c.FilterType != FilterType.CraftFilter);
            var filterPages = new Dictionary<string, IConfigPage>(); 
            foreach (var filter in filterConfigurations)
            {
                if (!filterPages.ContainsKey(filter.Key))
                {
                    filterPages.Add(filter.Key, new FilterPage(filter));
                }
            }

            _filterPages = filterPages;
        }
        
        public override bool SaveState => true;
        public static string AsKey => "configuration";
        public override string Key => AsKey;
        public override Vector2 DefaultSize { get; } = new(700, 700);
        public override Vector2 MaxSize { get; } = new(2000, 2000);
        public override Vector2 MinSize { get; } = new(200, 200);
        public override bool DestroyOnClose => true;
        private List<IConfigPage> _configPages;
        public Dictionary<string, IConfigPage> _filterPages = new Dictionary<string,IConfigPage>();

        public override void Draw()
        {
            ImGui.BeginChild("###ivConfigList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true,
                ImGuiWindowFlags.NoSavedSettings);
            
            for (var index = 0; index < _configPages.Count; index++)
            {
                var configPage = _configPages[index];
                if (ImGui.Selectable(configPage.Name, ConfigSelectedConfigurationPage == index))
                {
                    ConfigSelectedConfigurationPage = index;
                }
            }

            ImGui.NewLine();
            ImGui.Text("Filters");
            ImGui.Separator();

            var filterIndex = _configPages.Count;
            foreach (var item in _filterPages)
            {
                filterIndex++;
                if (ImGui.Selectable(item.Value.Name + "##" + item.Key, ConfigSelectedConfigurationPage == filterIndex))
                {
                    ConfigSelectedConfigurationPage = filterIndex;
                }

            }
            
            ImGui.EndChild();
            

            ImGui.SameLine();

            ImGui.BeginChild("###ivConfigView", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar);
            
            for (var index = 0; index < _configPages.Count; index++)
            {
                if (ConfigSelectedConfigurationPage == index)
                {
                    _configPages[index].Draw();
                }
            }

            var filterIndex2 = _configPages.Count;
            foreach(var filter in _filterPages)
            {
                filterIndex2++;
                if (ConfigSelectedConfigurationPage == filterIndex2)
                {
                    filter.Value.Draw();
                }
            }

            ImGui.EndChild();
            
        }

        public override void Invalidate()
        {
            GenerateFilterPages();
        }

        public override FilterConfiguration? SelectedConfiguration => null;
    }
}
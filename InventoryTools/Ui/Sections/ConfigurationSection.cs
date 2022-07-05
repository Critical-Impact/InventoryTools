using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;

namespace InventoryTools.Sections
{
    public static class ConfigurationSection
    {
        private static int ConfigSelectedConfigurationPage
        {
            get => Configuration.SelectedConfigurationPage;
            set => Configuration.SelectedConfigurationPage = value;
        }

        private static InventoryToolsConfiguration Configuration => ConfigurationManager.Config;

        private static List<IConfigPage>? _configPages;
        public static List<IConfigPage> ConfigPages
        {
            get
            {
                if (_configPages == null)
                {
                    _configPages = new List<IConfigPage>();
                    _configPages.Add(new SettingPage(SettingCategory.General));
                    _configPages.Add(new SettingPage(SettingCategory.Visuals));
                    _configPages.Add(new SettingPage(SettingCategory.MarketBoard));
                    _configPages.Add(new FiltersPage());
                    _configPages.Add(new ImportExportPage());
                    _configPages.Add(new CharacterRetainerPage());
                }

                return _configPages;
            }
        }

        public static Dictionary<string, IConfigPage> _filterPages = new Dictionary<string,IConfigPage>();
        public static Dictionary<string, IConfigPage> FilterPages
        {
            get
            {
                var currentKeys = _filterPages.Keys.ToHashSet();
                foreach (var filter in PluginService.PluginLogic.FilterConfigurations)
                {
                    if (!_filterPages.ContainsKey(filter.Key))
                    {
                        _filterPages.Add(filter.Key, new FilterPage(filter));
                        if (!currentKeys.Contains(filter.Key))
                        {
                            currentKeys.Add(filter.Key);
                        }
                    }
                }

                foreach (var deletedFilter in currentKeys.Where(c => !PluginService.PluginLogic.FilterConfigurations.Exists(d => d.Key == c)))
                {
                    if (_filterPages.ContainsKey(deletedFilter))
                    {
                        _filterPages.Remove(deletedFilter);
                    }
                }

                return _filterPages;
            }
        }

        public static void Draw()
        {
            if (ImGui.BeginChild("###ivConfigList", new Vector2(150, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                for (var index = 0; index < ConfigPages.Count; index++)
                {
                    var configPage = ConfigPages[index];
                    if (ImGui.Selectable(configPage.Name, ConfigSelectedConfigurationPage == index))
                    {
                        ConfigSelectedConfigurationPage = index;
                    }
                }

                ImGui.NewLine();
                ImGui.Text("Filters");
                ImGui.Separator();

                var filterIndex = ConfigPages.Count;
                foreach (var item in FilterPages)
                {
                    filterIndex++;
                    if (ImGui.Selectable(item.Value.Name, ConfigSelectedConfigurationPage == filterIndex))
                    {
                        ConfigSelectedConfigurationPage = filterIndex;
                    }

                }
                

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###ivConfigView", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                for (var index = 0; index < ConfigPages.Count; index++)
                {
                    if (ConfigSelectedConfigurationPage == index)
                    {
                        ConfigPages[index].Draw();
                    }
                }

                var filterIndex = ConfigPages.Count;
                foreach(var filter in FilterPages)
                {
                    filterIndex++;
                    if (ConfigSelectedConfigurationPage == filterIndex)
                    {
                        filter.Value.Draw();
                    }
                }

                ImGui.EndChild();
            }
        }
    }
}
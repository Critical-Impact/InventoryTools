using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Colors;
using ImGuiNET;
using ImGuiScene;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Sections;
using InventoryTools.Ui.MenuItems;
using InventoryTools.Ui.Widgets;
using OtterGui;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui
{
    public class ConfigurationWindow : Window
    {
        private TextureWrap _addIcon => PluginService.IconStorage.LoadIcon(66315);
        private TextureWrap _lightBulbIcon => PluginService.IconStorage.LoadIcon(66318);
        private PopupMenu _addFilterMenu;
        private PopupMenu _addSampleMenu;

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
            _configPages.Add(new SeparatorPageItem("Settings"));
            _configPages.Add(new SettingPage(SettingCategory.General));
            _configPages.Add(new SettingPage(SettingCategory.Visuals));
            _configPages.Add(new SettingPage(SettingCategory.ToolTips));
            _configPages.Add(new SettingPage(SettingCategory.MarketBoard));
            _configPages.Add(new SeparatorPageItem("Data", true));
            _configPages.Add(new FiltersPage());
            _configPages.Add(new CraftFiltersPage());
            _configPages.Add(new ImportExportPage());
            _configPages.Add(new CharacterRetainerPage());
            
            _addFilterMenu = new PopupMenu("addFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("Search Filter", "adf1", "New Search Filter", AddSearchFilter, "This will create a new filter that let's you search for specific items within your characters and retainers inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Sort Filter", "af2", "New Sort Filter", AddSortFilter, "This will create a new filter that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item Filter", "af3", "New Game Item Filter", AddGameItemFilter, "This will create a filter that lets you search for all items in the game.")
                });
            
            _addSampleMenu = new PopupMenu("addSampleFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("Purchased for less than 100 gil", "af4", "Less than 100 gil", AddLessThan100GilFilter, "This will add a filter that will show all items that can be purchased from gil shops under 100 gil. It will look in both character and retainer inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Put away materials +", "af5", "Put away materials", AddPutAwayMaterialsFilter, "This will add a filter that will be setup to quickly put away any excess materials. It will have all the material categories automatically added. When calculating where to put items it will try to prioritise existing stacks of items."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Duplicated items across characters/retainers +", "af6", "Duplicated items", AddDuplicatedItemsFilter, "This will add a filter that will provide a list of all the distinct stacks that appear in 2 sets of inventories. You can use this to make sure only one retainer has a specific type of item.")
                });
            
            GenerateFilterPages();
        }

        private Dictionary<FilterConfiguration, PopupMenu> _popupMenus = new();
        public PopupMenu GetFilterMenu(FilterConfiguration configuration)
        {
            if (!_popupMenus.ContainsKey(configuration))
            {
                _popupMenus[configuration] = new PopupMenu("fm" + configuration.Key, PopupMenu.PopupMenuButtons.Right,
                    new List<PopupMenu.IPopupMenuItem>()
                    {
                        new PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateFilter, "Duplicate the filter."),
                        new PopupMenu.PopupMenuItemSelectable("Move Up", "mu_" + configuration.Key, MoveFilterUp, "Move the filter up."),
                        new PopupMenu.PopupMenuItemSelectable("Move Down", "md_" + configuration.Key, MoveFilterDown, "Move the filter down."),
                        new PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this filter?", RemoveFilter, "Remove the filter."),
                    }
                );
            }

            return _popupMenus[configuration];
        }

        private void RemoveFilter(string id, bool confirmed)
        {
            if (confirmed)
            {
                id = id.Replace("rf_", "");
                var existingFilter = PluginService.FilterService.GetFilterByKey(id);
                if (existingFilter != null)
                {
                    PluginService.FilterService.RemoveFilter(existingFilter);
                    ConfigSelectedConfigurationPage--;
                }
            }
        }

        private void MoveFilterDown(string id)
        {
            id = id.Replace("md_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                PluginService.FilterService.MoveFilterDown(existingFilter);
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                PluginService.FilterService.MoveFilterUp(existingFilter);
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                PluginService.FilterService.DuplicateFilter(existingFilter, filterName);
                SetNewFilterActive();
            }
        }

        private void AddDuplicatedItemsFilter(string newName, string id)
        {
            PluginService.PluginLogic.AddSampleFilterDuplicatedItems(newName);
            SetNewFilterActive();
        }

        private void AddPutAwayMaterialsFilter(string newName, string id)
        {
            PluginService.PluginLogic.AddSampleFilterMaterials(newName);
            SetNewFilterActive();
        }

        private void AddLessThan100GilFilter(string newName, string id)
        {
            PluginService.PluginLogic.AddSampleFilter100Gil(newName);
            SetNewFilterActive();
        }

        private void AddSearchFilter(string newName, string id)
        {
            PluginService.FilterService.AddFilter(new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.SearchFilter));
            SetNewFilterActive();
        }

        private void AddGameItemFilter(string newName, string id)
        {
            PluginService.FilterService.AddFilter(new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.GameItemFilter));
            SetNewFilterActive();
        }

        private void AddSortFilter(string newName, string id)
        {
            PluginService.FilterService.AddFilter(new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.SortingFilter));
            SetNewFilterActive();
        }


        private int ConfigSelectedConfigurationPage
        {
            get => ConfigurationManager.Config.SelectedConfigurationPage;
            set => ConfigurationManager.Config.SelectedConfigurationPage = value;
        }

        public void SetActiveFilter(FilterConfiguration configuration)
        {
            var filterIndex = _filterPages.ContainsKey(configuration.Key) ? _filterPages.Keys.IndexOf(configuration.Key) + 1 : -1;
            if (filterIndex != -1)
            {
                ConfigSelectedConfigurationPage = _configPages.Count + filterIndex - 2;
            }
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

        private void SetNewFilterActive()
        {
            ConfigSelectedConfigurationPage = _configPages.Count + _filterPages.Count - 2;
        }

        public override void Draw()
        {
            ImGui.BeginChild("SideContainer", new Vector2(180, -1) * ImGui.GetIO().FontGlobalScale, true);
            ImGui.BeginChild("Menu", new Vector2(0, -28) * ImGui.GetIO().FontGlobalScale, false,
                ImGuiWindowFlags.NoSavedSettings);

            var count = 0;
            for (var index = 0; index < _configPages.Count; index++)
            {
                var configPage = _configPages[index];
                if (configPage.IsMenuItem)
                {
                    configPage.Draw();
                }
                else
                {
                    if (ImGui.Selectable(configPage.Name, ConfigSelectedConfigurationPage == count))
                    {
                        ConfigSelectedConfigurationPage = count;
                    }

                    count++;
                }
            }

            ImGui.NewLine();
            ImGui.Text("Filters");
            ImGui.Separator();

            var filterIndex = count;
            foreach (var item in _filterPages)
            {
                filterIndex++;
                if (ImGui.Selectable(item.Value.Name + "##" + item.Key, ConfigSelectedConfigurationPage == filterIndex))
                {
                    ConfigSelectedConfigurationPage = filterIndex;
                }

                var filter = PluginService.FilterService.GetFilterByKey(item.Key);
                if (filter != null)
                {
                    GetFilterMenu(filter).Draw();
                }

            }
            ImGui.EndChild();
            ImGui.BeginChild("Settings", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false);
            
            float height = ImGui.GetWindowSize().Y;
            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
            
            if (ImGui.ImageButton(_addIcon.ImGuiHandle, new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,
                    new Vector2(0, 0), new Vector2(1, 1), 2))
            {
                
            }
            
            _addFilterMenu.Draw();
            ImGuiUtil.HoverTooltip("Add a new filter");
            
            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
            ImGui.SetCursorPosX(26 * ImGui.GetIO().FontGlobalScale);
            
            if (ImGui.ImageButton(_lightBulbIcon.ImGuiHandle, new Vector2(20, 20) * ImGui.GetIO().FontGlobalScale,
                    new Vector2(0, 0), new Vector2(1, 1), 2))
            {
                
            }
            
            _addSampleMenu.Draw();
            ImGuiUtil.HoverTooltip("Add a sample filter");
            
            ImGui.EndChild();
            ImGui.EndChild();
            

            ImGui.SameLine();

            ImGui.BeginChild("###ivConfigView", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar);

            count = 0;
            for (var index = 0; index < _configPages.Count; index++)
            {
                if (_configPages[index].IsMenuItem)
                {
                    count++;
                    continue;
                }

                if (ConfigSelectedConfigurationPage == index - count)
                {
                    _configPages[index].Draw();
                }
            }

            var filterIndex2 = _configPages.Count - count;
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
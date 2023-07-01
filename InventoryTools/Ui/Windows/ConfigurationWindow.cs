using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Sections;
using InventoryTools.Ui.MenuItems;
using InventoryTools.Ui.Widgets;
using OtterGui;
using OtterGui.Raii;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui
{
    public class ConfigurationWindow : Window
    {
        private HoverButton _addIcon { get; } = new(PluginService.IconStorage.LoadIcon(66315),  new Vector2(22, 22));
        private HoverButton _lightBulbIcon { get; } = new(PluginService.IconStorage.LoadIcon(66318),  new Vector2(22, 22));
        private static HoverButton _menuIcon { get; } = new(PluginService.IconStorage.LoadImage("menu"),  new Vector2(22, 22));
        private PopupMenu _addFilterMenu;
        private PopupMenu _addSampleMenu;
        private PopupMenu _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
            new List<PopupMenu.IPopupMenuItem>()
            {
                new PopupMenu.PopupMenuItemSelectable("Filters Window", "filters", OpenFiltersWindow,"Open the filters window."),
                new PopupMenu.PopupMenuItemSelectable("Crafts Window", "crafts", OpenCraftsWindow,"Open the crafts window."),
                new PopupMenu.PopupMenuItemSeparator(),
                new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,"Open the mobs window."),
                new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,"Open the duties window."),
                new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,"Open the airships window."),
                new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenSubmarinesWindow,"Open the submarines window."),
                new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures", OpenRetainerVenturesWindow,"Open the retainer ventures window."),
                new PopupMenu.PopupMenuItemSelectable("Tetris", "tetris", OpenTetrisWindow,"Open the tetris window.", () => ConfigurationManager.Config.TetrisEnabled),
                new PopupMenu.PopupMenuItemSeparator(),
                new PopupMenu.PopupMenuItemSelectable("Help", "help", OpenHelpWindow,"Open the help window."),
            });

        private static void OpenCraftsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<CraftsWindow>(CraftsWindow.AsKey);
        }

        private static void OpenFiltersWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<FiltersWindow>(FiltersWindow.AsKey);
        }

        private static void OpenHelpWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<HelpWindow>(HelpWindow.AsKey);
        }

        private static void OpenDutiesWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<DutiesWindow>(DutiesWindow.AsKey);
        }

        private static void OpenAirshipsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<AirshipsWindow>(AirshipsWindow.AsKey);
        }

        private static void OpenSubmarinesWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<SubmarinesWindow>(SubmarinesWindow.AsKey);
        }
        
        private static void OpenRetainerVenturesWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<RetainerTasksWindow>(RetainerTasksWindow.AsKey);
        }

        private static void OpenMobsWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<BNpcWindow>(BNpcWindow.AsKey);
        }
        
        private static void OpenTetrisWindow(string obj)
        {
            PluginService.WindowService.OpenWindow<TetrisWindow>(TetrisWindow.AsKey);
        }

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
            _configPages.Add(new SettingPage(SettingCategory.Hotkeys));
            _configPages.Add(new SettingPage(SettingCategory.MarketBoard));
            _configPages.Add(new SettingPage(SettingCategory.History));
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
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item Filter", "af3", "New Game Item Filter", AddGameItemFilter, "This will create a filter that lets you search for all items in the game."),
                    new PopupMenu.PopupMenuItemSelectableAskName("History Filter", "af4", "New History Item Filter", AddHistoryFilter, "This will create a filter that lets you view historical data of how your inventory has changed."),
                });
            
            _addSampleMenu = new PopupMenu("addSampleFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("All", "af4", "All", AddAllFilter, "This will add a filter that will be preconfigured to show items across all inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Player", "af5", "Player", AddPlayerFilter, "This will add a filter that will be preconfigured to show items across all character inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Retainers", "af6", "Retainers", AddRetainersFilter, "This will add a filter that will be preconfigured to show items across all retainer inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Free Company", "af7", "Free Company", AddFreeCompanyFilter, "This will add a filter that will be preconfigured to show items across all free company inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("All Game Items", "af8", "All Game Items", AddAllGameItemsFilter, "This will add a filter that will be preconfigured to show all of the game's items."),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectableAskName("Purchased for less than 100 gil", "af9", "Less than 100 gil", AddLessThan100GilFilter, "This will add a filter that will show all items that can be purchased from gil shops under 100 gil. It will look in both character and retainer inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Put away materials +", "af10", "Put away materials", AddPutAwayMaterialsFilter, "This will add a filter that will be setup to quickly put away any excess materials. It will have all the material categories automatically added. When calculating where to put items it will try to prioritise existing stacks of items."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Duplicated items across characters/retainers +", "af11", "Duplicated items", AddDuplicatedItemsFilter, "This will add a filter that will provide a list of all the distinct stacks that appear in 2 sets of inventories. You can use this to make sure only one retainer has a specific type of item.")
                });
            
            GenerateFilterPages();
        }

        private void AddAllGameItemsFilter(string arg1, string arg2)
        {
            PluginService.PluginLogic.AddAllGameItemsFilter(arg1);
            SetNewFilterActive();
        }

        private void AddFreeCompanyFilter(string arg1, string arg2)
        {
            PluginService.PluginLogic.AddFreeCompanyFilter(arg1);
            SetNewFilterActive();
        }

        private void AddRetainersFilter(string arg1, string arg2)
        {
            PluginService.PluginLogic.AddRetainerFilter(arg1);
            SetNewFilterActive();
        }

        private void AddPlayerFilter(string arg1, string arg2)
        {
            PluginService.PluginLogic.AddPlayerFilter(arg1);
            SetNewFilterActive();
        }

        private void AddAllFilter(string arg1, string arg2)
        {
            PluginService.PluginLogic.AddAllFilter(arg1);
            SetNewFilterActive();
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

        private void AddHistoryFilter(string newName, string id)
        {
            PluginService.FilterService.AddFilter(new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.HistoryFilter));
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
            using (var sideBarChild =
                   ImRaii.Child("SideBar", new Vector2(180, 0) * ImGui.GetIO().FontGlobalScale, true))
            {
                if (sideBarChild.Success)
                {
                    using (var menuChild = ImRaii.Child("Menu", new Vector2(0, -28) * ImGui.GetIO().FontGlobalScale,
                               false, ImGuiWindowFlags.NoSavedSettings))
                    {
                        if (menuChild.Success)
                        {

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
                            ImGui.TextUnformatted("Filters");
                            ImGui.Separator();

                            var filterIndex = count;
                            foreach (var item in _filterPages)
                            {
                                filterIndex++;
                                if (ImGui.Selectable(item.Value.Name + "##" + item.Key,
                                        ConfigSelectedConfigurationPage == filterIndex))
                                {
                                    ConfigSelectedConfigurationPage = filterIndex;
                                }

                                var filter = PluginService.FilterService.GetFilterByKey(item.Key);
                                if (filter != null)
                                {
                                    GetFilterMenu(filter).Draw();
                                }

                            }
                        }
                    }

                    using (var commandBarChild = ImRaii.Child("CommandBar",
                               new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false))
                    {
                        if (commandBarChild.Success)
                        {

                            float height = ImGui.GetWindowSize().Y;
                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);

                            if(_addIcon.Draw("addFilter"))
                            {

                            }

                            _addFilterMenu.Draw();
                            ImGuiUtil.HoverTooltip("Add a new filter");

                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(26 * ImGui.GetIO().FontGlobalScale);

                            if (_lightBulbIcon.Draw("addSample"))
                            {

                            }

                            _addSampleMenu.Draw();
                            ImGuiUtil.HoverTooltip("Add a sample filter");

                            var width = ImGui.GetWindowSize().X;
                            width -= 24 * ImGui.GetIO().FontGlobalScale;
                            
                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);

                            if (_menuIcon.Draw("openMenu"))
                            {

                            }

                            _settingsMenu.Draw();
                        }
                    }
                }
            }
            

            ImGui.SameLine();

            using (var mainChild =
                   ImRaii.Child("Main", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (mainChild.Success)
                {
                    var count = 0;
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
                    foreach (var filter in _filterPages)
                    {
                        filterIndex2++;
                        if (ConfigSelectedConfigurationPage == filterIndex2)
                        {
                            filter.Value.Draw();
                        }
                    }
                }
            }
        }

        public override void Invalidate()
        {
            GenerateFilterPages();
        }

        public override FilterConfiguration? SelectedConfiguration => null;
    }
}
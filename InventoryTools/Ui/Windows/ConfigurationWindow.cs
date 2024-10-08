using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Autofac;
using CriticalCommonLib.Services;
using CriticalCommonLib.Services.Mediator;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings.Abstract;
using InventoryTools.Ui.MenuItems;
using InventoryTools.Ui.Widgets;
using OtterGui;
using Dalamud.Interface.Utility.Raii;
using InventoryTools.Extensions;
using InventoryTools.Mediator;
using InventoryTools.Services;
using InventoryTools.Services.Interfaces;
using InventoryTools.Ui.Pages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ImGuiUtil = OtterGui.ImGuiUtil;

namespace InventoryTools.Ui
{
    using Dalamud.Interface.Textures;

    public class ConfigurationWindow : GenericWindow, IMenuWindow
    {
        private readonly ConfigurationWizardService _configurationWizardService;
        private readonly IChatUtilities _chatUtilities;
        private readonly PluginLogic _pluginLogic;
        private readonly IListService _listService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Func<SettingCategory,SettingPage> _settingPageFactory;
        private readonly Func<Type, IConfigPage> _configPageFactory;
        private readonly Func<FilterConfiguration, FilterPage> _filterPageFactory;
        private readonly IComponentContext _context;
        private readonly InventoryToolsConfiguration _configuration;
        private IEnumerable<IMenuWindow>? _menuWindows;

        public ConfigurationWindow(ILogger<ConfigurationWindow> logger,
            MediatorService mediator,
            ImGuiService imGuiService,
            InventoryToolsConfiguration configuration,
            ConfigurationWizardService configurationWizardService,
            IChatUtilities chatUtilities,
            PluginLogic pluginLogic,
            IListService listService,
            IServiceScopeFactory serviceScopeFactory,
            Func<SettingCategory, SettingPage> settingPageFactory,
            Func<Type, IConfigPage> configPageFactory,
            Func<FilterConfiguration, FilterPage> filterPageFactory,
            IComponentContext context) : base(logger,
            mediator,
            imGuiService,
            configuration,
            "Configuration Window")
        {
            _configurationWizardService = configurationWizardService;
            _chatUtilities = chatUtilities;
            _pluginLogic = pluginLogic;
            _listService = listService;
            _serviceScopeFactory = serviceScopeFactory;
            _settingPageFactory = settingPageFactory;
            _configPageFactory = configPageFactory;
            _filterPageFactory = filterPageFactory;
            _context = context;
            _configuration = configuration;
            this.Flags = ImGuiWindowFlags.MenuBar;
        }


        public override void Initialize()
        {
            WindowName = "Configuration";
            Key = "configuration";
            _configPages = new List<IConfigPage>();
            _configPages.Add(new SeparatorPageItem("Settings"));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Lists));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Windows));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.AutoSave));
            _configPages.Add(new SeparatorPageItem("Modules", true));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.MarketBoard));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.ToolTips));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.ContextMenu));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Hotkeys));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.MobSpawnTracker));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.TitleMenuButtons));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.History));
            _configPages.Add(_settingPageFactory.Invoke(SettingCategory.Misc));
            _configPages.Add(new SeparatorPageItem("Data", true));
            _configPages.Add(_configPageFactory.Invoke(typeof(FiltersPage)));
            _configPages.Add(_configPageFactory.Invoke(typeof(CraftFiltersPage)));
            _configPages.Add(_configPageFactory.Invoke(typeof(ImportExportPage)));
            _configPages.Add(_configPageFactory.Invoke(typeof(CharacterRetainerPage)));

            _addFilterMenu = new PopupMenu("addFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("Search List", "adf1", "New Search List", AddSearchFilter, "This will create a new list that let's you search for specific items within your characters and retainers inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Sort List", "af2", "New Sort Filter", AddSortFilter, "This will create a new list that let's you search for specific items within your characters and retainers inventories then determine where they should be moved to."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Game Item List", "af3", "New Game Item List", AddGameItemFilter, "This will create a list that lets you search for all items in the game."),
                    new PopupMenu.PopupMenuItemSelectableAskName("History List", "af4", "New History Item List", AddHistoryFilter, "This will create a list that lets you view historical data of how your inventory has changed."),
                });

            _addSampleMenu = new PopupMenu("addSampleFilter", PopupMenu.PopupMenuButtons.LeftRight,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectableAskName("All", "af4", "All", AddAllFilter, "This will add a list that will be preconfigured to show items across all inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Player", "af5", "Player", AddPlayerFilter, "This will add a list that will be preconfigured to show items across all character inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Retainers", "af6", "Retainers", AddRetainersFilter, "This will add a list that will be preconfigured to show items across all retainer inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Free Company", "af7", "Free Company", AddFreeCompanyFilter, "This will add a list that will be preconfigured to show items across all free company inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("All Game Items", "af8", "All Game Items", AddAllGameItemsFilter, "This will add a list that will be preconfigured to show all of the game's items."),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectableAskName("Purchased for less than 100 gil", "af9", "Less than 100 gil", AddLessThan100GilFilter, "This will add a list that will show all items that can be purchased from gil shops under 100 gil. It will look in both character and retainer inventories."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Put away materials +", "af10", "Put away materials", AddPutAwayMaterialsFilter, "This will add a list that will be setup to quickly put away any excess materials. It will have all the material categories automatically added. When calculating where to put items it will try to prioritise existing stacks of items."),
                    new PopupMenu.PopupMenuItemSelectableAskName("Duplicated items across characters/retainers +", "af11", "Duplicated items", AddDuplicatedItemsFilter, "This will add a list that will provide a list of all the distinct stacks that appear in 2 sets of inventories. You can use this to make sure only one retainer has a specific type of item.")
                });

            _settingsMenu = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectable("Items Window", "filters", OpenFiltersWindow,"Open the items window."),
                    new PopupMenu.PopupMenuItemSelectable("Craft Window", "crafts", OpenCraftsWindow,"Open the crafts window."),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,"Open the mobs window."),
                    new PopupMenu.PopupMenuItemSelectable("Npcs Window", "npcs", OpenNpcsWindow,"Open the npcs window."),
                    new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,"Open the duties window."),
                    new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,"Open the airships window."),
                    new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenSubmarinesWindow,"Open the submarines window."),
                    new PopupMenu.PopupMenuItemSelectable("Retainer Ventures Window", "ventures", OpenRetainerVenturesWindow,"Open the retainer ventures window."),
                    new PopupMenu.PopupMenuItemSelectable("Tetris", "tetris", OpenTetrisWindow,"Open the tetris window.", () => _configuration.TetrisEnabled),
                    new PopupMenu.PopupMenuItemSeparator(),
                    new PopupMenu.PopupMenuItemSelectable("Help", "help", OpenHelpWindow,"Open the help window."),
                });

            _wizardMenu = new PopupMenu("wizardMenu", PopupMenu.PopupMenuButtons.All,
                new List<PopupMenu.IPopupMenuItem>()
                {
                    new PopupMenu.PopupMenuItemSelectable("Configure new settings", "configureNew", ConfigureNewSettings,"Configure new settings."),
                    new PopupMenu.PopupMenuItemSelectable("Configure all settings", "configureAll", ConfigureAllSettings,"Configure all settings."),
                });
            _menuWindows = _context.Resolve<IEnumerable<IMenuWindow>>().OrderBy(c => c.GenericName).Where(c => c.GetType() != this.GetType());

            GenerateFilterPages();
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ConfigurationWindowEditFilter>(this,  message =>
            {
                Invalidate();
                SetActiveFilter(message.filter);
            });
            MediatorService.Subscribe<ListInvalidatedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRepositionedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListAddedMessage>(this, _ => Invalidate());
            MediatorService.Subscribe<ListRemovedMessage>(this, _ => Invalidate());
        }

        private void ListInvalidated(ListInvalidatedMessage obj)
        {
            Invalidate();
        }

        private HoverButton _addIcon = new();
        private HoverButton _lightBulbIcon= new();
        private HoverButton _menuIcon = new ();
        private HoverButton _wizardStart = new();

        private PopupMenu _wizardMenu = null!;

        private void ConfigureAllSettings(string obj)
        {
            _configurationWizardService.ClearFeaturesSeen();
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
        }

        private void ConfigureNewSettings(string obj)
        {
            if (_configurationWizardService.HasNewFeatures)
            {
                MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
            }
            else
            {
                _chatUtilities.Print("There are no new settings available to configure.");
            }
        }

        private PopupMenu _addFilterMenu = null!;
        private PopupMenu _addSampleMenu = null!;
        private PopupMenu _settingsMenu = null!;

        private void OpenCraftsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(CraftsWindow)));
        }

        private void OpenFiltersWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(FiltersWindow)));
        }

        private void OpenHelpWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
        }

        private void OpenDutiesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(DutiesWindow)));
        }

        private void OpenAirshipsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(AirshipsWindow)));
        }

        private void OpenSubmarinesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(SubmarinesWindow)));
        }

        private void OpenRetainerVenturesWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(RetainerTasksWindow)));
        }

        private void OpenMobsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(BNpcsWindow)));
        }

        private void OpenNpcsWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(ENpcsWindow)));
        }

        private void OpenTetrisWindow(string obj)
        {
            MediatorService.Publish(new OpenGenericWindowMessage(typeof(TetrisWindow)));
        }

        private void AddAllGameItemsFilter(string arg1, string arg2)
        {
            _pluginLogic.AddAllGameItemsFilter(arg1);
            SetNewFilterActive();
        }

        private void AddFreeCompanyFilter(string arg1, string arg2)
        {
            _pluginLogic.AddFreeCompanyFilter(arg1);
            SetNewFilterActive();
        }

        private void AddRetainersFilter(string arg1, string arg2)
        {
            _pluginLogic.AddRetainerFilter(arg1);
            SetNewFilterActive();
        }

        private void AddPlayerFilter(string arg1, string arg2)
        {
            _pluginLogic.AddPlayerFilter(arg1);
            SetNewFilterActive();
        }

        private void AddAllFilter(string arg1, string arg2)
        {
            _pluginLogic.AddAllFilter(arg1);
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
                var existingFilter = _listService.GetListByKey(id);
                if (existingFilter != null)
                {
                    _listService.RemoveList(existingFilter);
                    ConfigSelectedConfigurationPage--;
                }
            }
        }

        private void MoveFilterDown(string id)
        {
            id = id.Replace("md_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListDown(existingFilter);
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.MoveListUp(existingFilter);
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = _listService.GetListByKey(id);
            if (existingFilter != null)
            {
                _listService.DuplicateList(existingFilter, filterName);
                SetNewFilterActive();
            }
        }

        private void AddDuplicatedItemsFilter(string newName, string id)
        {
            _pluginLogic.AddSampleFilterDuplicatedItems(newName);
            SetNewFilterActive();
        }

        private void AddPutAwayMaterialsFilter(string newName, string id)
        {
            _pluginLogic.AddSampleFilterMaterials(newName);
            SetNewFilterActive();
        }

        private void AddLessThan100GilFilter(string newName, string id)
        {
            _pluginLogic.AddSampleFilter100Gil(newName);
            SetNewFilterActive();
        }

        private void AddSearchFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.SearchFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }

        private void AddHistoryFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,
                Guid.NewGuid().ToString("N"), FilterType.HistoryFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }

        private void AddGameItemFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.GameItemFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }

        private void AddSortFilter(string newName, string id)
        {
            var filterConfiguration = new FilterConfiguration(newName,Guid.NewGuid().ToString("N"), FilterType.SortingFilter);
            _listService.AddDefaultColumns(filterConfiguration);
            _listService.AddList(filterConfiguration);
            SetNewFilterActive();
        }


        private int ConfigSelectedConfigurationPage
        {
            get => _configuration.SelectedConfigurationPage;
            set => _configuration.SelectedConfigurationPage = value;
        }

        public void SetActiveFilter(FilterConfiguration configuration)
        {
            var filterIndex = _filterPages.ContainsKey(configuration.Key) ? _filterPages.Where(c => !c.Value.IsMenuItem).Select(c => c.Key).IndexOf(configuration.Key) - 2 : -1;
            if (filterIndex != -1)
            {
                ConfigSelectedConfigurationPage = _configPages.Count + filterIndex;
            }
        }

        public void GenerateFilterPages()
        {
            var filterConfigurations = _listService.Lists.Where(c => c.FilterType != FilterType.CraftFilter);
            var filterPages = new Dictionary<string, IConfigPage>();
            foreach (var filter in filterConfigurations)
            {
                if (!filterPages.ContainsKey(filter.Key))
                {
                    filterPages.Add(filter.Key, _filterPageFactory.Invoke(filter));
                }
            }

            _filterPages = filterPages;
        }

        public override bool SaveState => true;
        public override Vector2? DefaultSize { get; } = new(700, 700);
        public override Vector2? MaxSize { get; } = new(2000, 2000);
        public override Vector2? MinSize { get; } = new(200, 200);
        public override string GenericKey => "configuration";
        public override string GenericName => "Configuration";
        public override bool DestroyOnClose => true;
        private List<IConfigPage> _configPages = null!;
        public Dictionary<string, IConfigPage> _filterPages = new Dictionary<string,IConfigPage>();


        private void SetNewFilterActive()
        {
            ConfigSelectedConfigurationPage = _configPages.Count + _filterPages.Count - 2;
        }

        private void DrawMenuBar()
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Report a Issue"))
                    {
                        "https://github.com/Critical-Impact/AllaganMarket".OpenBrowser();
                    }

                    if (ImGui.MenuItem("Help"))
                    {
                        this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(HelpWindow)));
                    }

                    if (ImGui.MenuItem("Ko-Fi"))
                    {
                        "https://ko-fi.com/critical_impact".OpenBrowser();
                    }

                    if (ImGui.MenuItem("Close"))
                    {
                        this.IsOpen = false;
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Wizard"))
                {
                    var hasNewFeatures = this._configurationWizardService.HasNewFeatures;
                    using var disabled = ImRaii.Disabled(!hasNewFeatures);
                    if (ImGui.MenuItem("Configure New Features"))
                    {
                        this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
                    }

                    disabled.Dispose();

                    if (ImGui.MenuItem("Reconfigure All Features"))
                    {
                        this._configurationWizardService.ClearFeaturesSeen();
                        this.MediatorService.Publish(new OpenGenericWindowMessage(typeof(ConfigurationWizard)));
                    }

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Windows"))
                {
                    if (_menuWindows != null)
                    {
                        foreach (var window in _menuWindows)
                        {
                            if (ImGui.MenuItem(window.GenericName))
                            {
                                this.MediatorService.Publish(new OpenGenericWindowMessage(window.GetType()));
                            }
                        }
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }
        }

        public override void Draw()
        {
            DrawMenuBar();
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
                                    MediatorService.Publish(configPage.Draw());
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
                            ImGui.TextUnformatted("Item Lists");
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

                                var filter = _listService.GetListByKey(item.Key);
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

                            if(_addIcon.Draw(ImGuiService.GetIconTexture(66315).ImGuiHandle, "addFilter"))
                            {

                            }

                            _addFilterMenu.Draw();
                            ImGuiUtil.HoverTooltip("Add a new filter");

                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(26 * ImGui.GetIO().FontGlobalScale);

                            if (_lightBulbIcon.Draw(ImGuiService.GetIconTexture(66318).ImGuiHandle,"addSample"))
                            {

                            }

                            _addSampleMenu.Draw();
                            ImGuiUtil.HoverTooltip("Add a sample filter");

                            var width = ImGui.GetWindowSize().X;
                            width -= 24 * ImGui.GetIO().FontGlobalScale;

                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(width);

                            if (_menuIcon.Draw(ImGuiService.GetImageTexture("menu").ImGuiHandle, "openMenu"))
                            {

                            }

                            _settingsMenu.Draw();


                            width -= 26 * ImGui.GetIO().FontGlobalScale;

                            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
                            ImGui.SetCursorPosX(width);

                            if (_wizardStart.Draw(ImGuiService.GetImageTexture("wizard").ImGuiHandle, "openMenu"))
                            {
                                _wizardMenu.Open();
                            }
                            _wizardMenu.Draw();


                            ImGuiUtil.HoverTooltip("Start configuration wizard.");
                        }
                    }
                }
            }



            ImGui.SameLine();

            IConfigPage? currentConfigPage = null;

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
                        currentConfigPage = _configPages[index];
                    }
                }

                var filterIndex2 = _configPages.Count - count;
                foreach (var filter in _filterPages)
                {
                    filterIndex2++;
                    if (ConfigSelectedConfigurationPage == filterIndex2)
                    {
                        currentConfigPage = filter.Value;
                    }
                }
            }

            using (var mainChild =
                   ImRaii.Child("Main", new Vector2(-1, -1), currentConfigPage?.DrawBorder ?? false, ImGuiWindowFlags.HorizontalScrollbar))
            {
                if (mainChild.Success)
                {
                    if (currentConfigPage != null)
                    {
                        MediatorService.Publish(currentConfigPage.Draw());
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
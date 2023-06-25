using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using InventoryTools.Extensions;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;
using InventoryTools.Ui.Widgets;
using OtterGui.Raii;
using PopupMenu = InventoryTools.Ui.Widgets.PopupMenu;

namespace InventoryTools.Ui;

public abstract class GenericFiltersWindow : Window
{
    private Dictionary<FilterConfiguration, Widgets.PopupMenu> _popupMenus = new();
    private List<FilterConfiguration>? _filters;
    private int? _newTab;
    private DateTime? _applyNewTabTime;
    private int _selectedFilterTab;
    private bool _createPopupOpen;
    private string _newFilterName = "";
    private bool SwitchNewTab => _newTab != null && _applyNewTabTime != null && _applyNewTabTime.Value <= DateTime.Now;
    private HoverButton _addButton { get; } = new(PluginService.IconStorage.LoadIcon(66315),  new Vector2(22, 22));
    private HoverButton _editButton { get; } = new(PluginService.IconStorage.LoadImage("edit"),  new Vector2(22, 22));
    private HoverButton _menuButton { get; } = new(PluginService.IconStorage.LoadImage("menu"),  new Vector2(22, 22));
    private HoverButton _rightPopoutButton { get; }

    protected GenericFiltersWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
        _rightPopoutButton = new(PluginService.IconStorage.LoadIcon(RightPopoutIcon ?? 66320), new Vector2(22, 22));
    }
    

    public abstract WindowLayout WindowLayout { get; }
    public abstract bool HasLeftPopout { get; }
    public abstract bool HasRightPopout { get; }
    public abstract WindowSidebarSide SidebarSide { get; }
    public abstract void DrawFilter(FilterConfiguration filterConfiguration);
    public abstract void DrawEmptyFilterView();
    
    public bool LeftPopoutOpen { get; set; }
    public bool RightPopoutOpen { get; set; }
    public abstract bool HasCreateTab { get; }
    public abstract bool AllowInWindowSettings { get; }
    public abstract string FormattedFilterNameLC { get; }

    public virtual Vector2 SidebarSizeLimits { get; } = new Vector2(20, 300);

    public int SidebarSize
    {
        get => _sidebarSize;
        set
        {
            _sidebarSize = (int)Math.Clamp(value, SidebarSizeLimits.X, SidebarSizeLimits.Y);
        }
    }

    private int? _sizeBarResize;

    private int _sidebarSize = 180;

    //Overridable icons
    public virtual int? RightPopoutIcon { get; }
    
    //Overridable strings
    public virtual string? RightPopupTooltipString { get; }

    public override unsafe void Draw()
    {
        if (WindowLayout == WindowLayout.Sidebar)
        {
            if (!ImGui.IsMouseDown(ImGuiMouseButton.Left))
            {
                if (_sizeBarResize != null)
                {
                    _sizeBarResize = null;
                }
            }
            
            if (SidebarSide == WindowSidebarSide.Left)
            {
                DrawSidebar();
                ImGui.SameLine();
                using (var rightDragger = ImRaii.Child("LeftDragger", new System.Numerics.Vector2(2, 0), false))
                {
                    if (rightDragger.Success)
                    {
                        ImGui.Button("LeftDraggerBtn", new(-1, -1));
                        if (ImGui.IsItemActive())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
                            if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                            {
                                if (_sizeBarResize == null)
                                {
                                    _sizeBarResize = SidebarSize;
                                }
                                var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                                SidebarSize = (int)(_sizeBarResize + mouseDragDelta.X);
                            }
                        }
                    }
                }
                
                ImGui.SameLine();
            }

            DrawLayout();
            
            if (SidebarSide == WindowSidebarSide.Right)
            {
                ImGui.SameLine();
                using (var rightDragger = ImRaii.Child("RightDragger", new System.Numerics.Vector2(2, 0), false))
                {
                    if (rightDragger.Success)
                    {
                        ImGui.Button("RightDraggerBtn", new(-1, -1));
                        if (ImGui.IsItemActive())
                        {
                            ImGui.SetMouseCursor(ImGuiMouseCursor.ResizeEW);
                            if (ImGui.IsMouseDown(ImGuiMouseButton.Left))
                            {
                                if (_sizeBarResize == null)
                                {
                                    _sizeBarResize = SidebarSize;
                                }
                                var mouseDragDelta = ImGui.GetMouseDragDelta(ImGuiMouseButton.Left, 0);
                                SidebarSize = (int)(_sizeBarResize - mouseDragDelta.X);
                            }
                        }
                    }
                }
                
                ImGui.SameLine();


                DrawSidebar();
            }
        }
        else
        {
            DrawTabBar();
        }
    }

    public virtual void DrawLeftPopout()
    {
        
    }

    public virtual void DrawRightPopout()
    {
        
    }


    public virtual void DrawTopBar()
    {
        using (var topBarChild = ImRaii.Child("TopBar", new Vector2(0, 40) * ImGui.GetIO().FontGlobalScale, true,
                   ImGuiWindowFlags.NoScrollbar))
        {
            if (!topBarChild.Success) return;
            
            ImGui.SameLine();
            float width = ImGui.GetWindowSize().X;
            width -= 28;
            ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);
            if (_rightPopoutButton.Draw("tb_rpb"))
            {
                RightPopoutOpen = !RightPopoutOpen;
            }

            OtterGui.ImGuiUtil.HoverTooltip(RightPopupTooltipString ?? "Toggles the add item side bar.");

            ImGui.SameLine();
            width -= 28;
            ImGui.SetCursorPosX(width * ImGui.GetIO().FontGlobalScale);
            if (_editButton.Draw("tb_edit"))
            {
                SettingsActive = !SettingsActive;
            }

            OtterGui.ImGuiUtil.HoverTooltip("Edit the craft list's configuration.");
        }
    }

    public virtual void DrawFilterView(FilterConfiguration filterConfiguration)
    {
        using (var contentChild = ImRaii.Child("Content", new Vector2(0, -44) * ImGui.GetIO().FontGlobalScale, true))
        {
            if (!contentChild.Success) return;
            if (SettingsActive)
            {
                DrawSettingsEdit(filterConfiguration);
            }
            else
            {
                DrawFilter(filterConfiguration);
            }
        }
    }

    public virtual void DrawBottomBar()
    {
        using (var bottomBarChild = ImRaii.Child("BottomBar", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale,
                   true, ImGuiWindowFlags.NoScrollbar))
        {
            if (!bottomBarChild.Success) return;

            var width = ImGui.GetWindowSize().X;
            
            width -= 30 * ImGui.GetIO().FontGlobalScale;
            ImGui.SetCursorPosX(width);
            UiHelpers.CenterElement(24 * ImGui.GetIO().FontGlobalScale);
            if (_menuButton.Draw("openMenu"))
            {
            }
            MainMenu.Draw();
        }
    }

    public virtual void DrawExtraTabBars()
    {
        
    }

    public virtual void DrawSettingsEdit(FilterConfiguration filterConfiguration)
    {
        var filterName = filterConfiguration.Name;
        var labelName = "##" + filterConfiguration.Key;
        if (ImGui.CollapsingHeader("General",
                ImGuiTreeNodeFlags.DefaultOpen | ImGuiTreeNodeFlags.CollapsingHeader))
        {
            ImGui.SetNextItemWidth(100);
            ImGui.LabelText(labelName + "FilterNameLabel", "Name: ");
            ImGui.SameLine();
            ImGui.InputText(labelName + "FilterName", ref filterName, 100);
            if (filterName != filterConfiguration.Name)
            {
                filterConfiguration.Name = filterName;
            }

            ImGui.NewLine();
            if (ImGui.Button("Export Configuration to Clipboard"))
            {
                var base64 = filterConfiguration.ExportBase64();
                ImGui.SetClipboardText(base64);
                PluginService.ChatUtilities.PrintClipboardMessage("[Export] ", "Filter Configuration");
            }

            var filterType = filterConfiguration.FormattedFilterType;
            ImGui.SetNextItemWidth(100);
            ImGui.LabelText(labelName + "FilterTypeLabel", "Filter Type: ");
            ImGui.SameLine();
            ImGui.TextDisabled(filterType);

        }

        using (var tabBar = ImRaii.TabBar("ConfigTabs", ImGuiTabBarFlags.FittingPolicyScroll))
        {
            if (tabBar.Success)
            {
                foreach (var group in PluginService.PluginLogic.GroupedFilters)
                {
                    var hasValuesSet = false;
                    foreach (var filter in group.Value)
                    {
                        if (filter.HasValueSet(filterConfiguration))
                        {
                            hasValuesSet = true;
                            break;
                        }
                    }

                    using var color = ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.HealerGreen,
                        hasValuesSet);

                    var hasValues = group.Value.Any(filter =>
                        filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                        filterConfiguration.FilterType.HasFlag(
                            FilterType.SearchFilter)
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .SortingFilter))
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .CraftFilter))
                        ||
                        (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                         filterConfiguration.FilterType.HasFlag(FilterType
                             .GameItemFilter)));
                    if (hasValues)
                    {
                        using (var tabItem = ImRaii.TabItem(group.Key.ToString().ToSentence()))
                        {
                            if (!tabItem.Success) continue;
                            using (ImRaii.PushColor(ImGuiCol.Text, ImGuiColors.DalamudWhite))
                            {
                                foreach (var filter in group.Value)
                                {
                                    if ((filter.AvailableIn.HasFlag(FilterType.SearchFilter) &&
                                         filterConfiguration.FilterType.HasFlag(FilterType.SearchFilter)
                                         ||
                                         (filter.AvailableIn.HasFlag(FilterType.SortingFilter) &&
                                          filterConfiguration.FilterType.HasFlag(FilterType.SortingFilter))
                                         ||
                                         (filter.AvailableIn.HasFlag(FilterType.CraftFilter) &&
                                          filterConfiguration.FilterType.HasFlag(FilterType.CraftFilter))
                                         ||
                                         (filter.AvailableIn.HasFlag(FilterType.GameItemFilter) &&
                                          filterConfiguration.FilterType.HasFlag(FilterType.GameItemFilter))
                                        ))
                                    {
                                        filter.Draw(filterConfiguration);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public virtual void AddNewFilter(string newFilterName)
    {
        
    }

    public virtual unsafe void DrawTabBar()
    {
        if (HasCreateTab && _createPopupOpen)
        {
            ImGui.OpenPopup("addFilterChooseName");
            _createPopupOpen = false;
        }
        if (HasCreateTab && ImGuiUtil.OpenNameField("addFilterChooseName", ref _newFilterName))
        {
            PluginService.FrameworkService.RunOnFrameworkThread(() =>
            {
                AddNewFilter(_newFilterName);
                _newFilterName = "";
            });
        }
        
        using (var tabBar = ImRaii.TabBar("TabBar",
                   ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.TabListPopupButton))
        {
            if (!tabBar.Success) return;
            var filterConfigurations = Filters;
            for (var index = 0; index < filterConfigurations.Count; index++)
            {
                var filterConfiguration = filterConfigurations[index];
                using var id = ImRaii.PushId(index);
                var imGuiTabItemFlags = _newTab == index && SwitchNewTab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
                fixed (byte* namePtr = filterConfiguration.NameAsBytes)
                {
                    using (var tabItem = ImRaii.TabItem(namePtr, imGuiTabItemFlags))
                    {
                        if (SwitchNewTab && _newTab != null && _newTab == index)
                        {
                            _newTab = null;
                            _applyNewTabTime = null;
                        }
                        GetFilterMenu(filterConfiguration, WindowLayout.Tabs).Draw();

                        if (tabItem.Success)
                        {
                            _selectedFilterTab = index;
                            DrawLayout();
                        }
                    }
                }
            }
            if (HasCreateTab && ImGui.TabItemButton("+", ImGuiTabItemFlags.Trailing | ImGuiTabItemFlags.NoTooltip))
            {
                _createPopupOpen = true;
            }
        }
    }

    public virtual unsafe void DrawSidebar()
    {
        using (var sideMenuChild = ImRaii.Child("SideMenu", new Vector2(SidebarSide == WindowSidebarSide.Left ? SidebarSize : -1, -1) * ImGui.GetIO().FontGlobalScale, true))
        {
            if (!sideMenuChild.Success) return;
            
            DrawSidebarList();
            DrawSidebarCommandBar();
            
        }
    }

    public virtual void DrawSidebarList()
    {
        var filterConfigurations = Filters;

        using (var craftListChild =
               ImRaii.Child("Sidebar", new Vector2(0, -28) * ImGui.GetIO().FontGlobalScale, false))
        {
            if (!craftListChild.Success) return;
            
            for (var index = 0; index < filterConfigurations.Count; index++)
            {
                var filterConfiguration = filterConfigurations[index];
                if (ImGui.Selectable(filterConfiguration.Name + "###fl" + filterConfiguration.Key,
                        index == _selectedFilterTab))
                {
                    _selectedFilterTab = index;
                    if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                        ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key &&
                        ConfigurationManager.Config.ActiveUiFilter != null)
                    {
                        PluginService.FrameworkService.RunOnFrameworkThread(() =>
                        {
                            PluginService.FilterService.ToggleActiveUiFilter(filterConfiguration);
                        });
                    }
                }

                GetFilterMenu(filterConfiguration, WindowLayout.Sidebar).Draw();
            }
        }
    }

    public virtual void DrawSidebarCommandBar()
    {
        using (var commandBarChild = ImRaii.Child("CommandBar", new Vector2(0, 0) * ImGui.GetIO().FontGlobalScale, false))
        {
            if (!commandBarChild.Success) return;
            
            float height = ImGui.GetWindowSize().Y;
            ImGui.SetCursorPosY(height - 24 * ImGui.GetIO().FontGlobalScale);
            if (_addButton.Draw("sb_afb"))
            {
                PluginService.PluginLogic.AddNewCraftFilter();
            }

            OtterGui.ImGuiUtil.HoverTooltip("Add a new " + FormattedFilterNameLC + ".");
        }
    }

    public virtual unsafe void DrawLayout()
    {
        if (HasLeftPopout && LeftPopoutOpen)
        {
            DrawLeftPopout();
        }

        var mainWindowSize = -1;
        if (RightPopoutOpen && SidebarSide == WindowSidebarSide.Right)
        {
            mainWindowSize = -250 - SidebarSize;
        }
        else if (RightPopoutOpen)
        {
            mainWindowSize = -250;
        }
        else if (SidebarSide == WindowSidebarSide.Right)
        {
            mainWindowSize = -SidebarSize;
        }
        using (var child = ImRaii.Child("Main",
                   new Vector2(mainWindowSize, -1) * ImGui.GetIO().FontGlobalScale, false,
                   ImGuiWindowFlags.HorizontalScrollbar))
        {
            if (!child.Success) return;
            
            if (Filters.Count == 0)
            {
                DrawEmptyFilterView();
                return;
            }

            var selectedConfiguration = SelectedConfiguration;
            if (selectedConfiguration == null) return;
            
            //Need to work in filter swapping somewhere here
            
            DrawTopBar();
            DrawFilterView(selectedConfiguration);
            DrawBottomBar();
        }
        
        if (HasRightPopout && RightPopoutOpen)
        {
            DrawRightPopout();
        }
    }
    

    public abstract List<FilterConfiguration> RefreshFilters();
    
    public List<FilterConfiguration> Filters
    {
        get
        {
            if (_filters == null)
            {
                _filters = RefreshFilters();
            }

            return _filters;
        }
    }

    public PopupMenu MainMenu { get; } = new PopupMenu("configMenu", PopupMenu.PopupMenuButtons.All,
        new List<PopupMenu.IPopupMenuItem>()
        {
            new PopupMenu.PopupMenuItemSelectable("Mob Window", "mobs", OpenMobsWindow,
                "Open the mobs window."),
            new PopupMenu.PopupMenuItemSelectable("Duties Window", "duties", OpenDutiesWindow,
                "Open the duties window."),
            new PopupMenu.PopupMenuItemSelectable("Airships Window", "airships", OpenAirshipsWindow,
                "Open the airships window."),
            new PopupMenu.PopupMenuItemSelectable("Submarines Window", "submarines", OpenAirshipsWindow,
                "Open the submarines window."),
            new PopupMenu.PopupMenuItemSeparator(),
            new PopupMenu.PopupMenuItemSelectable("Help", "help", OpenHelpWindow, "Open the help window."),
        });

    public bool SettingsActive { get; set; }

    public Widgets.PopupMenu GetFilterMenu(FilterConfiguration configuration, WindowLayout layout)
    {
        if (!_popupMenus.ContainsKey(configuration))
        {
            _popupMenus[configuration] = new Widgets.PopupMenu("fm" + configuration.Key, Widgets.PopupMenu.PopupMenuButtons.Right,
                new List<Widgets.PopupMenu.IPopupMenuItem>()
                {
                    new Widgets.PopupMenu.PopupMenuItemSelectable("Edit", "ef_" + configuration.Key, EditFilter, "Edit the craft list."),
                    new Widgets.PopupMenu.PopupMenuItemSelectableAskName("Duplicate", "df_" + configuration.Key, configuration.Name, DuplicateFilter, "Duplicate the craft list."),
                    new Widgets.PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Left" : "Move Up", "mu_" + configuration.Key, MoveFilterUp, layout == WindowLayout.Tabs ? "Move the craft list left." : "Move the craft list up."),
                    new Widgets.PopupMenu.PopupMenuItemSelectable(layout == WindowLayout.Tabs ? "Move Right" : "Move Down", "md_" + configuration.Key, MoveFilterDown, layout == WindowLayout.Tabs ? "Move the craft list right." : "Move the craft list down."),
                    new Widgets.PopupMenu.PopupMenuItemSelectableConfirm("Remove", "rf_" + configuration.Key, "Are you sure you want to remove this craft list?", RemoveFilter, "Remove the craft list."),
                }
            );
        }

        return _popupMenus[configuration];
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

    private static void OpenMobsWindow(string obj)
    {
        PluginService.WindowService.OpenWindow<BNpcWindow>(BNpcWindow.AsKey);
    }
    
    private void EditFilter(string id)
        {
            id = id.Replace("ef_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                SetActiveFilter(existingFilter);
                SettingsActive = true;
            }
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
                }
            }
        }

        private void MoveFilterDown(string id)
        {
            id = id.Replace("md_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                var currentFilter = this.SelectedConfiguration;
                PluginService.FilterService.MoveFilterDown(existingFilter);
                if (currentFilter != null)
                {
                    SetActiveFilter(currentFilter);
                    FocusFilter(currentFilter);
                }
            }
        }

        private void MoveFilterUp(string id)
        {
            id = id.Replace("mu_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                var currentFilter = this.SelectedConfiguration;
                PluginService.FilterService.MoveFilterUp(existingFilter);
                if (currentFilter != null)
                {
                    SetActiveFilter(currentFilter);
                    FocusFilter(currentFilter);
                }
            }
        }

        private void DuplicateFilter(string filterName, string id)
        {
            id = id.Replace("df_", "");
            var existingFilter = PluginService.FilterService.GetFilterByKey(id);
            if (existingFilter != null)
            {
                var newFilter = PluginService.FilterService.DuplicateFilter(existingFilter, filterName);
                SetActiveFilter(newFilter);
            }
        }
        
        public void SetActiveFilter(FilterConfiguration configuration)
        {
            var filterIndex = Filters.Contains(configuration) ? Filters.IndexOf(configuration) : -1;
            if (filterIndex != -1)
            {
                _newTab = filterIndex;
                _applyNewTabTime = DateTime.Now + TimeSpan.FromMilliseconds(5);
                //ImGui being shit workaround
            }
        }
        
        public void FocusFilter(FilterConfiguration filterConfiguration, bool showSettings = false)
        {
            var filterConfigurations = Filters;
            if (filterConfigurations.Contains(filterConfiguration))
            {
                _selectedFilterTab = filterConfigurations.IndexOf(filterConfiguration);
                if (showSettings)
                {
                    SettingsActive = true;
                }
            }
        }
        
        public override FilterConfiguration? SelectedConfiguration
        {
            get
            {
                if (_selectedFilterTab >= 0 && _selectedFilterTab < Filters.Count) return Filters[_selectedFilterTab];
                return null;
            }
        }
    
        public override void Invalidate()
        {
            _filters = null;
        }
}
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public static class GeneralConfigurationSection
    {
        private static InventoryToolsConfiguration Configuration => ConfigurationManager.Config;

        public static void Draw()
        {
            var activeUiFilter = PluginService.PluginLogic.GetActiveUiFilter();
            var activeBackgroundFilter = PluginService.PluginLogic.GetActiveBackgroundFilter();
            var filterItems = new string[PluginService.PluginLogic.FilterConfigurations.Count + 1];

            var selectedUiFilter = 0;
            filterItems[0] = "None";
            for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
            {
                var config = PluginService.PluginLogic.FilterConfigurations[index];
                if (activeUiFilter == config)
                {
                    selectedUiFilter = index + 1;
                }

                filterItems[index + 1] = config.Name;
            }

            ImGui.Text("Filter Details:");
            ImGui.Separator();
            ImGui.Text("Window Filter: ");
            ImGui.SameLine();
            if (ImGui.Combo("##WindowFilter", ref selectedUiFilter,
                filterItems, filterItems.Length))
            {
                for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
                {
                    if (selectedUiFilter == 0)
                    {
                        PluginService.PluginLogic.DisableActiveUiFilter();
                    }
                    else if (selectedUiFilter - 1 == index)
                    {
                        var config = PluginService.PluginLogic.FilterConfigurations[index];
                        if (activeUiFilter != config)
                        {
                            PluginService.PluginLogic.ToggleActiveUiFilterByKey(config.Key);
                            PluginService.PluginLogic.GetFilterTable(config.Key)?.Refresh(Configuration);
                            break;
                        }
                    }
                }
            }

            ImGui.SameLine();

            UiHelpers.HelpMarker(
                "This is the filter that is active when the Inventory Tools window is visible.");

            var selectedBackgroundFilter = 0;
            for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
            {
                var config = PluginService.PluginLogic.FilterConfigurations[index];
                if (activeBackgroundFilter == config)
                {
                    selectedBackgroundFilter = index + 1;
                }
            }

            ImGui.Text("Background Filter: ");
            ImGui.SameLine();
            if (ImGui.Combo("##BackgroundFilter", ref selectedBackgroundFilter,
                filterItems, filterItems.Length))
            {
                for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
                {
                    if (selectedBackgroundFilter == 0)
                    {
                        PluginService.PluginLogic.DisableActiveBackgroundFilter();
                    }
                    else if (selectedBackgroundFilter - 1 == index)
                    {
                        var config = PluginService.PluginLogic.FilterConfigurations[index];
                        if (activeBackgroundFilter != config)
                        {
                            PluginService.PluginLogic.ToggleActiveBackgroundFilterByKey(config.Key);
                            break;
                        }
                    }
                }
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(
                "This is the filter that is active when the Inventory Tools window is not visible. This filter can be toggled with the associated slash commands.");
            ImGui.Text("General Options:");
            ImGui.Separator();
            bool showMonitorTab = Configuration.ShowFilterTab;
            bool switchFiltersAutomatically = Configuration.SwitchFiltersAutomatically;
            bool autoSave = Configuration.AutoSave;
            int autoSaveMinutes = Configuration.AutoSaveMinutes;
            bool displayCrossCharacter = Configuration.DisplayCrossCharacter;
            bool displayTooltip = Configuration.DisplayTooltip;

            if (ImGui.Checkbox("Show Filters Tab?", ref showMonitorTab))
            {
                Configuration.ShowFilterTab = !Configuration.ShowFilterTab;
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(
                "Should the main window show the tab called 'Filters' that lists all the available filters in one screen?");

            if (ImGui.Checkbox("Switch filters when navigating UI?", ref switchFiltersAutomatically))
            {
                Configuration.SwitchFiltersAutomatically = !Configuration.SwitchFiltersAutomatically;
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(
                "Should the active window filter automatically change when moving between each filter tab? The active filter will only change if there is an active filter already selected.");

            if (ImGui.Checkbox("Show Tooltip?", ref displayTooltip))
            {
                Configuration.DisplayTooltip = !Configuration.DisplayTooltip;
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(
                "When hovering an item, show additional information about the item including it's location in inventories and market price(if available).");

            ImGui.Text("Auto Save:");
            ImGui.Separator();
            if (ImGui.Checkbox("Auto save inventories/configuration?", ref autoSave))
            {
                Configuration.AutoSave = !Configuration.AutoSave;
                PluginService.PluginLogic.ClearAutoSave();
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(
                "Should the inventories/configuration be automatically saved on a defined interval? While the plugin does save when the game is closed and when configurations are altered, it is not saved in cases of crashing so this attempts to alleviate this.");

            ImGui.SetNextItemWidth(100);
            if (ImGui.InputInt("Auto save every:", ref autoSaveMinutes))
            {
                if (autoSaveMinutes != Configuration.AutoSaveMinutes)
                {
                    Configuration.AutoSaveMinutes = autoSaveMinutes;
                    PluginService.PluginLogic.ClearAutoSave();
                }
            }

            ImGui.Text("Next Autosave: " + (PluginService.PluginLogic.NextSaveTime?.ToString() ?? "N/A"));
            ImGui.SameLine();
            UiHelpers.HelpMarker(
                "How many minutes should there be between each auto save?");

            ImGui.Text("Advanced Settings:");
            ImGui.Separator();
            if (ImGui.Checkbox("Allow Cross-Character Inventories?", ref displayCrossCharacter))
            {
                Configuration.DisplayCrossCharacter = !Configuration.DisplayCrossCharacter;
            }

            ImGui.SameLine();
            UiHelpers.HelpMarker(
                "This is an experimental feature, should characters not currently logged in and their associated retainers be shown in filter configurations?");
        }
    }
}
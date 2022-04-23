using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Sections
{
    public static class FiltersSection
    {
        private static int _selectedFilterTab = 0;

        public static void Draw()
        {
            if (ImGui.BeginChild("###monitorLeft", new Vector2(100, -1) * ImGui.GetIO().FontGlobalScale, true))
            {
                for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
                {
                    var filterConfiguration = PluginService.PluginLogic.FilterConfigurations[index];
                    if (ImGui.Selectable(filterConfiguration.Name, index == _selectedFilterTab))
                    {
                        if (ConfigurationManager.Config.SwitchFiltersAutomatically && ConfigurationManager.Config.ActiveUiFilter != filterConfiguration.Key)
                        {
                            PluginService.PluginLogic.ToggleActiveUiFilterByKey(filterConfiguration.Key);
                        }

                        _selectedFilterTab = index;
                    }
                }

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("###monitorRight", new Vector2(-1, -1), true, ImGuiWindowFlags.HorizontalScrollbar))
            {
                for (var index = 0; index < PluginService.PluginLogic.FilterConfigurations.Count; index++)
                {
                    if (_selectedFilterTab == index)
                    {
                        var filterConfiguration = PluginService.PluginLogic.FilterConfigurations[index];
                        var table = PluginService.PluginLogic.GetFilterTable(filterConfiguration.Key);
                        if (table != null)
                        {
                            table.Draw();
                        }
                    }
                }
                ImGui.EndChild();
            }
        }
    }
}
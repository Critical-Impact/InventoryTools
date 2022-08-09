using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;

namespace InventoryTools.Ui
{
    public class FilterWindow : Window
    {
        public FilterWindow(string filterKey)
        {
            _filterKey = filterKey;
        }
        
        public override void Invalidate()
        {
            
        }

        private string _filterKey;
        
        public static string AsKey(string filterKey)
        {
            return "filter_" + filterKey;
        }

        public override FilterConfiguration? SelectedConfiguration =>
            PluginService.FilterService.GetFilterByKey(_filterKey);
        
        public override string Name => "Allagan Tools - " + (SelectedConfiguration?.Name ?? "Unknown");
        public override string Key => AsKey(_filterKey);
        public override bool DestroyOnClose => true;
        public override bool SaveState => true;
        public override void Draw()
        {
            if (SelectedConfiguration != null)
            {
                if (ImGui.IsWindowFocused())
                {
                    if (ConfigurationManager.Config.SwitchFiltersAutomatically &&
                        ConfigurationManager.Config.ActiveUiFilter != SelectedConfiguration.Key &&
                        ConfigurationManager.Config.ActiveUiFilter != null)
                    {
                        PluginService.FilterService.ToggleActiveUiFilter(SelectedConfiguration);
                    }
                }
                var table = PluginService.FilterService.GetFilterTable(_filterKey);
                if (table != null)
                {
                    FiltersWindow.DrawFilter(table, SelectedConfiguration, "");
                }
            }
        }

        public override Vector2 Size => new Vector2(600, 500);
        public override Vector2 MaxSize => new Vector2(1500, 1500);
        public override Vector2 MinSize => new Vector2(200, 200);
    }
}
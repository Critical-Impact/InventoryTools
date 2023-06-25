using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using InventoryTools.Logic;
using InventoryTools.Logic.Settings;

namespace InventoryTools.Ui;

public class NewFiltersWindow : GenericFiltersWindow
{
    public NewFiltersWindow(string name, ImGuiWindowFlags flags = ImGuiWindowFlags.None, bool forceMainWindow = false) : base(name, flags, forceMainWindow)
    {
    }

    public NewFiltersWindow() : base("Filters Window")
    {
        
    }

    public static string AsKey => "NewFilters";
    public override string Key => AsKey;
    public override bool DestroyOnClose => false;
    public override bool SaveState => false;
    public override Vector2 DefaultSize => new Vector2(500, 500);
    public override Vector2 MaxSize => new Vector2(2000, 2000);
    public override Vector2 MinSize => new Vector2(100, 100);

    public override WindowLayout WindowLayout
    {
        get
        {
            return WindowLayout.Sidebar;
        }
    }

    public override bool HasLeftPopout => false;
    public override bool HasRightPopout => false;

    public override WindowSidebarSide SidebarSide
    {
        get
        {
            return WindowSidebarSide.Left;
        }
    }
    public override void DrawFilter(FilterConfiguration filterConfiguration)
    {
        var itemTable = PluginService.FilterService.GetFilterTable(filterConfiguration);
        if (itemTable == null)
        {
            return;
        }

        itemTable.Draw(new Vector2(0, 0));
    }

    public override void DrawEmptyFilterView()
    {
        ImGui.TextUnformatted("You have no filter");
    }

    public override bool HasCreateTab => true;
    public override bool AllowInWindowSettings => true;
    public override string FormattedFilterNameLC => "filter";
    public override List<FilterConfiguration> RefreshFilters()
    {
        return PluginService.FilterService.FiltersList.Where(c => c.FilterType == FilterType.SearchFilter || c.FilterType == FilterType.SortingFilter || c.FilterType == FilterType.GameItemFilter).ToList();
    }
}
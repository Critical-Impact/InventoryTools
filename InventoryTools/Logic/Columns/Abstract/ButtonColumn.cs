using System.Collections.Generic;
using CriticalCommonLib.Services.Mediator;
using DalaMock.Host.Mediator;
using ImGuiNET;
using InventoryTools.Logic.Columns.Abstract.ColumnSettings;

namespace InventoryTools.Logic.Columns.Abstract;

public abstract class ButtonColumn : IColumn
{
    public virtual void Dispose()
    {

    }

    public abstract string Name { get; set; }
    public abstract float Width { get; set; }
    public abstract string HelpText { get; set; }
    public virtual string FilterText { get; set; } = "";
    public virtual List<string>? FilterChoices { get; set; } = new();
    public virtual ColumnCategory ColumnCategory { get; } = ColumnCategory.Buttons;
    public virtual bool HasFilter { get; set; } = false;
    public virtual ColumnFilterType FilterType { get; set; } = ColumnFilterType.None;
    public virtual bool IsDebug { get; set; } = false;
    public virtual FilterType AvailableIn { get; } = Logic.FilterType.SearchFilter | Logic.FilterType.SortingFilter |
                                                      Logic.FilterType.GameItemFilter | Logic.FilterType.HistoryFilter |
                                                      Logic.FilterType.CraftFilter | Logic.FilterType.CuratedList;

    public bool AvailableInType(FilterType type) =>
        AvailableIn.HasFlag(InventoryTools.Logic.FilterType.SearchFilter) &&
        type.HasFlag(InventoryTools.Logic.FilterType.SearchFilter)
        ||
        (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.SortingFilter) &&
         type.HasFlag(InventoryTools.Logic.FilterType.SortingFilter))
        ||
        (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.CraftFilter) &&
         type.HasFlag(InventoryTools.Logic.FilterType.CraftFilter))
        ||
        (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.GameItemFilter) &&
         type.HasFlag(InventoryTools.Logic.FilterType.GameItemFilter))
        ||
        (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.HistoryFilter) &&
         type.HasFlag(InventoryTools.Logic.FilterType.HistoryFilter))
        ||
        (AvailableIn.HasFlag(InventoryTools.Logic.FilterType.CuratedList) &&
         type.HasFlag(InventoryTools.Logic.FilterType.CuratedList));

    public virtual bool? CraftOnly { get; } = null;
    public bool IsConfigurable => false;
    public virtual string? RenderName { get; }

    public List<IColumnSetting> FilterSettings { get; set; } = new();
    public List<IColumnSetting> Settings { get; set; } = new();

    public string? FilterIcon { get; set; } = null;

    public virtual IEnumerable<SearchResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SearchResult> items)
    {
        return items;
    }

    public virtual IEnumerable<SearchResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<SearchResult> items)
    {
        return items;
    }

    public abstract List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SearchResult searchResult, int rowIndex, int columnIndex);

    public virtual List<MessageBase>? DrawEditor(ColumnConfiguration columnConfiguration,
        FilterConfiguration configuration)
    {
        if (this.Settings.Count != 0)
        {
            ImGui.NewLine();
            ImGui.Separator();
            foreach (var setting in this.Settings)
            {
                setting.Draw(columnConfiguration, null);
            }
        }
        return null;
    }

    public virtual string CsvExport(ColumnConfiguration columnConfiguration, SearchResult item)
    {
        return "";
    }

    public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, SearchResult item)
    {
        return "";
    }

    public virtual void Setup(FilterConfiguration filterConfiguration, ColumnConfiguration columnConfiguration, int columnIndex)
    {
        var imGuiTableColumnFlags = ImGuiTableColumnFlags.WidthFixed;
        if (filterConfiguration.DefaultSortColumn != null && filterConfiguration.DefaultSortColumn == columnConfiguration.Key)
        {
            imGuiTableColumnFlags |= ImGuiTableColumnFlags.DefaultSort;
            if (filterConfiguration.DefaultSortOrder != null)
            {
                imGuiTableColumnFlags |= filterConfiguration.DefaultSortOrder == ImGuiSortDirection.Ascending
                    ? ImGuiTableColumnFlags.PreferSortAscending
                    : ImGuiTableColumnFlags.PreferSortDescending;
            }
        }
        ImGui.TableSetupColumn(columnConfiguration.Name ?? (RenderName ?? Name), imGuiTableColumnFlags, Width, (uint)columnIndex);
    }

    public bool? DrawFilter(ColumnConfiguration columnConfiguration, int columnIndex)
    {
        return null;
    }

    public virtual IFilterEvent? DrawFooterFilter(ColumnConfiguration columnConfiguration, FilterTable filterTable)
    {
        return null;
    }

    public virtual event IColumn.ButtonPressedDelegate? ButtonPressed;
    public virtual void InvalidateSearchCache()
    {

    }

    public virtual bool DrawFilter(string tableKey, int columnIndex)
    {
        return false;
    }

    public FilterType DefaultIn => Logic.FilterType.None;
    public uint MaxFilterLength { get; set; } = 200;
}
using System.Collections.Generic;
using CriticalCommonLib.Crafting;
using CriticalCommonLib.Models;
using CriticalCommonLib.Services.Mediator;
using CriticalCommonLib.Sheets;
using ImGuiNET;

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
                                                      Logic.FilterType.FavouriteFilter | Logic.FilterType.CraftFilter;

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
         type.HasFlag(InventoryTools.Logic.FilterType.HistoryFilter));

    public virtual bool? CraftOnly { get; } = null;
    public bool IsConfigurable => false;
    public virtual string? RenderName { get; }
    public virtual IEnumerable<InventoryItem> Filter(ColumnConfiguration columnConfiguration, IEnumerable<InventoryItem> items)
    {
        return items;
    }

    public virtual IEnumerable<SortingResult> Filter(ColumnConfiguration columnConfiguration, IEnumerable<SortingResult> items)
    {
        return items;
    }

    public virtual IEnumerable<ItemEx> Filter(ColumnConfiguration columnConfiguration, IEnumerable<ItemEx> items)
    {
        return items;
    }

    public virtual IEnumerable<CraftItem> Filter(ColumnConfiguration columnConfiguration, IEnumerable<CraftItem> items)
    {
        return items;
    }

    public virtual IEnumerable<InventoryChange> Filter(ColumnConfiguration columnConfiguration, IEnumerable<InventoryChange> items)
    {
        return items;
    }

    public virtual IEnumerable<InventoryItem> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<InventoryItem> items)
    {
        return items;
    }

    public virtual IEnumerable<SortingResult> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<SortingResult> items)
    {
        return items;
    }

    public virtual IEnumerable<ItemEx> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<ItemEx> items)
    {
        return items;
    }

    public virtual IEnumerable<CraftItem> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<CraftItem> items)
    {
        return items;
    }

    public virtual IEnumerable<InventoryChange> Sort(ColumnConfiguration columnConfiguration, ImGuiSortDirection direction, IEnumerable<InventoryChange> items)
    {
        return items;
    }

    public virtual List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        InventoryItem item,
        int rowIndex, int columnIndex)
    {
        return Draw(configuration, columnConfiguration, item.Item, rowIndex, columnIndex);
    }

    public virtual List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        SortingResult item,
        int rowIndex, int columnIndex)
    {
        return Draw(configuration, columnConfiguration, item.Item, rowIndex, columnIndex);
    }

    public abstract List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        ItemEx item, int rowIndex, int columnIndex);

    public virtual List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        CraftItem item, int rowIndex, int columnIndex)
    {
        return Draw(configuration, columnConfiguration, item.Item, rowIndex, columnIndex);
    }

    public virtual List<MessageBase>? Draw(FilterConfiguration configuration, ColumnConfiguration columnConfiguration,
        InventoryChange item,
        int rowIndex, int columnIndex)
    {
        return Draw(configuration, columnConfiguration, item.Item, rowIndex, columnIndex);
    }

    public virtual void DrawEditor(ColumnConfiguration columnConfiguration, FilterConfiguration configuration)
    {
        
    }

    public virtual string CsvExport(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return "";
    }

    public virtual string CsvExport(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return "";
    }

    public virtual string CsvExport(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return "";
    }

    public virtual string CsvExport(ColumnConfiguration columnConfiguration, CraftItem item)
    {
        return "";
    }

    public virtual string CsvExport(ColumnConfiguration columnConfiguration, InventoryChange item)
    {
        return "";
    }

    public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryItem item)
    {
        return "";
    }

    public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, SortingResult item)
    {
        return "";
    }

    public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, ItemEx item)
    {
        return "";
    }

    public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, CraftItem item)
    {
        return "";
    }

    public virtual dynamic? JsonExport(ColumnConfiguration columnConfiguration, InventoryChange item)
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

    public virtual IFilterEvent? DrawFooterFilter(FilterConfiguration configuration, FilterTable filterTable)
    {
        return null;
    }

    public virtual event IColumn.ButtonPressedDelegate? ButtonPressed;
    public virtual bool DrawFilter(string tableKey, int columnIndex)
    {
        return false;
    }

    public FilterType DefaultIn => Logic.FilterType.None;
    public uint MaxFilterLength { get; set; } = 200;
}
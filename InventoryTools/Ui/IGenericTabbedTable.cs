using System.Collections.Generic;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Ui;

public interface IGenericTabbedTable<T>
{
    public Dictionary<uint, List<T>> Items { get; }
    public Dictionary<uint, List<T>> FilteredItems { get; }
    public List<TableColumn<T>> Columns { get; }
    public ImGuiTableFlags TableFlags { get; }
    public List<T> GetItems(uint tabId);
    public Dictionary<uint,string> Tabs { get; }
    public string TableName { get; }
    public int? SortColumn { get; }
    public ImGuiSortDirection? SortDirection { get; }
    public bool UseClipper { get; }
    public float RowSize { get; }
}
using System.Collections.Generic;
using Dalamud.Bindings.ImGui;

namespace InventoryTools.Ui;

public interface ITableColumn
{
    string Name { get; }
    uint Width { get; }
    ImGuiTableColumnFlags ColumnFlags { get; }

    public IEnumerable<T> Sort<T>(ImGuiSortDirection direction, IEnumerable<T> items);


}
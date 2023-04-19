using System;
using System.Collections.Generic;
using ImGuiNET;
using Action = Lumina.Excel.GeneratedSheets.Action;

namespace InventoryTools.Ui;

public class TableColumn<T>
{
    public TableColumn(string name, uint width, ImGuiTableColumnFlags columnFlags, bool allTabOnly = false)
    {
        Name = name;
        Width = width;
        ColumnFlags = columnFlags;
        AllTabOnly = allTabOnly;
    }

    public string Name { get; private set; }
    public uint Width { get; private set; }
    public ImGuiTableColumnFlags ColumnFlags { get; private set; }
    public Func<ImGuiSortDirection?, IEnumerable<T>, IEnumerable<T>>? Sort { get; set; }
    public Func<T, bool>? OnLeftClick { get; set; }
    
    public Func<string?, IEnumerable<T>, IEnumerable<T>>? Filter { get; set; }
    public Func<bool?, IEnumerable<T>, IEnumerable<T>>? FilterBool { get; set; }
    
    public Action<T, uint> Draw { get; set; }
    public string FilterText = "";
    public bool? FilterBoolean = null;
    public bool AllTabOnly;

}
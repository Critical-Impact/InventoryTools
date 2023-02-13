using System;
using System.Collections;
using System.Collections.Generic;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

namespace InventoryTools.Ui;

public class TableColumn<T>
{
    public TableColumn(string name, uint width, ImGuiTableColumnFlags columnFlags)
    {
        Name = name;
        Width = width;
        ColumnFlags = columnFlags;
    }

    public string Name { get; private set; }
    public uint Width { get; private set; }
    public ImGuiTableColumnFlags ColumnFlags { get; private set; }
    
    public Func<ImGuiSortDirection?, IEnumerable<T>, IEnumerable<T>>? Sort { get; set; }
    public Func<T, bool>? OnLeftClick { get; set; }
    
}
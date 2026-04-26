using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllaganLib.Data.Service;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Logic.Settings;

namespace InventoryTools.Compendium.Models;

public sealed class CompendiumTable<TData> : RenderTable<WindowState, TData, MessageBase>, ICompendiumTable<WindowState, MessageBase>
{
    private readonly ICompendiumType<TData> _compendiumType;
    private readonly CompendiumRowHeightSetting _compendiumRowHeightSetting;
    private readonly InventoryToolsConfiguration _configuration;

    public delegate CompendiumTable<TData> Factory(CompendiumTableOptions<TData> tableOptions);

    private ICompendiumGrouping<TData>? _grouping;
    private object? _groupingGroup;
    private readonly Func<(ICompendiumGrouping<TData>, object?)?, List<IColumn<WindowState, TData, MessageBase>>>? _columnGenerator;

    public CompendiumTable(CompendiumTableOptions<TData> tableOptions, CompendiumRowHeightSetting compendiumRowHeightSetting, InventoryToolsConfiguration configuration, CsvLoaderService csvLoaderService, WindowState searchFilter) : base(csvLoaderService, searchFilter, tableOptions.Columns.Invoke(null), tableOptions.TableFlags, tableOptions.Name, "ct_" + tableOptions.Key)
    {
        _compendiumType = tableOptions.CompendiumType;
        _columnGenerator = tableOptions.Columns;
        _compendiumRowHeightSetting = compendiumRowHeightSetting;
        _configuration = configuration;
        ShowFilterRow = true;
        ResizeOnOpen = true;
        FreezeRows = 2;
        if (tableOptions.BuildContextMenu != null)
        {
            RightClickFunc = tableOptions.BuildContextMenu;
        }
    }

    public override List<TData> GetItems()
    {
        if (_grouping != null)
        {
            return _compendiumType.GetRows().Where(c => _groupingGroup != null && _grouping.GroupFunc(c).Equals(_groupingGroup)).ToList();
        }
        return _compendiumType.GetRows();
    }

    public override int RowHeight => _compendiumRowHeightSetting.CurrentValue(_configuration);

    public void SetGrouping(ICompendiumGrouping grouping, object group)
    {
        if (grouping is ICompendiumGrouping<TData> compendiumGrouping)
        {
            _grouping = compendiumGrouping;
            _groupingGroup = group;
            if (this._columnGenerator != null)
            {
                this.Columns = this._columnGenerator((compendiumGrouping, group));
            }
            this.IsDirty = true;
        }
    }
    public void ClearGrouping()
    {
        _grouping = null;
        _groupingGroup = null;
        if (this._columnGenerator != null)
        {
            this.Columns = this._columnGenerator(null);
        }

        this.IsDirty = true;
    }
}
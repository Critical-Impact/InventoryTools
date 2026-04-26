using System.Collections.Generic;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Columns;
using InventoryTools.Compendium.Columns.Options;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Models;

public sealed class CompendiumColumnBuilder<TData>
{
    private readonly List<IColumn<WindowState, TData, MessageBase>> _columns = new();

    private readonly ICompendiumGrouping<TData>? _compendiumGrouping;
    private readonly GenericStringTableColumn<TData>.Factory _stringColumnFactory;
    private readonly GenericIntegerTableColumn<TData>.Factory _integerColumnFactory;
    private readonly GenericBooleanTableColumn<TData>.Factory _booleanColumnFactory;
    private readonly GenericIconTableColumn<TData>.Factory _iconColumnFactory;
    private readonly GenericItemSourcesTableColumn<TData>.Factory _itemSourcesTableColumnFactory;
    private readonly GenericItemsTableColumn<TData>.Factory _itemsTableColumnFactory;
    private readonly GenericItemTableColumn<TData>.Factory _itemTableColumnFactory;
    private readonly OpenViewTableColumn<TData>.Factory _compendiumOpenViewTableColumnFactory;
    private readonly object? _groupItem;

    public delegate CompendiumColumnBuilder<TData> Factory(ICompendiumGrouping<TData>? compendiumGrouping, object? groupItem);

    public CompendiumColumnBuilder(
        ICompendiumGrouping<TData>? compendiumGrouping,
        GenericStringTableColumn<TData>.Factory stringColumnFactory,
        GenericIntegerTableColumn<TData>.Factory integerColumnFactory,
        GenericBooleanTableColumn<TData>.Factory booleanColumnFactory,
        GenericIconTableColumn<TData>.Factory iconColumnFactory,
        GenericItemSourcesTableColumn<TData>.Factory itemSourcesTableColumnFactory,
        GenericItemsTableColumn<TData>.Factory itemsTableColumnFactory,
        GenericItemTableColumn<TData>.Factory itemTableColumnFactory,
        OpenViewTableColumn<TData>.Factory compendiumOpenViewTableColumnFactory,
        object? groupItem)
    {
        _groupItem = groupItem;
        _compendiumGrouping = compendiumGrouping;
        _stringColumnFactory = stringColumnFactory;
        _integerColumnFactory = integerColumnFactory;
        _booleanColumnFactory = booleanColumnFactory;
        _iconColumnFactory = iconColumnFactory;
        _itemSourcesTableColumnFactory = itemSourcesTableColumnFactory;
        _itemsTableColumnFactory = itemsTableColumnFactory;
        _itemTableColumnFactory = itemTableColumnFactory;
        _compendiumOpenViewTableColumnFactory = compendiumOpenViewTableColumnFactory;
    }

    public CompendiumColumnBuilder<TData> AddStringColumn(StringColumnOptions<TData> options)
    {
        var column = _stringColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemSourcesColumn(ItemSourceColumnOptions<TData> options)
    {
        var column = _itemSourcesTableColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemsColumn(ItemsColumnOptions<TData> options)
    {
        var column = _itemsTableColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddIntegerColumn(IntegerColumnOptions<TData> options)
    {
        var column = _integerColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddBooleanColumn(BooleanColumnOptions<TData> options)
    {
        var column = _booleanColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddIconColumn(IconColumnOptions<TData> options)
    {
        var column = _iconColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddImageIconColumn(IconColumnOptions<TData> options)
    {
        var column = _iconColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddCompendiumOpenViewColumn(OpenViewTableColumnOptions<TData> options)
    {
        var column = _compendiumOpenViewTableColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemColumn(ItemColumnOptions<TData> options)
    {
        var column = _itemTableColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public List<IColumn<WindowState, TData, MessageBase>> Columns => _columns;
    public ICompendiumGrouping? CompendiumGrouping => _compendiumGrouping;
    public object? GroupItem => _groupItem;
}
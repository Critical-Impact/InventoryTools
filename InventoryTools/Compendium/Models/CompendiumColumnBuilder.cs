using System.Collections.Generic;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Columns;

namespace InventoryTools.Compendium.Models;

public sealed class CompendiumColumnBuilder<TData>
{
    private readonly List<IColumn<WindowState, TData, MessageBase>> _columns = new();

    private readonly GenericStringTableColumn<TData>.Factory _stringColumnFactory;
    private readonly GenericIntegerTableColumn<TData>.Factory _integerColumnFactory;
    private readonly GenericBooleanTableColumn<TData>.Factory _booleanColumnFactory;
    private readonly GenericIconTableColumn<TData>.Factory _iconColumnFactory;
    private readonly GenericItemSourcesTableColumn<TData>.Factory _itemSourcesTableColumn;
    private readonly GenericItemsTableColumn<TData>.Factory _itemsTableColumn;
    private readonly GenericItemTableColumn<TData>.Factory _itemTableColumn;

    public CompendiumColumnBuilder(
        GenericStringTableColumn<TData>.Factory stringColumnFactory,
        GenericIntegerTableColumn<TData>.Factory integerColumnFactory,
        GenericBooleanTableColumn<TData>.Factory booleanColumnFactory,
        GenericIconTableColumn<TData>.Factory iconColumnFactory,
        GenericItemSourcesTableColumn<TData>.Factory  itemSourcesTableColumn,
        GenericItemsTableColumn<TData>.Factory  itemsTableColumn,
        GenericItemTableColumn<TData>.Factory  itemTableColumn)
    {
        _stringColumnFactory = stringColumnFactory;
        _integerColumnFactory = integerColumnFactory;
        _booleanColumnFactory = booleanColumnFactory;
        _iconColumnFactory = iconColumnFactory;
        _itemSourcesTableColumn = itemSourcesTableColumn;
        _itemsTableColumn = itemsTableColumn;
        _itemTableColumn = itemTableColumn;
    }

    public CompendiumColumnBuilder<TData> AddStringColumn(CompendiumStringColumnOptions<TData> options)
    {
        var column = _stringColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemSourcesColumn(CompendiumItemSourceColumnOptions<TData> options)
    {
        var column = _itemSourcesTableColumn(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemsColumn(CompendiumItemsColumnOptions<TData> options)
    {
        var column = _itemsTableColumn(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddIntegerColumn(CompendiumIntegerColumnOptions<TData> options)
    {
        var column = _integerColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddBooleanColumn(CompendiumBooleanColumnOptions<TData> options)
    {
        var column = _booleanColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddIconColumn(CompendiumIconColumnOptions<TData>  options)
    {
        var column = _iconColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemColumn(CompendiumItemColumnOptions<TData>  options)
    {
        var column = _itemTableColumn(options);
        _columns.Add(column);
        return this;
    }

    public List<IColumn<WindowState, TData, MessageBase>> Columns => _columns;
}
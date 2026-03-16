using System.Collections.Generic;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Columns;
using InventoryTools.Compendium.Columns.Options;

namespace InventoryTools.Compendium.Models;

public sealed class CompendiumColumnBuilder<TData>
{
    private readonly List<IColumn<WindowState, TData, MessageBase>> _columns = new();

    private readonly GenericStringTableColumn<TData>.Factory _stringColumnFactory;
    private readonly GenericIntegerTableColumn<TData>.Factory _integerColumnFactory;
    private readonly GenericBooleanTableColumn<TData>.Factory _booleanColumnFactory;
    private readonly GenericIconTableColumn<TData>.Factory _iconColumnFactory;
    private readonly GenericItemSourcesTableColumn<TData>.Factory _itemSourcesTableColumnFactory;
    private readonly GenericItemsTableColumn<TData>.Factory _itemsTableColumnFactory;
    private readonly GenericItemTableColumn<TData>.Factory _itemTableColumnFactory;
    private readonly OpenViewTableColumn<TData>.Factory _compendiumOpenViewTableColumnFactory;

    public CompendiumColumnBuilder(
        GenericStringTableColumn<TData>.Factory stringColumnFactory,
        GenericIntegerTableColumn<TData>.Factory integerColumnFactory,
        GenericBooleanTableColumn<TData>.Factory booleanColumnFactory,
        GenericIconTableColumn<TData>.Factory iconColumnFactory,
        GenericItemSourcesTableColumn<TData>.Factory  itemSourcesTableColumnFactory,
        GenericItemsTableColumn<TData>.Factory  itemsTableColumnFactory,
        GenericItemTableColumn<TData>.Factory  itemTableColumnFactory,
        OpenViewTableColumn<TData>.Factory compendiumOpenViewTableColumnFactory)
    {
        _stringColumnFactory = stringColumnFactory;
        _integerColumnFactory = integerColumnFactory;
        _booleanColumnFactory = booleanColumnFactory;
        _iconColumnFactory = iconColumnFactory;
        _itemSourcesTableColumnFactory = itemSourcesTableColumnFactory;
        _itemsTableColumnFactory = itemsTableColumnFactory;
        _itemTableColumnFactory = itemTableColumnFactory;
        _compendiumOpenViewTableColumnFactory = compendiumOpenViewTableColumnFactory;
    }

    public CompendiumColumnBuilder<TData> AddStringColumn(CompendiumStringColumnOptions<TData> options)
    {
        var column = _stringColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemSourcesColumn(CompendiumItemSourceColumnOptions<TData> options)
    {
        var column = _itemSourcesTableColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public CompendiumColumnBuilder<TData> AddItemsColumn(CompendiumItemsColumnOptions<TData> options)
    {
        var column = _itemsTableColumnFactory(options);
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

    public CompendiumColumnBuilder<TData> AddImageIconColumn(CompendiumIconColumnOptions<TData>  options)
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

    public CompendiumColumnBuilder<TData> AddItemColumn(CompendiumItemColumnOptions<TData>  options)
    {
        var column = _itemTableColumnFactory(options);
        _columns.Add(column);
        return this;
    }

    public List<IColumn<WindowState, TData, MessageBase>> Columns => _columns;
}
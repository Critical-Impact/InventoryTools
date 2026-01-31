using System;
using System.Collections.Generic;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;

namespace InventoryTools.Compendium.Models;

public abstract class CompendiumType<TData> : ICompendiumType<TData>
{
    private readonly Func<CompendiumColumnBuilder<TData>> _columnBuilder;
    public CompendiumTable<TData>.Factory Factory { get; }

    public Func<CompendiumColumnBuilder<TData>> ColumnBuilder => _columnBuilder;

    public CompendiumType(CompendiumTable<TData>.Factory tableFactory, Func<CompendiumColumnBuilder<TData>> columnBuilder)
    {
        _columnBuilder = columnBuilder;
        Factory = tableFactory;
    }

    public abstract string Singular { get; }
    public abstract string Plural { get; }

    public abstract IRenderTable<WindowState, MessageBase> BuildTable();
    public abstract TData? GetRow(uint row);
    public abstract List<TData> GetRows();
    public abstract void BuildColumns(CompendiumColumnBuilder<TData> builder);

    public virtual Func<TData, List<MessageBase>>? BuildContextMenu() => null;

    public List<IColumn<WindowState, TData, MessageBase>> BuiltColumns()
    {
        var builder = _columnBuilder.Invoke();
        BuildColumns(builder);
        return builder.Columns;
    }
}
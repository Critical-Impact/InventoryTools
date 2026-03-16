using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Services;

namespace InventoryTools.Compendium.Models;

public abstract class CompendiumType<TData> : ICompendiumType<TData>
{
    private readonly Func<CompendiumColumnBuilder<TData>> _columnBuilder;
    private readonly CompendiumViewBuilder.Factory _viewBuilderFactory;
    public CompendiumTable<TData>.Factory Factory { get; }

    public Func<CompendiumColumnBuilder<TData>> ColumnBuilder => _columnBuilder;

    public CompendiumType(CompendiumTable<TData>.Factory tableFactory, Func<CompendiumColumnBuilder<TData>> columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory)
    {
        _columnBuilder = columnBuilder;
        _viewBuilderFactory = viewBuilderFactory;
        Factory = tableFactory;
    }

    public abstract string Singular { get; }
    public abstract string Plural { get; }
    public abstract string Description { get; }
    public abstract string Key { get; }
    public abstract (string?, uint?) Icon { get; }
    public Type Type => typeof(TData);
    public virtual List<Type>? RelatedTypes { get; } = null;

    public string? GetName(uint rowId)
    {
        var row = GetRow(rowId);
        if (object.Equals(row, default(TData)))
        {
            return null;
        }

        return GetName(row!);
    }

    public string? GetSubtitle(uint rowId)
    {
        var row = GetRow(rowId);
        if (object.Equals(row, default(TData)))
        {
            return null;
        }

        return GetSubtitle(row!);
    }

    public object? GetObject(uint row)
    {
        return GetRow(row);
    }

    public (string?, uint?) GetIcon(uint rowId)
    {
        var row = GetRow(rowId);
        if (object.Equals(row, default(TData)))
        {
            return (null, null);
        }

        return GetIcon(row!);
    }

    public CompendiumViewBuilder? BuildView(uint rowId)
    {
        var row = GetRow(rowId);
        if (object.Equals(row, default(TData)))
        {
            return null;
        }
        var viewBuilder = _viewBuilderFactory.Invoke(this);
        BuildViewFields(viewBuilder, row!);
        return viewBuilder;
    }

    public abstract ICompendiumTable<WindowState, MessageBase> BuildTable();
    public abstract string? GetName(TData row);
    public abstract string? GetSubtitle(TData row);
    public abstract (string?, uint?) GetIcon(TData row);
    public abstract TData? GetRow(uint row);
    public abstract bool HasRow(uint rowId);
    public abstract List<TData> GetRows();
    public abstract void BuildColumns(CompendiumColumnBuilder<TData> builder);
    public abstract void BuildViewFields(CompendiumViewBuilder viewBuilder, TData row);

    public virtual Func<TData, List<MessageBase>>? BuildContextMenu() => null;
    public Dictionary<object, string>? GetGroups(ICompendiumGrouping<TData> compendiumGrouping)
    {
        return GetRows().Select(c => compendiumGrouping.GroupFunc(c)).Distinct().ToDictionary(c => c, c => compendiumGrouping.GroupMapping(c));
    }

    public virtual List<ICompendiumGrouping>? GetGroupings()
    {
        return [];
    }

    public virtual string? GetDefaultGrouping()
    {
        return null;
    }

    public virtual bool ShowInListing => true;
    public virtual Type? ViewRedirection => null;

    public Dictionary<object, string>? GetGroups(ICompendiumGrouping compendiumGrouping)
    {
        if (compendiumGrouping is ICompendiumGrouping<TData> compendiumGroupingData)
        {
            return GetGroups(compendiumGroupingData);
        }

        return null;
    }

    public List<IColumn<WindowState, TData, MessageBase>> BuiltColumns()
    {
        var builder = _columnBuilder.Invoke();
        BuildColumns(builder);
        return builder.Columns;
    }
}
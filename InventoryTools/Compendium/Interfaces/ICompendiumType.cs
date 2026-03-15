using System;
using System.Collections.Generic;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;

namespace InventoryTools.Compendium.Interfaces;

public interface ICompendiumType<TData> : ICompendiumType
{
    public string? GetName(TData row);
    public string? GetSubtitle(TData row);
    public (string?, uint?) GetIcon(TData row);
    public TData? GetRow(uint row);
    public List<TData> GetRows();
    public void BuildColumns(CompendiumColumnBuilder<TData> builder);
    public void BuildViewFields(CompendiumViewBuilder viewBuilder, TData row);
    public Func<TData, List<MessageBase>>? BuildContextMenu();
    public Dictionary<object, string>? GetGroups(ICompendiumGrouping<TData> compendiumGrouping);
}

public interface ICompendiumType
{
    public bool HasRow(uint rowId);
    public string? GetName(uint rowId);
    public string? GetSubtitle(uint rowId);
    public object? GetObject(uint row);
    public (string?, uint?) GetIcon(uint rowId);
    public CompendiumViewBuilder? BuildView(uint rowId);
    public ICompendiumTable<WindowState, MessageBase> BuildTable();
    public string Singular { get; }
    public string Plural { get; }
    public string Description { get; }
    public string Key { get; }
    public (string?, uint?) Icon { get; }
    public Type Type { get; }
    public List<Type>? RelatedTypes { get; }
    public List<ICompendiumGrouping>? GetGroupings();
    public Dictionary<object, string>? GetGroups(ICompendiumGrouping compendiumGrouping);
    public string? GetDefaultGrouping();
    public bool ShowInListing { get; }
    public Type? ViewRedirection { get; }
}
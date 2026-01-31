using System;
using System.Collections.Generic;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Interfaces;

public interface ICompendiumType<TData> : ICompendiumType
{
    public TData? GetRow(uint row);
    public List<TData> GetRows();
    public void BuildColumns(CompendiumColumnBuilder<TData> builder);
    public Func<TData, List<MessageBase>>? BuildContextMenu();
}

public interface ICompendiumType
{
    public IRenderTable<WindowState, MessageBase> BuildTable();
    public string Singular { get; }
    public string Plural { get; }
}
using System.ComponentModel;
using AllaganLib.Interface.Grid;

namespace InventoryTools.Compendium.Interfaces;

public interface ICompendiumTable<WindowState, MessageBase> : ICompendiumTable, IRenderTable<WindowState, MessageBase> where WindowState : INotifyPropertyChanged
{
    public void SetGrouping(ICompendiumGrouping grouping, object group);
    public void ClearGrouping();
}

public interface ICompendiumTable
{
    public void SetGrouping(ICompendiumGrouping grouping, object group);
    public void ClearGrouping();
}
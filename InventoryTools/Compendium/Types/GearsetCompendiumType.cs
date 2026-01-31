using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.Interface.Grid;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Services;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Compendium.Types;

public class GearsetCompendiumType : CompendiumType<Gearset>, IItems
{
    private readonly List<Gearset> _gearsets;
    private readonly ItemSheet _itemSheet;
    private readonly ItemInfoCache _itemInfoCache;
    private readonly CompendiumMenuBuilder _menuBuilder;

    public GearsetCompendiumType(CompendiumTable<Gearset>.Factory tableFactory, Func<CompendiumColumnBuilder<Gearset>> columnBuilder, List<Gearset> gearsets, ItemSheet itemSheet, ItemInfoCache itemInfoCache, CompendiumMenuBuilder menuBuilder) : base(tableFactory, columnBuilder)
    {
        _gearsets = gearsets;
        _itemSheet = itemSheet;
        _itemInfoCache = itemInfoCache;
        _menuBuilder = menuBuilder;
    }

    public override IRenderTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "gearsets",
            Name = Plural,
            Columns = BuiltColumns(),
            CompendiumType = this,
            BuildContextMenu = BuildContextMenu
        });
    }

    private List<MessageBase> BuildContextMenu(Gearset arg)
    {
        _menuBuilder.Header(arg.Name);
        _menuBuilder.TryOn(arg.Items, "Try on Gearset");
        _menuBuilder.NewLine();
        _menuBuilder.Header("Gear Pieces");
        _menuBuilder.Items(arg.Items);
        _menuBuilder.GroupedItems(arg.Items, "All Gear Pieces");
        return [];
    }

    public override string Singular => "Gearset";
    public override string Plural => "Gearsets";



    public override Gearset GetRow(uint row)
    {
        return _gearsets[(int)row];
    }

    public override List<Gearset> GetRows()
    {
        return _gearsets;
    }

    public override void BuildColumns(CompendiumColumnBuilder<Gearset> builder)
    {
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the gearset", Version = "14.0.3", ValueSelector = row => row.Name});
        builder.AddItemSourcesColumn(new() { Key = "sources", Name = "Sources", HelpText = "The combined sources for the gearset.", Version = "14.0.3", ValueSelector = gearset => gearset.Items.Where(c => c.RowId != 0).SelectMany(c => _itemInfoCache.GetItemSources(c.RowId) ?? []).ToList()});
        builder.AddStringColumn(new (){Key = "patch", Name = "Patch", HelpText = "The patch the gearset was added.", Version = "14.0.3", ValueSelector = gearset => string.Join(", ", gearset.Items.Where(c => c.RowId != 0).Select(c => _itemSheet.GetRow(c.RowId).Patch.ToString(CultureInfo.InvariantCulture)).Distinct())});
        for (int i = 0; i < 12; i++)
        {
            var index = i;
            builder.AddItemColumn(new(){Key = "item" + i, Name = "Item " + (i + 1), HelpText = "The item", Version = "14.0.3", ValueSelector = row => row.Items[index].RowId});
        }
    }

    public List<ItemInfo>? GetItems(uint rowId)
    {
        return GetRow(rowId).Items.Where(c => c.RowId != 0).Select(c => ItemInfo.Create(_itemSheet.GetRow(c.RowId))).ToList();
    }

    public bool AllowTryOn => true;
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using CriticalCommonLib.Models;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Services;
using LuminaSupplemental.Excel.Model;

namespace InventoryTools.Compendium.Types;

public class GearsetCompendiumType : CompendiumType<Gearset>
{
    private readonly ItemListSection.Factory _itemListSectionFactory;
    private readonly List<Gearset> _gearsets;
    private readonly ItemSheet _itemSheet;
    private readonly ItemInfoCache _itemInfoCache;
    private readonly CompendiumMenuBuilder _menuBuilder;

    public GearsetCompendiumType(CompendiumTable<Gearset>.Factory tableFactory,
        CompendiumColumnBuilder<Gearset>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory,
        ItemListSection.Factory  itemListSectionFactory,
        List<Gearset> gearsets,
        ItemSheet itemSheet,
        ItemInfoCache itemInfoCache,
        CompendiumMenuBuilder menuBuilder) : base(tableFactory,
        columnBuilder,
        viewBuilderFactory)
    {
        _itemListSectionFactory = itemListSectionFactory;
        _gearsets = gearsets;
        _itemSheet = itemSheet;
        _itemInfoCache = itemInfoCache;
        _menuBuilder = menuBuilder;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "gearsets",
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
            BuildContextMenu = BuildContextMenu
        });
    }

    public override string? GetName(Gearset row)
    {
        return row.Name;
    }

    public override string? GetSubtitle(Gearset row)
    {
        return row.Items.Count + " items";
    }

    public override (string?, uint?) GetIcon(Gearset row)
    {
        return (null, Icons.ArmorIcon);
    }

    public override uint GetRowId(Gearset row)
    {
        return (uint)GetRows().IndexOf(row);
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
    public override string Description => "Gearsets based on Eorzea Collection's organizing.";
    public override string Key => "gearsets";
    public override (string?, uint?) Icon => (null, Icons.ArmorIcon);


    public override Gearset GetRow(uint row)
    {
        return _gearsets[(int)row];
    }

    public override bool HasRow(uint rowId)
    {
        return (int)rowId >= 0 && (int)rowId < _gearsets.Count;
    }

    public override List<Gearset> GetRows()
    {
        return _gearsets;
    }

    public override void BuildColumns(CompendiumColumnBuilder<Gearset> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "##Icon", HelpText = "The icon of the gearset", Version = "14.0.3", ValueSelector = row => (null, Icons.ArmorIcon), CompendiumType = this, RowIdSelector = row => (uint)_gearsets.IndexOf(row)});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the gearset", Version = "14.0.3", ValueSelector = row => row.Name});
        builder.AddItemSourcesColumn(new() { Key = "sources", Name = "Sources", HelpText = "The combined sources for the gearset.", Version = "14.0.3", ValueSelector = gearset => gearset.Items.Where(c => c.RowId != 0).SelectMany(c => _itemInfoCache.GetItemSources(c.RowId) ?? []).ToList()});
        builder.AddStringColumn(new (){Key = "patch", Name = "Patch", HelpText = "The patch the gearset was added.", Version = "14.0.3", ValueSelector = gearset => string.Join(", ", gearset.Items.Where(c => c.RowId != 0).Select(c => _itemSheet.GetRow(c.RowId).Patch.ToString(CultureInfo.InvariantCulture)).Distinct())});
        for (int i = 0; i < 12; i++)
        {
            var index = i;
            builder.AddItemColumn(new(){Key = "item" + i, Name = "Item " + (i + 1), HelpText = "The item", Version = "14.0.3", ValueSelector = row => row.Items[index].RowId});
        }
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, Gearset row)
    {
        var itemCount = row.Items.Count(c => c.RowId != 0);
        viewBuilder.Title = row.Name;
        viewBuilder.Icon = Icons.ArmorIcon;
        viewBuilder.Subtitle = itemCount + " " + (row.Items.Count == 1 ? "item" : "items");
        viewBuilder.AddLink("https://ffxiv.eorzeacollection.com/gearset/" + row.Key, "Open in Eorzea Collection", "ec");
        viewBuilder.AddSection(_itemListSectionFactory.Invoke(new(){SectionName = "Set Items", Items = row.Items.Where(c => c.RowId != 0).Select(c => ItemInfo.Create(_itemSheet.GetRow(c.RowId)))}));
    }
}
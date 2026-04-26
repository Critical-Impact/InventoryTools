using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Dalamud.Utility;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Localizers;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class ENpcCompendiumType : CompendiumType<IGrouping<string, ENpcBaseRow>>
{
    private readonly ENpcBaseSheet _eNpcBaseSheet;
    private readonly ILocalizer<ENpcBase> _npcLocalizer;
    private readonly ItemInfoCache _itemInfoCache;
    private List<IGrouping<string, ENpcBaseRow>>? _groupedRows;

    public ENpcCompendiumType(ENpcBaseSheet eNpcBaseSheet, ILocalizer<ENpcBase> npcLocalizer, ItemInfoCache itemInfoCache, CompendiumTable<IGrouping<string, ENpcBaseRow>>.Factory tableFactory, CompendiumColumnBuilder<IGrouping<string, ENpcBaseRow>>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _eNpcBaseSheet = eNpcBaseSheet;
        _npcLocalizer = npcLocalizer;
        _itemInfoCache = itemInfoCache;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<IGrouping<string, ENpcBaseRow>>()
        {
            Key = "npcs",
            Columns = BuiltColumns,
            CompendiumType = this,
            Name = "NPCs",
        });
    }

    public override string? GetName(IGrouping<string, ENpcBaseRow> row)
    {
        return _npcLocalizer.Format(row.First().Base);
    }

    public override string? GetSubtitle(IGrouping<string, ENpcBaseRow> row)
    {
        return null;
    }

    public override (string?, uint?) GetIcon(IGrouping<string, ENpcBaseRow> row)
    {
        return (null, Icons.ThreePeople);
    }

    public override uint GetRowId(IGrouping<string, ENpcBaseRow> row)
    {
        return (uint)GetRows().IndexOf(row);
    }

    public override IGrouping<string, ENpcBaseRow>? GetRow(uint row)
    {
        return this.GetRows()[(int)row];
    }

    public override List<IGrouping<string, ENpcBaseRow>> GetRows()
    {
        return _groupedRows ??= _eNpcBaseSheet.Where(c => c.Name != "").GroupBy(c => c.Name).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<IGrouping<string, ENpcBaseRow>> builder)
    {
        builder.AddCompendiumOpenViewColumn(new() { Key = "icon", Name = "##Icon", HelpText = "The icon of the npc", Version = "14.0.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.FirstOrDefault()!.RowId });
        builder.AddStringColumn(new() { Key = "name", Name = "Name", HelpText = "The name of the npc", Version = "14.0.3", ValueSelector = GetName });
        builder.AddBooleanColumn(new() { Key = "is_vendor", Name = "Is Vendor?", HelpText = "Is the NPC a vendor?", Version = "14.0.3", ValueSelector = row => row.Any(c => c.IsVendor) });
        builder.AddBooleanColumn(new() { Key = "is_calamity_salvager", Name = "Is Calamity Salvager?", HelpText = "Is the NPC a calamity salvager?", Version = "14.0.3", ValueSelector = row => row.Any(c => c.IsCalamitySalvager) });
        builder.AddBooleanColumn(new() { Key = "is_house_vendor", Name = "Is Housing Vendor?", HelpText = "Is the NPC a housing vendor?", Version = "14.0.3", ValueSelector = row => row.Any(c => c.IsHouseVendor) });
        builder.AddItemsColumn(new() { Key = "vendor_items", Name = "Vendor Items", HelpText = "The items this vendor sells?", Version = "14.0.3", ValueSelector = row => GetShopItems(row.FirstOrDefault()!.ENpcResidentRow) ?? [] });
    }

    private List<ItemRow>? GetShopItems(ENpcResidentRow npc)
    {

        var npcShops = _itemInfoCache.GetNpcShops(npc.RowId);
        if (npcShops != null)
        {
            IEnumerable<ItemRow> items = new List<ItemRow>();
            foreach (var shop in npcShops)
            {
                items = items.Concat(shop.Items);
            }
            var shopItems = items.ToList();
            return shopItems;
        }
        return null;
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, IGrouping<string, ENpcBaseRow> row)
    {
        viewBuilder.SetupDefaults(this, row);
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = row.SelectMany(c => c.Base.ENpcData).DistinctBy(c => c.RowId).ToList(),
            Filter = typeof(Quest),
            SectionName = "Related Quests"
        });
        var mapLinks = row.SelectMany(c => c.Locations).Select(c => new MapLinkEntry(Icons.FlagIcon, c.FormattedName, "", c)).ToList();
        viewBuilder.AddMapLinksSectionSection(new MapLinksViewSectionOptions()
        {
            MapLinks = mapLinks,
            SectionName = "Known Locations"
        });

    }

    public override bool HasRow(uint rowId)
    {
        var rows = this.GetRows();
        if ((int)rowId >= 0 && (int)rowId < rows.Count)
        {
            return true;
        }

        return false;
    }

    public override bool HasLocation => true;

    public override ILocation? GetLocation(IGrouping<string, ENpcBaseRow> row)
    {
        return row.SelectMany(c => c.Locations).FirstOrDefault();
    }

    public override uint? RemapType(Type type, uint rowId)
    {
        if (type == typeof(ENpcBaseRow) || type == typeof(ENpcBase) || type == typeof(ENpcResident) || type == typeof(ENpcResidentRow))
        {
            var cs = GetRows();
            for (var index = 0; index < cs.Count; index++)
            {
                var c = cs[index];
                if (c.Any(d => d.RowId == rowId))
                {
                    return (uint?)index;
                }
            }
        }

        return null;
    }

    public override List<Type>? RelatedTypes => [typeof(ENpcResidentRow), typeof(ENpcResident), typeof(ENpcBase)];

    public override string Singular => "NPC";
    public override string Plural => "NPCs";
    public override string Description => "A list of all the NPCs in the game";
    public override string Key => "npcs";
    public override (string?, uint?) Icon => (null, Icons.ThreePeople);
}
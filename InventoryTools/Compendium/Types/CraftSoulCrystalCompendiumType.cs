using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using AllaganLib.GameSheets.Caches;
using Lumina.Excel;
using AllaganLib.GameSheets.ItemSources;
using AllaganLib.GameSheets.Model;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using Dalamud.Utility;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using InventoryTools.Ui;

namespace InventoryTools.Compendium.Types;

public class CraftSoulCrystalCompendiumType : CompendiumType<ItemCraftSoulCrystalUse>
{
    private readonly ItemInfoCache _itemInfoCache;
    private readonly Lazy<List<ItemCraftSoulCrystalUse>> _rows;
    private readonly Lazy<Dictionary<uint, ItemCraftSoulCrystalUse>> _rowsById;
    private readonly Lazy<(string?, uint?)> _staticIcon;

    public CraftSoulCrystalCompendiumType(
        ItemInfoCache itemInfoCache,
        CompendiumTable<ItemCraftSoulCrystalUse>.Factory tableFactory,
        CompendiumColumnBuilder<ItemCraftSoulCrystalUse>.Factory columnBuilder,
        CompendiumViewBuilder.Factory viewBuilderFactory)
        : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _itemInfoCache = itemInfoCache;
        _rows = new Lazy<List<ItemCraftSoulCrystalUse>>(BuildRows, LazyThreadSafetyMode.ExecutionAndPublication);
        _rowsById = new Lazy<Dictionary<uint, ItemCraftSoulCrystalUse>>(
            () => _rows.Value.ToDictionary(r => r.Item.RowId),
            LazyThreadSafetyMode.ExecutionAndPublication);
        _staticIcon = new Lazy<(string?, uint?)>(BuildStaticIcon, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public override string Singular => "Craft Soul Crystal";
    public override string Plural => "Craft Soul Crystals";
    public override string Description => "Soul crystals that allow specialization in a crafting class.";
    public override string Key => "craft_soul_crystals";
    public override (string?, uint?) Icon => _staticIcon.Value;

    public override ICompendiumTable<WindowState, MessageBase> BuildTable() =>
        Factory.Invoke(new() { Key = Key, Name = Plural, Columns = BuiltColumns, CompendiumType = this });

    public override string? GetName(ItemCraftSoulCrystalUse row) => row.Item.NameString;

    public override string? GetSubtitle(ItemCraftSoulCrystalUse row) =>
        row.ClassJob.Base.Name.ToImGuiString();

    public override (string?, uint?) GetIcon(ItemCraftSoulCrystalUse row) =>
        (null, row.Item.Icon);

    public override uint GetRowId(ItemCraftSoulCrystalUse row) => row.Item.RowId;

    public override ItemCraftSoulCrystalUse? GetRow(uint rowId) =>
        _rowsById.Value.GetValueOrDefault(rowId);

    public override bool HasRow(uint rowId) => _rowsById.Value.ContainsKey(rowId);

    public override List<ItemCraftSoulCrystalUse> GetRows() => _rows.Value;

    public override void BuildColumns(CompendiumColumnBuilder<ItemCraftSoulCrystalUse> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "##Icon",
            HelpText = "The icon of the soul crystal",
            Version = "15.0.6",
            ValueSelector = GetIcon,
            CompendiumType = this,
            RowIdSelector = row => row.Item.RowId,
        });
        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the soul crystal",
            Version = "15.0.6",
            ValueSelector = row => row.Item.NameString,
        });
        builder.AddStringColumn(new()
        {
            Key = "class",
            Name = "Class",
            HelpText = "The crafting class this soul crystal specializes",
            Version = "15.0.6",
            ValueSelector = row => row.ClassJob.Base.Name.ToImGuiString().FirstCharToUpper(),
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, ItemCraftSoulCrystalUse row)
    {
        viewBuilder.SetupDefaults(this, row);

        viewBuilder.AddInfoTableSection(new InfoTableSectionOptions()
        {
            SectionKey = "info",
            SectionName = "Info",
            Items =
            [
                ("Class", row.ClassJob.Base.Name.ToImGuiString(), true),
            ],
        });

        viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
        {
            SectionKey = "class_job",
            SectionName = "Class",
            RelatedRef = (RowRef)row.ClassJob.RowRef,
        });

        viewBuilder.AddItemListSection(new ItemListSectionOptions()
        {
            SectionKey = "soul_crystal_item",
            SectionName = "Soul Crystal",
            Items = [new ItemInfo(row.Item)],
        });

        var itemSources = _itemInfoCache.GetItemSources(row.Item.RowId);
        viewBuilder.AddItemSourcesSection(new ItemSourcesSectionOptions()
        {
            SectionKey = "soul_crystal_sources",
            SectionName = "Obtain",
            Sources = itemSources ?? [],
            SourceType = SourceType.Source,
        });
    }

    private List<ItemCraftSoulCrystalUse> BuildRows() =>
        _itemInfoCache.GetItemUsesByType<ItemCraftSoulCrystalUse>(ItemInfoType.CraftSoulCrystal)
            .OrderBy(r => r.ClassJob.Base.Name.ToImGuiString())
            .ToList();

    private (string?, uint?) BuildStaticIcon()
    {
        var first = _rows.Value.FirstOrDefault();
        if (first != null && first.Item.Icon != 0)
        {
            return (null, first.Item.Icon);
        }

        return (null, null);
    }
}
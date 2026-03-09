using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class ItemCompendiumType : CompendiumType<ItemRow>
{
    private readonly ItemSheet _itemSheet;

    public ItemCompendiumType(ItemSheet itemSheet, CompendiumTable<ItemRow>.Factory tableFactory, Func<CompendiumColumnBuilder<ItemRow>> columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _itemSheet = itemSheet;
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<ItemRow>()
        {
            Columns = BuiltColumns(),
            CompendiumType = this,
            Key = "items",
            Name = "Items"
        });
    }

    public override string? GetName(ItemRow row)
    {
        return row.NameString;
    }

    public override string? GetSubtitle(ItemRow row)
    {
        return row.Base.ItemSearchCategory.ValueNullable?.Name.ToImGuiString();
    }

    public override (string?, uint?) GetIcon(ItemRow row)
    {
        return (null, row.Icon);
    }

    public override ItemRow? GetRow(uint row)
    {
        if (row == 0)
        {
            return null;
        }
        return _itemSheet.GetRow(row);
    }

    public override List<ItemRow> GetRows()
    {
        return _itemSheet.Where(c => c.NameString != string.Empty).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<ItemRow> builder)
    {
        builder.AddCompendiumOpenViewColumn(new() { Key = "icon", Name = "##Icon", HelpText = "The icon of the leve", Version = "14.0.3", ValueSelector = this.GetIcon, CompendiumType = this, RowIdSelector = row => row.RowId });
        builder.AddStringColumn(new() { Key = "name", Name = "Name", HelpText = "The name of the leve", Version = "14.0.3", ValueSelector = row => row.NameString });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, ItemRow row)
    {
        viewBuilder.SetupDefaults(this, row);
    }

    public override bool HasRow(uint rowId)
    {
        if (rowId == 0)
        {
            return false;
        }
        return _itemSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<Type>? RelatedTypes => [typeof(Item)];

    public override string Singular => "Item";
    public override string Plural => "Items";
    public override string Description => "All the items available in the game";
    public override string Key => "items";
    public override (string?, uint?) Icon => (null, Icons.QuestionMarkBag);
}
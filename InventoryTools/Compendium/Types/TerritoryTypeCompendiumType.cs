using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using DalaMock.Host.Mediator;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using Lumina.Excel.Sheets;
using Icons = AllaganLib.Shared.Misc.Icons;

namespace InventoryTools.Compendium.Types;

public class TerritoryTypeCompendiumType : CompendiumType<IGrouping<string, TerritoryTypeRow>>
{
    private readonly TerritoryTypeSheet _territoryTypeSheet;

    public TerritoryTypeCompendiumType(TerritoryTypeSheet territoryTypeSheet, CompendiumTable<IGrouping<string, TerritoryTypeRow>>.Factory tableFactory, Func<CompendiumColumnBuilder<IGrouping<string, TerritoryTypeRow>>> columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _territoryTypeSheet = territoryTypeSheet;
    }

    public override List<Type>? RelatedTypes => [typeof(TerritoryType), typeof(TerritoryTypeRow)];

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new CompendiumTableOptions<IGrouping<string, TerritoryTypeRow>>()
        {
            Columns = BuiltColumns(),
            CompendiumType = this,
            Key = "territory_types",
            Name = "Territory Types",
        });
    }

    public override string? GetName(IGrouping<string, TerritoryTypeRow> row)
    {
        return row.First().Map?.FormattedName ?? "";
    }

    public override string? GetSubtitle(IGrouping<string, TerritoryTypeRow> row)
    {
        return row.First().Map?.FormattedName;
    }

    public override (string?, uint?) GetIcon(IGrouping<string, TerritoryTypeRow> row)
    {
        return (null, Icons.FlagIcon);
    }

    public override IGrouping<string, TerritoryTypeRow>? GetRow(uint row)
    {
        return this.GetRows().FirstOrDefault(c => c.Any(d => d.RowId == row));
    }

    public override List<IGrouping<string, TerritoryTypeRow>> GetRows()
    {
        return _territoryTypeSheet.Where(c => c.Base.TerritoryIntendedUse.RowId != 0 && c.Base.Map.RowId != 0).GroupBy(c => c.Base.Bg.ToImGuiString()).ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<IGrouping<string, TerritoryTypeRow>> builder)
    {
        builder.AddCompendiumOpenViewColumn(new()
        {
            Key = "icon",
            Name = "Icon",
            HelpText = "Territory icon",
            Version = "14.0.3",
            CompendiumType = this,
            RowIdSelector = row => row.First().RowId,
            ValueSelector = GetIcon
        });

        builder.AddStringColumn(new()
        {
            Key = "name",
            Name = "Name",
            HelpText = "The name of the territory",
            Version = "14.0.3",
            ValueSelector = GetName
        });
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, IGrouping<string, TerritoryTypeRow> row)
    {
        viewBuilder.SetupDefaults(this, row);
    }

    public override bool HasRow(uint rowId)
    {
        return _territoryTypeSheet.GetRowOrDefault(rowId) != null;
    }

    public override bool ShowInListing => false;

    public override string Singular => "Territory";
    public override string Plural => "Territories";
    public override string Description => "Territories available in the game";
    public override string Key => "territories";
    public override (string?, uint?) Icon => (null, Icons.FlagIcon);
}
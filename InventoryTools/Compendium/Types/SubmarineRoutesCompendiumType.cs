using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Extensions;
using AllaganLib.GameSheets.Model;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Interfaces;
using InventoryTools.Compendium.Models;
using InventoryTools.Compendium.Sections;
using InventoryTools.Compendium.Sections.Options;
using InventoryTools.Compendium.Services;
using Lumina.Excel.Sheets;

namespace InventoryTools.Compendium.Types;

public class SubmarineRoutesCompendiumType : CompendiumType<SubmarineExplorationRow>
{
    private readonly SubmarineExplorationSheet _submarineExplorationSheet;

    public SubmarineRoutesCompendiumType(SubmarineExplorationSheet submarineExplorationSheet, CompendiumTable<SubmarineExplorationRow>.Factory tableFactory, CompendiumColumnBuilder<SubmarineExplorationRow>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _submarineExplorationSheet = submarineExplorationSheet;
    }

    public override string? GetName(SubmarineExplorationRow row)
    {
        return row.Base.Destination.ToImGuiString();
    }

    public override string? GetSubtitle(SubmarineExplorationRow row)
    {
        return null;
    }

    public override (string?, uint?) GetIcon(SubmarineExplorationRow row)
    {
        return (null, Icons.SubmarineIcon);
    }

    public override uint GetRowId(SubmarineExplorationRow row)
    {
        return row.RowId;
    }

    public override SubmarineExplorationRow? GetRow(uint row)
    {
        return _submarineExplorationSheet.GetRowOrDefault(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _submarineExplorationSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<SubmarineExplorationRow> GetRows()
    {
        return _submarineExplorationSheet.Where(c => c.Base.RowId != 0 && c.Base.Destination.ToImGuiString() != "").ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<SubmarineExplorationRow> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "Icon", HelpText = "The icon of the route", Version = "14.0.3", CompendiumType = this, RowIdSelector = row => row.RowId, ValueSelector = this.GetIcon});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the route", Version = "14.0.3", ValueSelector = row => row.Base.Destination.ToImGuiString()});
        builder.AddStringColumn(new (){Key = "unlock", Name = "Unlock Route", HelpText = "The name of the route that unlocks this", Version = "14.0.3", ValueSelector = row => row.Unlock?.Base.Destination.ToImGuiString() ?? ""});
        builder.AddIntegerColumn(new(){Key = "rankrequired", Name = "Rank Required", HelpText = "The rank required for the route", Version = "14.0.3", ValueSelector =row => row.Base.RankReq.ToString()});
        builder.AddIntegerColumn(new(){Key = "cerelumrequired", Name = "Ceruleum Required", HelpText = "The ceruleum required for the route", Version = "14.0.3", ValueSelector =row => row.Base.CeruleumTankReq.ToString()});
        builder.AddItemsColumn(new(){Key = "drops", Name = "Drops", HelpText = "The drops for this submarine route", Version = "14.0.3", ValueSelector = row => row.DropItems, ColumnFlags = ImGuiTableColumnFlags.WidthFixed});
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, SubmarineExplorationRow row)
    {
        viewBuilder.SetupDefaults(this, row);
        var information = new List<(string Header, string Value, bool IsVisible)>
        {
            ("Rank Req.", row.Base.RankReq.ToString(), true),
            ("Ceruleum Req.", row.Base.CeruleumTankReq.ToString(), true),
        };
        viewBuilder.AddInfoTableSection(new InfoTableSectionOptions()
        {
            SectionName = "Information",
            Items = information.AsReadOnly()
        });
        if (row.Unlock != null)
        {
            viewBuilder.AddSingleRowRefSection(new SingleRowRefSectionOptions()
            {
                SectionName = "Unlocked Via",
                RelatedRef = row.Unlock.Base.AsUntypedRowRef()
            });
        }
        viewBuilder.AddCollectionRowRefSection(new CollectionRowRefSectionOptions()
        {
            RelatedRefs = _submarineExplorationSheet.Where(c => c.UnlockId != null && c.UnlockId == row.RowId).Select(c => c.Base.AsUntypedRowRef()).ToList(),
            SectionName = "Unlocks",
            HideIfEmpty = true
        });

        viewBuilder.AddItemListSection(new ItemListSectionOptions()
        {
            Items = row.DropItems.Select(c => new ItemInfo(c)).ToList(),
            SectionName = "Potential Drops"
        });
    }

    public override ICompendiumTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "submarines",
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
        });
    }

    public override List<Type>? RelatedTypes => [typeof(SubmarineExploration)];

    public override string Singular => "Submarine Route";
    public override string Plural => "Submarine Routes";
    public override string Description => "Routes traversed by company submarines";
    public override string Key => "submarines";
    public override (string?, uint?) Icon => (null, Icons.SubmarineIcon);
}
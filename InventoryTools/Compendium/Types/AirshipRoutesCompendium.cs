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

public class AirshipRoutesCompendiumType : CompendiumType<AirshipExplorationPointRow>
{
    private readonly AirshipExplorationPointSheet _airshipExplorationPointSheet;

    public AirshipRoutesCompendiumType(AirshipExplorationPointSheet airshipExplorationPointSheet, CompendiumTable<AirshipExplorationPointRow>.Factory tableFactory, CompendiumColumnBuilder<AirshipExplorationPointRow>.Factory columnBuilder, CompendiumViewBuilder.Factory viewBuilderFactory) : base(tableFactory, columnBuilder, viewBuilderFactory)
    {
        _airshipExplorationPointSheet = airshipExplorationPointSheet;
    }

    public override string? GetName(AirshipExplorationPointRow row)
    {
        return row.Base.Name.ToImGuiString();
    }

    public override string? GetSubtitle(AirshipExplorationPointRow row)
    {
        return null;
    }

    public override (string?, uint?) GetIcon(AirshipExplorationPointRow row)
    {
        return (null, Icons.AirshipIcon);
    }

    public override uint GetRowId(AirshipExplorationPointRow row)
    {
        return row.RowId;
    }

    public override AirshipExplorationPointRow? GetRow(uint row)
    {
        return _airshipExplorationPointSheet.GetRowOrDefault(row);
    }

    public override bool HasRow(uint rowId)
    {
        return _airshipExplorationPointSheet.GetRowOrDefault(rowId) != null;
    }

    public override List<AirshipExplorationPointRow> GetRows()
    {
        return _airshipExplorationPointSheet.Where(c => c.Base.RowId != 0 && c.Base.Name.ToImGuiString() != "").ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<AirshipExplorationPointRow> builder)
    {
        builder.AddCompendiumOpenViewColumn(new(){Key = "icon", Name = "Icon", HelpText = "The icon of the route", Version = "14.0.3", CompendiumType = this, RowIdSelector = row => row.RowId, ValueSelector = this.GetIcon});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the route", Version = "14.0.3", ValueSelector = row => row.Base.Name.ToImGuiString()});
        builder.AddStringColumn(new (){Key = "unlock", Name = "Unlock Route", HelpText = "The name of the route that unlocks this", Version = "14.0.3", ValueSelector = row => row.Unlock?.Base.Name.ToImGuiString() ?? ""});
        builder.AddIntegerColumn(new(){Key = "rankrequired", Name = "Rank Required", HelpText = "The rank required for the route", Version = "14.0.3", ValueSelector =row => row.Base.RankReq.ToString()});
        builder.AddIntegerColumn(new(){Key = "cerelumrequired", Name = "Ceruleum Required", HelpText = "The ceruleum required for the route", Version = "14.0.3", ValueSelector =row => row.Base.CeruleumTankReq.ToString()});
        builder.AddIntegerColumn(new(){Key = "surveillancerequired", Name = "Surveillance Required", HelpText = "The surveillance required for the route", Version = "14.0.3", ValueSelector =row => row.Base.SurveillanceReq.ToString()});
        builder.AddItemsColumn(new(){Key = "drops", Name = "Drops", HelpText = "The drops for this airship route", Version = "14.0.3", ValueSelector = row => row.DropItems, ColumnFlags = ImGuiTableColumnFlags.WidthFixed});
    }

    public override void BuildViewFields(CompendiumViewBuilder viewBuilder, AirshipExplorationPointRow row)
    {
        viewBuilder.SetupDefaults(this, row);
        var information = new List<(string Header, string Value, bool IsVisible)>
        {
            ("Rank Req.", row.Base.RankReq.ToString(), true),
            ("Ceruleum Req.", row.Base.CeruleumTankReq.ToString(), true),
            ("Surveillance Req.", row.Base.SurveillanceReq.ToString(), true)
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
            RelatedRefs = _airshipExplorationPointSheet.Where(c => c.UnlockId != null && c.UnlockId == row.RowId).Select(c => c.Base.AsUntypedRowRef()).ToList(),
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
            Key = "airships",
            Name = Plural,
            Columns = BuiltColumns,
            CompendiumType = this,
        });
    }

    public override List<Type>? RelatedTypes => [typeof(AirshipExplorationPoint)];

    public override string Singular => "Airship Route";
    public override string Plural => "Airship Routes";
    public override string Description => "Routes flown by Company Airships";
    public override string Key => "airships";
    public override (string?, uint?) Icon => (null, Icons.AirshipIcon);
}
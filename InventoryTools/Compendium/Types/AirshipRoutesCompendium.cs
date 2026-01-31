using System;
using System.Collections.Generic;
using System.Linq;
using AllaganLib.GameSheets.Caches;
using AllaganLib.GameSheets.Sheets;
using AllaganLib.GameSheets.Sheets.Rows;
using AllaganLib.Interface.Grid;
using AllaganLib.Shared.Extensions;
using AllaganLib.Shared.Misc;
using DalaMock.Host.Mediator;
using Dalamud.Bindings.ImGui;
using InventoryTools.Compendium.Models;

namespace InventoryTools.Compendium.Types;

public class AirshipRoutesCompendium : CompendiumType<AirshipExplorationPointRow>
{
    private readonly AirshipExplorationPointSheet _airshipExplorationPointSheet;
    private readonly ItemInfoCache _itemInfoCache;

    public AirshipRoutesCompendium(AirshipExplorationPointSheet airshipExplorationPointSheet, CompendiumTable<AirshipExplorationPointRow>.Factory tableFactory, Func<CompendiumColumnBuilder<AirshipExplorationPointRow>> columnBuilder, ItemInfoCache itemInfoCache) : base(tableFactory, columnBuilder)
    {
        _airshipExplorationPointSheet = airshipExplorationPointSheet;
        _itemInfoCache = itemInfoCache;
    }

    public override AirshipExplorationPointRow? GetRow(uint row)
    {
        return _airshipExplorationPointSheet.GetRowOrDefault(row);
    }

    public override List<AirshipExplorationPointRow> GetRows()
    {
        return _airshipExplorationPointSheet.Where(c => c.Base.RowId != 0 && c.Base.Name.ToImGuiString() != "").ToList();
    }

    public override void BuildColumns(CompendiumColumnBuilder<AirshipExplorationPointRow> builder)
    {
        builder.AddIconColumn(new(){Key = "icon", Name = "Icon", HelpText = "The icon of the route", Version = "14.0.3", ValueSelector = row => Icons.AirshipIcon});
        builder.AddStringColumn(new (){Key = "name", Name = "Name", HelpText = "The name of the route", Version = "14.0.3", ValueSelector = row => row.Base.Name.ToImGuiString()});
        builder.AddStringColumn(new (){Key = "unlock", Name = "Unlock Route", HelpText = "The name of the route that unlocks this", Version = "14.0.3", ValueSelector = row => row.Unlock?.Base.Name.ToImGuiString() ?? ""});
        builder.AddIntegerColumn(new(){Key = "rankrequired", Name = "Rank Required", HelpText = "The rank required for the route", Version = "14.0.3", ValueSelector =row => row.Base.RankReq.ToString()});
        builder.AddIntegerColumn(new(){Key = "cerelumrequired", Name = "Ceruleum Required", HelpText = "The ceruleum required for the route", Version = "14.0.3", ValueSelector =row => row.Base.CeruleumTankReq.ToString()});
        builder.AddIntegerColumn(new(){Key = "surveillancerequired", Name = "Surveillance Required", HelpText = "The surveillance required for the route", Version = "14.0.3", ValueSelector =row => row.Base.SurveillanceReq.ToString()});
        builder.AddItemsColumn(new(){Key = "drops", Name = "Drops", HelpText = "The drops for this airship route", Version = "14.0.3", ValueSelector = row => row.DropItems, ColumnFlags = ImGuiTableColumnFlags.WidthFixed});
    }

    public override IRenderTable<WindowState, MessageBase> BuildTable()
    {
        return Factory.Invoke(new()
        {
            Key = "airships",
            Name = Plural,
            Columns = BuiltColumns(),
            CompendiumType = this,
        });
    }

    public override string Singular => "Airship Route";
    public override string Plural => "Airship Routes";
}